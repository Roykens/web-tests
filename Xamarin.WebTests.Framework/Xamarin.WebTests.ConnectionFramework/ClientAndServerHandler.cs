﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.WebTests.ConnectionFramework
{
	public abstract class ClientAndServerHandler : IConnectionHandler
	{
		public IServer Server {
			get;
			private set;
		}

		public IClient Client {
			get;
			private set;
		}

		public ClientAndServerHandler (IServer server, IClient client)
		{
			Server = server;
			Client = client;
		}

		public bool SupportsCleanShutdown {
			get { return Server.SupportsCleanShutdown && Client.SupportsCleanShutdown; }
		}

		public async Task WaitForConnection ()
		{
			var serverTask = Server.WaitForConnection ();
			var clientTask = Client.WaitForConnection ();

			var t1 = clientTask.ContinueWith (t => {
				if (t.IsFaulted || t.IsCanceled)
					Server.Dispose ();
			});
			var t2 = serverTask.ContinueWith (t => {
				if (t.IsFaulted || t.IsCanceled)
					Client.Dispose ();
			});

			await Task.WhenAll (serverTask, clientTask, t1, t2);
		}

		public async Task Run ()
		{
			await WaitForConnection ();
			var serverWrapper = new StreamWrapper (Server.Stream);
			var clientWrapper = new StreamWrapper (Client.Stream);
			await MainLoop (serverWrapper, clientWrapper);
		}

		protected abstract Task MainLoop (ILineBasedStream serverStream, ILineBasedStream clientStream);

		public async Task<bool> Shutdown (bool attemptCleanShutdown, bool waitForReply)
		{
			var clientShutdown = Client.Shutdown (attemptCleanShutdown, waitForReply);
			var serverShutdown = Server.Shutdown (attemptCleanShutdown, waitForReply);
			await Task.WhenAll (clientShutdown, serverShutdown);
			return clientShutdown.Result && serverShutdown.Result;
		}

		public void Close ()
		{
			Client.Dispose ();
			Server.Dispose ();
		}
	}
}

