﻿//
// ServerHost.cs
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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.AsyncTests.Portable;

namespace Xamarin.AsyncTests.Portable
{
	using Portable;

	public class ServerHost : IServerHost
	{
		public Task<IServerConnection> Listen (IPortableEndPoint endpoint, CancellationToken cancellationToken)
		{
			return Task.Run<IServerConnection> (() => {
				var networkEndpoint = PortableSupportImpl.GetEndpoint (endpoint);
				var listener = new TcpListener (networkEndpoint);
				listener.Start ();
				Debug.WriteLine ("Server started: {0}", listener.LocalEndpoint);
				return new ServerConnection (listener);
			});
		}

		public async Task<IServerConnection> Connect (IPortableEndPoint endpoint, CancellationToken cancellationToken)
		{
			var networkEndpoint = PortableSupportImpl.GetEndpoint (endpoint);
			var client = new TcpClient ();
			await client.ConnectAsync (networkEndpoint.Address, networkEndpoint.Port);
			return new ClientConnection (client);
		}

		class PipeConnection : IPipeConnection
		{
			public IServerConnection Client {
				get;
				private set;
			}

			public IServerConnection Server {
				get;
				private set;
			}

			public PipeConnection (IServerConnection client, IServerConnection server)
			{
				Client = client;
				Server = server;
			}
		}

		class ClientConnection : IServerConnection
		{
			TcpClient client;
			NetworkStream stream;

			public ClientConnection (TcpClient client)
			{
				this.client = client;

				Name = client.Client.RemoteEndPoint.ToString ();
			}

			public string Name {
				get;
				private set;
			}

			public Task<Stream> Open (CancellationToken cancellationToken)
			{
				return Task.Run<Stream> (() => {
					stream = client.GetStream ();
					return stream;
				});
			}


			public void Close ()
			{
				try {
					if (stream != null) {
						stream.Close ();
						stream = null;
					}
				} catch {
					;
				}
				try {
					if (client != null) {
						client.Close ();
						client = null;
					}
				} catch {
					;
				}
			}

		}

		class ServerConnection : IServerConnection
		{
			TcpListener listener;
			Socket socket;
			NetworkStream stream;

			public ServerConnection (TcpListener listener)
			{
				this.listener = listener;
				Name = listener.LocalEndpoint.ToString ();
			}

			public string Name {
				get;
				private set;
			}

			public async Task<Stream> Open (CancellationToken cancellationToken)
			{
				var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
				cts.Token.Register (() => listener.Stop ());

				socket = await listener.AcceptSocketAsync ();
				cts.Token.ThrowIfCancellationRequested ();

				Debug.WriteLine ("Server accepted connection from {0}.", socket.RemoteEndPoint);
				stream = new NetworkStream (socket);
				return stream;
			}

			public void Close ()
			{
				try {
					if (socket != null) {
						socket.Shutdown (SocketShutdown.Both);
						socket.Dispose ();
						socket = null;
					}
				} catch {
					;
				}
				try {
					if (stream != null) {
						stream.Close ();
						stream = null;
					}
				} catch {
					;
				}
				try {
					listener.Stop ();
				} catch {
					;
				}
			}
		}
	}
}

