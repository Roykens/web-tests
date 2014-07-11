﻿//
// TestServer.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Server
{
	public abstract class Connection
	{
		Stream stream;
		Serializer serializer;
		CancellationTokenSource cancelCts;
		TaskCompletionSource<bool> commandTcs;
		Queue<QueuedCommand> commandQueue;
		bool shutdownRequested;

		public Connection (Stream stream)
		{
			this.stream = stream;
			serializer = new Serializer (this);
			cancelCts = new CancellationTokenSource ();
			commandQueue = new Queue<QueuedCommand> ();
		}

		public int DebugLevel {
			get; set;
		}

		#region Public Client API

		public async Task Hello (CancellationToken cancellationToken)
		{
			var command = new HelloCommand ();
			await SendCommand (command);
		}

		public async Task Debug (int level, string message)
		{
			var command = new DebugCommand { Level = level, Message = message };
			await SendCommand (command);
		}

		public async Task Message (string message)
		{
			var command = new MessageCommand { Message = message };
			await SendCommand (command);
		}

		public async Task SetDebugLevel (int level)
		{
			var command = new SetDebugLevelCommand { Level = level };
			await SendCommand (command);
		}

		public async Task SyncConfiguration (TestConfiguration configuration, bool fullUpdate)
		{
			var command = new SyncConfigurationCommand {
				Configuration = configuration, FullUpdate = fullUpdate
			};
			await SendWithResponse (command);
		}

		public async Task Shutdown ()
		{
			var command = new ShutdownCommand ();
			await SendWithResponse (command);
		}

		#endregion

		internal Serializer Serializer {
			get { return serializer; }
		}

		public virtual void Stop ()
		{
			cancelCts.Cancel ();

			lock (this) {
				foreach (var queued in commandQueue)
					queued.Task.TrySetCanceled ();
				foreach (var operation in operations.Values)
					operation.Task.TrySetCanceled ();
			}
		}

		public ITestLogger GetLogger ()
		{
			return new ServerLogger (this);
		}

		internal Task SendCommand (Command command)
		{
			var queued = new QueuedCommand (command);
			lock (this) {
				if (commandTcs != null) {
					commandQueue.Enqueue (queued);
					return queued.Task.Task;
				}
				commandTcs = queued.Task;
			}

			Task.Factory.StartNew (async () => {
				try {
					await RunQueue (queued);
					queued.Task.SetResult (true);
				} catch (OperationCanceledException) {
					queued.Task.SetCanceled ();
				} catch (Exception ex) {
					queued.Task.SetException (ex);
				}

				await RunQueue ();
			});

			return queued.Task.Task;
		}

		async Task RunQueue ()
		{
			while (true) {
				QueuedCommand command;
				lock (this) {
					if (commandQueue.Count == 0) {
						commandTcs = null;
						return;
					}
					command = commandQueue.Dequeue ();
					commandTcs = command.Task;
				}

				await RunQueue (command);
			}
		}

		async Task RunQueue (QueuedCommand command)
		{
			var formatted = serializer.Write (command.Command);
			var bytes = new UTF8Encoding ().GetBytes (formatted);

			var header = BitConverter.GetBytes (bytes.Length);
			if (bytes.Length == 0)
				throw new InvalidOperationException ();

			await stream.WriteAsync (header, 0, 4).ConfigureAwait (false);
			await stream.FlushAsync ();

			await stream.WriteAsync (bytes, 0, bytes.Length);

			await stream.FlushAsync ();
		}

		internal void SendCommandSync (Command command)
		{
			SendCommand (command).Wait ();
		}

		internal async Task<bool> SendWithResponse (CommandWithResponse command)
		{
			var operation = RegisterResponse ();
			command.ResponseID = operation.ObjectID;
			await SendCommand (command).ConfigureAwait (false);
			return await operation.Task.Task;
		}

		internal static void Debug (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (message, args);
		}

		async Task<byte[]> ReadBuffer (int length)
		{
			var buffer = new byte [length];
			int pos = 0;
			while (pos < length) {
				var ret = await stream.ReadAsync (buffer, pos, length-pos, cancelCts.Token);
				if (ret <= 0)
					throw new InvalidOperationException ();
				pos += ret;
			}
			return buffer;
		}

		protected async Task MainLoop ()
		{
			while (!shutdownRequested && !cancelCts.IsCancellationRequested) {
				var header = await ReadBuffer (4);
				var len = BitConverter.ToInt32 (header, 0);
				if (len == 0)
					return;

				var body = await ReadBuffer (len);
				var content = new UTF8Encoding ().GetString (body, 0, body.Length);

				cancelCts.Token.ThrowIfCancellationRequested ();
				HandleCommand (content);
			}
		}

		async void HandleCommand (string formatted)
		{
			var command = serializer.ReadCommand (formatted);

			cancelCts.Token.ThrowIfCancellationRequested ();

			bool success;
			string error = null;

			try {
				var commonCommand = command as ICommonCommand;
				if (commonCommand != null)
					await commonCommand.Run (this, cancelCts.Token);
				else
					await HandleCommand (command, cancelCts.Token);
				success = true;
			} catch (Exception ex) {
				error = ex.ToString ();
				Debug ("COMMAND FAILED: {0}", ex);
				success = false;
			}

			var withResponse = command as CommandWithResponse;
			if (withResponse == null || withResponse.ResponseID == 0)
				return;

			var response = new ResponseCommand {
				ObjectID = withResponse.ResponseID, Success = success, Error = error
			};

			try {
				await SendCommand (response);
			} catch (Exception ex) {
				Debug ("ERROR WHILE SENDING RESPONSE: {0}", ex);
			}
		}

		internal abstract Task HandleCommand (Command command, CancellationToken cancellationToken);

		internal Task Run (MessageCommand command, CancellationToken cancellationToken)
		{
			OnMessage (command.Message);
			return Task.FromResult<object> (null);
		}

		internal Task Run (DebugCommand command, CancellationToken cancellationToken)
		{
			OnDebug (command.Level, command.Message);
			return Task.FromResult<object> (null);
		}

		internal Task Run (SetDebugLevelCommand command, CancellationToken cancellationToken)
		{
			DebugLevel = command.Level;
			return Task.FromResult<object> (null);
		}

		internal Task Run (HelloCommand command, CancellationToken cancellationToken)
		{
			return OnHello (cancellationToken);
		}

		internal Task Run (SyncConfigurationCommand command, CancellationToken cancellationToken)
		{
			OnSyncConfiguration (command.Configuration, command.FullUpdate);
			return Task.FromResult<object> (null);
		}

		internal Task Run (ShutdownCommand command, CancellationToken cancellationToken)
		{
			shutdownRequested = true;
			return Task.FromResult<object> (null);
		}

		protected abstract Task OnHello (CancellationToken cancellationToken);

		protected abstract void OnMessage (string message);

		protected abstract void OnDebug (int level, string message);

		protected abstract void OnSyncConfiguration (TestConfiguration configuration, bool fullUpdate);

		static long next_id;
		protected long GetNextObjectId ()
		{
			return ++next_id;
		}

		Dictionary<long,Operation> operations = new Dictionary<long, Operation> ();

		internal Operation RegisterResponse ()
		{
			lock (this) {
				var objectID = GetNextObjectId ();
				var operation = new Operation (objectID);
				operations.Add (objectID, operation);
				return operation;
			}
		}

		internal Task Run (ResponseCommand command, CancellationToken cancellationToken)
		{
			lock (this) {
				var operation = operations [command.ObjectID];
				operations.Remove (operation.ObjectID);
				if (command.Error != null)
					operation.Task.SetException (new SavedException (command.Error));
				else
					operation.Task.SetResult (command.Success);
			}
			return Task.FromResult<object> (null);
		}

		internal class Operation
		{
			public readonly long ObjectID;
			public TaskCompletionSource<bool> Task;

			public Operation (long objectID)
			{
				ObjectID = objectID;
				Task = new TaskCompletionSource<bool> ();
			}
		}

		internal class QueuedCommand
		{
			public readonly Command Command;
			public readonly TaskCompletionSource<bool> Task;

			public QueuedCommand (Command command)
			{
				Command = command;
				Task = new TaskCompletionSource<bool> ();
			}
		}
	}
}

