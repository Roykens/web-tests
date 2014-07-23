﻿//
// HttpClientHandler.cs
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
using System.Net;
using Http = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.AsyncTests;

namespace Xamarin.WebTests.Handlers
{
	using Server;
	using Framework;

	public class HttpClientHandler : Handler
	{
		HttpClientOperation operation;
		HttpContent content;
		HttpContent returnContent;

		public HttpClientOperation Operation {
			get { return operation; }
			set {
				WantToModify ();
				operation = value;
			}
		}

		public HttpContent Content {
			get {
				return content;
			}
			set {
				WantToModify ();
				content = value;
			}
		}

		public HttpContent ReturnContent {
			get {
				return returnContent;
			}
			set {
				WantToModify ();
				returnContent = value;
			}
		}

		public override object Clone ()
		{
			var handler = new HttpClientHandler ();
			handler.operation = operation;
			return handler;
		}

		protected internal override HttpResponse HandleRequest (HttpConnection connection, HttpRequest request, RequestFlags effectiveFlags)
		{
			switch (Operation) {
			case HttpClientOperation.GetString:
				if (!request.Method.Equals ("GET"))
					return HttpResponse.CreateError ("Wrong method: {0}", request.Method);

				return HttpResponse.CreateSuccess (string.Format ("Hello World!"));

			case HttpClientOperation.PostString:
				return HandlePostString (connection, request, effectiveFlags);

			default:
				return HttpResponse.CreateError ("Invalid operation: {0}", operation);
			}
		}

		HttpResponse HandlePostString (HttpConnection connection, HttpRequest request, RequestFlags effectiveFlags)
		{
			var body = request.ReadBody ();

			Debug (0, "BODY", body);
			if ((effectiveFlags & RequestFlags.NoBody) != 0) {
				if (body != null)
					return HttpResponse.CreateError ("Must not send a body with this request.");
				return HttpResponse.CreateSuccess ();
			}

			if (Content != null) {
				if (body == null)
					return HttpResponse.CreateError ("Missing body");
				else if (!Content.AsString ().Equals (body.AsString ()))
					return HttpResponse.CreateError ("Invalid body");
			} else if (body != null) {
				return HttpResponse.CreateError ("Must not have a body");
			}

			return new HttpResponse (HttpStatusCode.OK, returnContent);
		}

		public override Request CreateRequest (Uri uri)
		{
			if (Operation == HttpClientOperation.GetString) {
				if (Content != null)
					throw new InvalidOperationException ();
			} else if (Operation == HttpClientOperation.PostString) {
				if (Content == null)
					throw new InvalidOperationException ();
			} else {
				throw new InvalidOperationException ();
			}

			return new HttpClientRequest (this, uri);
		}

		class HttpClientRequest : Request
		{
			public readonly Uri RequestUri;
			public readonly HttpClientHandler Parent;
			public readonly Http.HttpClientHandler Handler;
			public readonly Http.HttpClient Client;

			public HttpClientRequest (HttpClientHandler parent, Uri uri)
			{
				Parent = parent;
				RequestUri = uri;
				Handler = new Http.HttpClientHandler ();
				Client = new Http.HttpClient (Handler, true);
			}

			public override Task<Response> Send (InvocationContext ctx, CancellationToken cancellationToken)
			{
				switch (Parent.Operation) {
				case HttpClientOperation.GetString:
					return GetString (cancellationToken);
				case HttpClientOperation.PostString:
					return PostString (ctx, cancellationToken);
				default:
					throw new InvalidOperationException ();
				}
			}

			public override void SetProxy (IWebProxy proxy)
			{
				Handler.Proxy = proxy;
			}
			public override void SetCredentials (ICredentials credentials)
			{
				Handler.Credentials = credentials;
			}

			async Task<Response> GetString (CancellationToken cancellationToken)
			{
				var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
				cts.Token.Register (() => Client.CancelPendingRequests ());

				try {
					var body = await Client.GetStringAsync (RequestUri);
					return new SimpleResponse (this, HttpStatusCode.OK, body);
				} catch (Exception ex) {
					return new SimpleResponse (this, HttpStatusCode.InternalServerError, null, ex);
				} finally {
					cts.Dispose ();
				}
			}

			async Task<Response> PostString (InvocationContext ctx, CancellationToken cancellationToken)
			{
				var message = new Http.HttpRequestMessage ();
				message.Method = Http.HttpMethod.Post;
				message.RequestUri = RequestUri;
				message.Content = new Http.StringContent (Parent.Content.AsString ());

				var response = await Client.SendAsync (
					message, Http.HttpCompletionOption.ResponseContentRead, cancellationToken);

				if (response == null)
					throw new InvalidOperationException ("Got null response.");

				ctx.LogMessage ("GOT RESPONSE: {0}", response.StatusCode);

				if (!response.IsSuccessStatusCode)
					return new SimpleResponse (this, response.StatusCode, null);

				string body = null;
				if (response.Content != null) {
					body = await response.Content.ReadAsStringAsync ();
					ctx.LogMessage ("GOT BODY: {0}", body);
				}

				if (Parent.ReturnContent != null) {
					if (body == null)
						throw new InvalidOperationException ("Got null body.");

					body = body.TrimEnd ();
					if (!body.Equals (Parent.ReturnContent.AsString ()))
						throw new InvalidOperationException ("Got invalid body.");
				} else {
					if (!string.IsNullOrEmpty (body))
						throw new InvalidOperationException ("Did not expect a body.");
				}

				return new SimpleResponse (this, response.StatusCode, body);
			}
		}
	}
}

