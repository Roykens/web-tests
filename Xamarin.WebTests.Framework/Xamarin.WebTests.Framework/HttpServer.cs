﻿//
// ServerInstance.cs
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
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Constraints;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.WebTests.Framework
{
	using Handlers;
	using Framework;
	using Providers;
	using Portable;

	[FriendlyName ("[HttpServer]")]
	public class HttpServer : ITestInstance
	{
		readonly Uri uri;
		readonly ListenerFlags flags;
		readonly bool ssl;
		readonly IServerCertificate serverCertificate;
		readonly ISslStreamProvider sslStreamProvider;
		readonly IPortableWebSupport WebSupport;

		IPortableEndPoint endpoint;
		IListener listener;

		static long nextId;
		Dictionary<string,Handler> handlers = new Dictionary<string, Handler> ();

		public HttpServer (IPortableEndPoint endpoint, ListenerFlags flags, IServerCertificate serverCertificate = null, ISslStreamProvider sslStreamProvider = null)
		{
			this.endpoint = endpoint;
			this.flags = flags;
			this.serverCertificate = serverCertificate;
			this.sslStreamProvider = sslStreamProvider;
			this.ssl = serverCertificate != null;

			WebSupport = DependencyInjector.Get<IPortableWebSupport> ();

			uri = new Uri (string.Format ("http{0}://{1}:{2}/", ssl ? "s" : "", endpoint.Address, endpoint.Port));
		}

		protected IListener Listener {
			get { return listener; }
		}

		public bool UseSSL {
			get { return ssl; }
		}

		public bool ReuseConnection {
			get { return flags == ListenerFlags.ReuseConnection; }
		}

		public ListenerFlags Flags {
			get { return flags; }
		}

		public IServerCertificate ServerCertificate {
			get { return serverCertificate; }
		}

		public virtual IPortableProxy GetProxy ()
		{
			return null;
		}

		#region ITestInstance implementation

		bool initialized;

		public async Task Initialize (TestContext ctx, CancellationToken cancellationToken)
		{
			if (initialized)
				throw new InvalidOperationException ();
			initialized = true;

			if (ReuseConnection)
				await Start (ctx, cancellationToken);
		}

		public async Task PreRun (TestContext ctx, CancellationToken cancellationToken)
		{
			if (!ReuseConnection)
				await Start (ctx, cancellationToken);
		}

		public async Task PostRun (TestContext ctx, CancellationToken cancellationToken)
		{
			if (!ReuseConnection)
				await Stop (ctx, cancellationToken);
		}

		public async Task Destroy (TestContext ctx, CancellationToken cancellationToken)
		{
			if (!initialized)
				throw new InvalidOperationException ();

			if (ReuseConnection)
				await Stop (ctx, cancellationToken);

			initialized = false;
		}

		#endregion

		public virtual Task Start (TestContext ctx, CancellationToken cancellationToken)
		{
			listener = WebSupport.CreateHttpListener (endpoint, this, Flags, serverCertificate, sslStreamProvider);
			return listener.Start ();
		}

		public virtual Task Stop (TestContext ctx, CancellationToken cancellationToken)
		{
			return Task.Run (() => listener.Stop ());
		}

		public Uri RegisterHandler (Handler handler)
		{
			var path = string.Format ("/{0}/{1}/", handler.GetType (), ++nextId);
			handlers.Add (path, handler);
			return new Uri (uri, path);
		}

		public void RegisterHandler (string path, Handler handler)
		{
			handlers.Add (path, handler);
		}

		public bool HandleConnection (Stream stream)
		{
			var connection = new HttpConnection (this, stream);
			var request = connection.ReadRequest ();

			if (Flags == ListenerFlags.ExpectTrustFailure) {
				if (request != null)
					throw new InvalidOperationException ();
				return false;
			}

			var path = request.Path;
			var handler = handlers [path];
			handlers.Remove (path);

			return handler.HandleRequest (connection, request);
		}

		protected void Debug (TestContext ctx, int level, Handler handler, string message, params object[] args)
		{
			var sb = new StringBuilder ();
			sb.AppendFormat ("{0}:{1}: {2}", this, handler, message);
			for (int i = 0; i < args.Length; i++) {
				sb.Append (" ");
				sb.Append (args [i] != null ? args [i].ToString () : "<null>");
			}

			ctx.LogDebug (level, sb.ToString ());
		}

		protected virtual string MyToString ()
		{
			var sb = new StringBuilder ();
			if (ReuseConnection)
				sb.Append ("shared");
			if (UseSSL) {
				if (sb.Length > 0)
					sb.Append (",");
				sb.Append ("ssl");
			}
			return sb.ToString ();
		}

		public override string ToString ()
		{
			var description = MyToString ();
			var padding = string.IsNullOrEmpty (description) ? string.Empty : ": ";
			return string.Format ("[{0}{1}{2}]", GetType ().Name, padding, description);
		}
	}
}

