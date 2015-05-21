﻿//
// TraditionalRequest.cs
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
using System.Threading;
using System.Threading.Tasks;

using Xamarin.AsyncTests;

namespace Xamarin.WebTests.HttpHandlers
{
	using HttpFramework;
	using Portable;
	using Providers;

	public class TraditionalRequest : Request
	{
		public readonly IHttpWebRequest Request;

		public TraditionalRequest (Uri uri)
		{
			var factory = DependencyInjector.Get<ConnectionProviderFactory> ();
			var provider = factory.DefaultHttpProvider;

			Request = provider.CreateWebRequest (uri);
			Request.SetKeepAlive (true);
		}

		public TraditionalRequest (HttpWebRequest request)
		{
			var factory = DependencyInjector.Get<ConnectionProviderFactory> ();
			var provider = factory.DefaultHttpProvider;

			Request = provider.CreateWebRequest (request);
			Request.SetKeepAlive (true);
		}

		public TraditionalRequest (IHttpWebRequest request)
		{
			Request = request;
		}

		#region implemented abstract members of Request

		public override void SetCredentials (ICredentials credentials)
		{
			Request.Request.Credentials = credentials;
		}

		public override void SetProxy (IPortableProxy proxy)
		{
			Request.SetProxy (proxy);
		}

		public override string Method {
			get { return Request.Request.Method; }
			set { Request.Request.Method = value; }
		}

		public override void SetContentLength (long contentLength)
		{
			Request.SetContentLength (contentLength);
		}

		public override void SetContentType (string contentType)
		{
			Request.Request.ContentType = contentType;
		}

		public override void SendChunked ()
		{
			Request.SetSendChunked (true);
		}

		public async Task<Response> Send (TestContext ctx, CancellationToken cancellationToken)
		{
			var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			cts.Token.Register (() => Request.Request.Abort ());

			var task = Task.Factory.StartNew (() => {
				return Send (ctx);
			});

			try {
				return await task;
			} finally {
				cts.Dispose ();
			}
		}

		public override async Task<Response> SendAsync (TestContext ctx, CancellationToken cancellationToken)
		{
			var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			cts.Token.Register (() => Request.Request.Abort ());

			try {
				if (Content != null) {
					using (var stream = await Request.GetRequestStreamAsync ()) {
						using (var writer = new StreamWriter (stream)) {
							await Content.WriteToAsync (writer);
							await writer.FlushAsync ();
						}
					}
				}

				var response = await Request.GetResponseAsync ();
				return await GetResponseFromHttp (ctx, response, null, cancellationToken);
			} catch (WebException wexc) {
				var response = (HttpWebResponse)wexc.Response;
				if (response == null)
					return new SimpleResponse (this, HttpStatusCode.InternalServerError, null, wexc);

				return GetResponseFromHttpSync (ctx, response, wexc);
			} catch (Exception ex) {
				return new SimpleResponse (this, HttpStatusCode.InternalServerError, null, ex);
			} finally {
				cts.Dispose ();
			}
		}

		Response GetResponseFromHttpSync (TestContext ctx, HttpWebResponse response, WebException error)
		{
			return GetResponseFromHttp (ctx, response, error, CancellationToken.None).Result;
		}

		protected async virtual Task<Response> GetResponseFromHttp (TestContext ctx, HttpWebResponse response, WebException error, CancellationToken cancellationToken)
		{
			string content;
			var status = response.StatusCode;

			using (var reader = new StreamReader (response.GetResponseStream ())) {
				content = await reader.ReadToEndAsync ().ConfigureAwait (false);
			}
			response.Dispose ();

			return new SimpleResponse (this, status, StringContent.CreateMaybeNull (content), error);
		}

		Response Send (TestContext ctx)
		{
			try {
				if (Content != null) {
					using (var writer = new StreamWriter (Request.GetRequestStream ())) {
						Content.WriteToAsync (writer).Wait ();
					}
				}

				var response = Request.GetResponse ();
				return GetResponseFromHttpSync (ctx, response, null);
			} catch (WebException wexc) {
				var response = (HttpWebResponse)wexc.Response;
				if (response == null)
					return new SimpleResponse (this, HttpStatusCode.InternalServerError, null, wexc);

				return GetResponseFromHttpSync (ctx, response, wexc);
			} catch (Exception ex) {
				return new SimpleResponse (this, HttpStatusCode.InternalServerError, null, ex);
			}
		}

		#endregion
	}
}

