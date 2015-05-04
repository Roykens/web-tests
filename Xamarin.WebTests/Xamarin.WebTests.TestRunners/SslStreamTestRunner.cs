﻿//
// SslStreamTestRunner.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
using System.Threading;
using System.Threading.Tasks;
using Xamarin.AsyncTests;

namespace Xamarin.WebTests.TestRunners
{
	using ConnectionFramework;

	public class SslStreamTestRunner : ClientAndServer
	{
		public SslStreamTestRunner (IServer server, IClient client)
			: base (server, client)
		{
		}

		public async Task Run (TestContext ctx, CancellationToken cancellationToken)
		{
			await WaitForConnection (ctx, cancellationToken);

			var serverWrapper = new StreamWrapper (Server.Stream);
			var clientWrapper = new StreamWrapper (Client.Stream);
			await MainLoop (ctx, serverWrapper, clientWrapper, cancellationToken);
		}

		protected async Task MainLoop (TestContext ctx, ILineBasedStream serverStream, ILineBasedStream clientStream, CancellationToken cancellationToken)
		{
			await serverStream.WriteLineAsync ("SERVER OK");
			var line = await clientStream.ReadLineAsync ();
			if (!line.Equals ("SERVER OK"))
				throw new ConnectionException ("Got unexpected output from server: '{0}'", line);
			await clientStream.WriteLineAsync ("CLIENT OK");
			line = await serverStream.ReadLineAsync ();
			if (!line.Equals ("CLIENT OK"))
				throw new ConnectionException ("Got unexpected output from client: '{0}'", line);
			await Shutdown (ctx, SupportsCleanShutdown, true, cancellationToken);
		}
	}
}

