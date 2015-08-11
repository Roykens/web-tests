﻿//
// HttpsTestRunner.cs
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
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Constraints;
using Xamarin.AsyncTests.Portable;
using Xamarin.AsyncTests.Framework;

namespace Xamarin.WebTests.TestRunners
{
	using ConnectionFramework;
	using HttpFramework;
	using HttpHandlers;
	using Features;
	using Providers;
	using Portable;
	using Resources;

	[FriendlyName ("[HttpsTestRunner]")]
	public class HttpsTestRunner : HttpServer
	{
		public ClientAndServerParameters Parameters {
			get;
			private set;
		}

		public ClientParameters ClientParameters {
			get { return Parameters.ClientParameters; }
		}

		public HttpsTestRunner (IHttpProvider provider, IPortableEndPoint endpoint, ListenerFlags flags, ClientAndServerParameters parameters)
			: base (provider, endpoint, flags, parameters.ServerParameters)
		{
			Parameters = parameters;
		}

		public Task Run (TestContext ctx, CancellationToken cancellationToken)
		{
			var handler = new HelloWorldHandler ("hello");
			return Run (ctx, handler, cancellationToken);
		}

		public Task Run (TestContext ctx, Handler handler, CancellationToken cancellationToken)
		{
			var impl = new TestRunnerImpl (this, handler);
			if ((ClientParameters.Flags & ClientFlags.ExpectTrustFailure) != 0)
				return impl.Run (ctx, cancellationToken, HttpStatusCode.InternalServerError, WebExceptionStatus.TrustFailure);
			else if ((ClientParameters.Flags & ClientFlags.ExpectWebException) != 0)
				return impl.Run (ctx, cancellationToken, HttpStatusCode.InternalServerError, WebExceptionStatus.AnyErrorStatus);
			else
				return impl.Run (ctx, cancellationToken, HttpStatusCode.OK, WebExceptionStatus.Success);
		}

		protected override HttpConnection CreateConnection (TestContext ctx, Stream stream)
		{
			try {
				var connection = base.CreateConnection (ctx, stream);

				/*
				 * There seems to be some kind of a race condition here.
				 *
				 * When the client aborts the handshake due the a certificate validation failure,
				 * then we either receive an exception during the TLS handshake or the connection
				 * will be closed when the handshake is completed.
				 *
				 */
				var haveReq = connection.HasRequest();
				if ((ServerParameters.Flags & ServerFlags.ClientAbortsHandshake) != 0) {
					ctx.Assert (haveReq, Is.False, "expected client to abort handshake");
					return null;
				} else {
					ctx.Assert (haveReq, Is.True, "expected non-empty request");
				}
				return connection;
			} catch {
				if ((ServerParameters.Flags & ServerFlags.ClientAbortsHandshake) != 0)
					return null;
				throw;
			}
		}

		protected override bool HandleConnection (TestContext ctx, HttpConnection connection)
		{
			ctx.Expect (connection.SslStream.IsAuthenticated, "server is authenticated");
			if ((ServerParameters.Flags & ServerFlags.RequireClientCertificate) != 0)
				ctx.Expect (connection.SslStream.IsMutuallyAuthenticated, "server is mutually authenticated");

			return base.HandleConnection (ctx, connection);
		}

		protected Request CreateRequest (TestContext ctx, Uri uri)
		{
			var webRequest = HttpProvider.CreateWebRequest (uri);
			webRequest.SetKeepAlive (true);

			var request = new TraditionalRequest (webRequest);

			if (ClientParameters.ClientCertificateValidator != null)
				request.Request.InstallCertificateValidator (ClientParameters.ClientCertificateValidator);

			if (ClientParameters.ClientCertificate != null)
				request.Request.SetClientCertificates (new IClientCertificate[] { ClientParameters.ClientCertificate });

			return request;
		}

		protected async Task<Response> RunInner (TestContext ctx, CancellationToken cancellationToken, Request request)
		{
			var traditionalRequest = (TraditionalRequest)request;
			var response = await traditionalRequest.SendAsync (ctx, cancellationToken);

			var provider = DependencyInjector.Get<ICertificateProvider> ();

			var certificate = traditionalRequest.Request.GetCertificate ();
			ctx.Assert (certificate, Is.Not.Null, "certificate");
			ctx.Assert (provider.AreEqual (certificate, ServerParameters.ServerCertificate), "correct certificate");

			var clientCertificate = traditionalRequest.Request.GetClientCertificate ();
			if (((ServerParameters.Flags & (ServerFlags.AskForClientCertificate|ServerFlags.RequireClientCertificate)) != 0) && ClientParameters.ClientCertificate != null) {
				ctx.Assert (clientCertificate, Is.Not.Null, "client certificate");
				ctx.Assert (provider.AreEqual (clientCertificate, ClientParameters.ClientCertificate), "correct client certificate");
			}

			return response;
		}

		class TestRunnerImpl : TestRunner
		{
			readonly HttpsTestRunner runner;

			public TestRunnerImpl (HttpsTestRunner runner, Handler handler)
				: base (runner, handler)
			{
				this.runner = runner;
			}

			protected override Request CreateRequest (TestContext ctx, Uri uri)
			{
				return runner.CreateRequest (ctx, uri);
			}
			protected override Task<Response> RunInner (TestContext ctx, CancellationToken cancellationToken, Request request)
			{
				return runner.RunInner (ctx, cancellationToken, request);
			}
		}
	}
}
