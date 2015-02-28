﻿//
// ServerConnection.cs
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

namespace Xamarin.AsyncTests.Server
{
	using Portable;
	using Framework;

	public class ServerConnection : Connection
	{
		IServerConnection connection;
		TaskCompletionSource<object> helloTcs;
		TestLogger logger;

		public TestLogger Logger {
			get { return logger; }
		}

		public ServerConnection (TestApp context, Stream stream, IServerConnection connection)
			: base (context, stream)
		{
			this.connection = connection;
			helloTcs = new TaskCompletionSource<object> ();
		}

		public async Task StartServer (CancellationToken cancellationToken)
		{
			await Start (cancellationToken);
			await helloTcs.Task;
		}

		internal async Task OnHello (Handshake handshake, CancellationToken cancellationToken)
		{
			Debug ("Server Handshake: {0}", handshake);

			await Task.Yield ();

			var loggerClient = await EventSinkClient.FromProxy (handshake.EventSink, cancellationToken);

			lock (this) {
				if (handshake.Settings != null)
					App.Settings.Merge (handshake.Settings);

				logger = new TestLogger (loggerClient.Backend);
				logger.LogMessage ("Hello from Server!");

				helloTcs.SetResult (null);
			}

			Debug ("Server Handshake done");
		}

		public override void Stop ()
		{
			try {
				base.Stop ();
			} catch {
				;
			}

			try {
				if (connection != null) {
					connection.Close ();
					connection = null;
				}
			} catch {
				;
			}
		}
	}
}

