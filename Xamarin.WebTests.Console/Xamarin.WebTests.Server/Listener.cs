﻿//
// Listener.cs
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
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SD = System.Diagnostics;

using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.WebTests.Server
{
	using Portable;
	using Providers;

	abstract class Listener : IListener
	{
		Socket server;
		int currentConnections;
		volatile Exception currentError;
		volatile TaskCompletionSource<bool> tcs;
		volatile CancellationTokenSource cts;

		readonly IHttpProvider provider;
		readonly IServerCertificate serverCertificate;
		readonly ListenerFlags flags;
		readonly bool ssl;
		readonly Uri uri;

		public Listener (IHttpProvider provider, IPortableEndPoint endpoint, ListenerFlags flags, IServerCertificate serverCertificate)
		{
			this.provider = provider;
			this.flags = flags;
			this.serverCertificate = serverCertificate;
			this.ssl = serverCertificate != null;

			if (ssl) {
				if ((flags & ListenerFlags.SSLIncompatibleMask) != 0)
					throw new InvalidOperationException ();
			} else {
				if ((flags & ListenerFlags.SSLMask) != 0)
					throw new InvalidOperationException ();
			}

			var address = IPAddress.Parse (endpoint.Address);
			var networkEndpoint = new IPEndPoint (address, endpoint.Port);

			uri = new Uri (string.Format ("http{0}://{1}:{2}/", ssl ? "s" : "", endpoint.Address, endpoint.Port));

			server = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind (networkEndpoint);
			server.Listen (1);
		}

		public IHttpProvider HttpProvider {
			get { return provider; }
		}

		public Uri Uri {
			get { return uri; }
		}

		static void Debug (string message, params object[] args)
		{
			SD.Debug.WriteLine (message, args);
		}

		public Task Start ()
		{
			lock (this) {
				if (cts != null)
					throw new InvalidOperationException ();

				cts = new CancellationTokenSource ();
				tcs = new TaskCompletionSource<bool> ();
			}

			return Task.Run (() => {
				Listen ();
			});
		}

		void Listen ()
		{
			var args = new SocketAsyncEventArgs ();
			args.Completed += (sender, e) => OnAccepted (e);

			Interlocked.Increment (ref currentConnections);

			try {
				var retval = server.AcceptAsync (args);
				if (retval)
					return;
				throw new InvalidOperationException ();
			} catch (Exception ex) {
				OnException (ex);
				OnFinished ();
				throw;
			}
		}

		void OnException (Exception error)
		{
			lock (this) {
				if (currentError == null) {
					currentError = error;
					return;
				}

				var aggregated = currentError as AggregateException;
				if (aggregated == null) {
					currentError = new AggregateException (error);
					return;
				}

				var inner = aggregated.InnerExceptions.ToList ();
				inner.Add (error);
				currentError = new AggregateException (inner);
			}
		}

		void OnAccepted (SocketAsyncEventArgs args)
		{
			if (cts.IsCancellationRequested) {
				OnFinished ();
				args.Dispose ();
				return;
			} else if (args.SocketError != SocketError.Success) {
				var error = new IOException (string.Format ("Accept failed: {0}", args.SocketError));
				OnException (error);
				args.Dispose ();
				return;
			}

			try {
				Listen ();
			} catch {
				return;
			}

			var socket = args.AcceptSocket;
			Stream stream;

			try {
				stream = CreateStream (socket);
			} catch (OperationCanceledException) {
				stream = null;
			} catch (Exception ex) {
				if (!cts.IsCancellationRequested)
					OnException (ex);
				stream = null;
			}

			try {
				if (stream != null)
					HandleConnection_internal (socket, stream, cts.Token);
				Close (socket);
			} catch (OperationCanceledException) {
				;
			} catch (Exception ex) {
				OnException (ex);
			}

			OnFinished ();
			args.Dispose ();
		}

		void OnFinished ()
		{
			lock (this) {
				var connections = Interlocked.Decrement (ref currentConnections);

				if (connections > 0)
					return;

				if (currentError != null)
					tcs.SetException (currentError);
				else
					tcs.SetResult (true);
			}
		}

		public async Task Stop ()
		{
			cts.Cancel ();
			if (server.Connected)
				server.Shutdown (SocketShutdown.Both);
			server.Close ();
			await tcs.Task;
			OnStop ();

			lock (this) {
				cts.Dispose ();
				cts = null;
				tcs = null;
			}
		}

		protected virtual void OnStop ()
		{
		}

		void Close (Socket socket)
		{
			try {
				socket.Shutdown (SocketShutdown.Both);
			} catch {
				;
			} finally {
				socket.Close ();
				socket.Dispose ();
			}
		}

		Stream CreateStream (Socket socket)
		{
			var stream = new NetworkStream (socket);
			if (!ssl)
				return stream;

			try {
				var sslStreamProvider = provider.SslStreamProvider ?? provider.DefaultSslStreamProvider;

				ICertificateValidator validator;
				if ((flags & ListenerFlags.RejectClientCertificate) != 0)
					validator = CertificateProvider.RejectAll;
				else
					validator = CertificateProvider.AcceptAll;

				var authenticatedStream = sslStreamProvider.CreateServerStream (stream, serverCertificate, validator, flags);

				if ((flags & ListenerFlags.ExpectTrustFailure) != 0)
					throw new InvalidOperationException ("Expected TLS Trust Failure error.");

				return authenticatedStream;
			} catch {
				if ((flags & ListenerFlags.SSLErrorMask) != 0)
					return null;
				throw;
			}
		}

		bool IsStillConnected (Socket socket)
		{
			try {
				if (!socket.Poll (-1, SelectMode.SelectRead))
					return false;
				return socket.Available > 0;
			} catch {
				return false;
			}
		}

		void HandleConnection_internal (Socket socket, Stream stream, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				var wantToReuse = HandleConnection (socket, stream, cancellationToken);
				if (!wantToReuse || cancellationToken.IsCancellationRequested)
					break;

				bool connectionAvailable = IsStillConnected (socket);
				if (!connectionAvailable && !cts.IsCancellationRequested)
					throw new InvalidOperationException ("Expecting another connection, but socket has been shut down.");
			}
		}

		protected abstract bool HandleConnection (Socket socket, Stream stream, CancellationToken cancellationToken);
	}
}
