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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Xamarin.AsyncTests.UI
{
	using Framework;
	using Server;

	public class TestServer : Connection
	{
		public TestApp App {
			get;
			private set;
		}

		public IServerConnection Connection {
			get;
			private set;
		}

		public bool IsServer {
			get;
			private set;
		}

		public TestServer (TestApp app, Stream stream, IServerConnection connection, bool isServer)
			: base (app, stream)
		{
			App = app;
			Connection = connection;
			IsServer = isServer;
		}

		public async Task Run (CancellationToken cancellationToken)
		{
			var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			cts.Token.Register (() => {
				stopRequested = true;
				Stop ();
			});

			var tcs = new TaskCompletionSource<object> ();

			await Task.Factory.StartNew (async () => {
				try {
					await MainLoop ();
					tcs.SetResult (null);
				} catch (Exception ex) {
					tcs.SetException (ex);
				}
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext ());

			if (!IsServer)
				await Hello (IsServer, cancellationToken);

			try {
				await tcs.Task;
			} catch (Exception ex) {
				if (!stopRequested)
					App.Context.Debug (0, "SERVER ERROR: {0}", ex);
			}
		}

		public override void Stop ()
		{
			stopRequested = true;
			try {
				base.Stop ();
			} catch {
				;
			}
			try {
				Connection.Close ();
			} catch {
				;
			}
		}

		protected override void OnShutdown ()
		{
			stopRequested = true;
			base.OnShutdown ();
		}

		#region implemented abstract members of ServerConnection

		protected override void OnLogMessage (string message)
		{
			Debug ("MESSAGE: {0}", message);
		}

		protected override void OnDebug (int level, string message)
		{
			Debug ("DEBUG ({0}): {1}", level, message);
		}

		bool stopRequested;

		#endregion

		protected override async Task<TestSuite> OnLoadTestSuite (CancellationToken cancellationToken)
		{
			await App.TestSuiteManager.LoadLocal.Execute ();
			return App.TestSuiteManager.Instance;
		}
	}
}

