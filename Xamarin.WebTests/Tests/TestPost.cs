﻿//
// TestPost.cs
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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Xamarin.AsyncTests;

namespace Xamarin.WebTests.Tests
{
	using Server;
	using Runners;
	using Handlers;
	using Framework;

	[AsyncTestFixture (Timeout = 15000)]
	public class TestPost : ITestHost<HttpTestRunner>, ITestParameterSource<Handler>
	{
		[TestParameter (typeof (WebTestFeatures.SelectSSL), null, TestFlags.Hidden)]
		public bool UseSSL {
			get; set;
		}

		public HttpTestRunner CreateInstance (TestContext context)
		{
			return new HttpTestRunner { UseSSL = UseSSL };
		}

		public static IEnumerable<PostHandler> GetPostTests ()
		{
			yield return new PostHandler () {
				Description = "No body"
			};
			yield return new PostHandler () {
				Description = "Empty body", Content = new StringContent (string.Empty)
			};
			yield return new PostHandler () {
				Description = "Normal post",
				Content = new StringContent ("Hello Unknown World!")
			};
			yield return new PostHandler () {
				Description = "Content-Length",
				Content = new StringContent ("Hello Known World!"),
				Mode = TransferMode.ContentLength
			};
			yield return new PostHandler () {
				Description = "Chunked",
				Content = new ChunkedContent ("Hello Chunked World!"),
				Mode = TransferMode.Chunked
			};
			yield return new PostHandler () {
				Description = "Explicit length and empty body",
				Mode = TransferMode.ContentLength,
				Content = new StringContent (string.Empty)
			};
			yield return new PostHandler () {
				Description = "Explicit length and no body",
				Mode = TransferMode.ContentLength
			};
		}

		public static IEnumerable<Handler> GetDeleteTests ()
		{
			yield return new DeleteHandler ();
			yield return new DeleteHandler () {
				Description = "DELETE with empty body",
				Body = string.Empty
			};
			yield return new DeleteHandler () {
				Description = "DELETE with request body",
				Body = "I have a body!"
			};
			yield return new DeleteHandler () {
				Description = "DELETE with no body and a length",
				Flags = RequestFlags.ExplicitlySetLength
			};
		}

		static IEnumerable<Handler> GetChunkedTests ()
		{
			var chunks = new List<string> ();
			for (var i = 'A'; i < 'Z'; i++) {
				chunks.Add (new string (i, 1000));
			}

			var content = new ChunkedContent (chunks);

			yield return new PostHandler () {
				Description = "Chunked",
				Content = content,
				Mode = TransferMode.Chunked
			};
		}

		public static IEnumerable<Handler> GetParameters (TestContext context, string filter)
		{
			if (filter == null) {
				var list = new List<Handler> ();
				list.Add (new HelloWorldHandler ());
				list.AddRange (GetPostTests ());
				list.AddRange (GetDeleteTests ());
				return list;
			} else if (filter.Equals ("post"))
				return GetPostTests ();
			else if (filter.Equals ("delete"))
				return GetDeleteTests ();
			else if (filter.Equals ("chunked"))
				return GetChunkedTests ();
			else
				throw new InvalidOperationException ();
		}

		IEnumerable<Handler> ITestParameterSource<Handler>.GetParameters (TestContext context, string filter)
		{
			return GetParameters (context, filter);
		}

		[AsyncTest]
		public Task RedirectAsGetNoBuffering (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			CancellationToken cancellationToken)
		{
			var post = new PostHandler {
				Description = "RedirectAsGetNoBuffering",
				Content = new ChunkedContent ("Hello chunked world"),
				Mode = TransferMode.Chunked,
				Flags = RequestFlags.RedirectedAsGet,
				AllowWriteStreamBuffering = false
			};
			var handler = new RedirectHandler (post, HttpStatusCode.Redirect);
			return runner.Run (ctx, handler, cancellationToken);
		}

		[AsyncTest]
		public Task RedirectNoBuffering (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			CancellationToken cancellationToken)
		{
			var post = new PostHandler {
				Description = "RedirectNoBuffering",
				Content = new ChunkedContent ("Hello chunked world"),
				Mode = TransferMode.Chunked,
				Flags = RequestFlags.Redirected,
				AllowWriteStreamBuffering = false
			};
			var handler = new RedirectHandler (post, HttpStatusCode.TemporaryRedirect);
			return runner.Run (ctx, handler, cancellationToken, HttpStatusCode.TemporaryRedirect, true);
		}

		[AsyncTest]
		public Task Run (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			[TestParameter] Handler handler, CancellationToken cancellationToken)
		{
			return runner.Run (ctx, handler, cancellationToken);
		}

		[AsyncTest]
		public Task Redirect (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			[TestParameter (typeof (RedirectStatusSource))] HttpStatusCode code,
			[TestParameter ("post")] Handler handler, CancellationToken cancellationToken)
		{
			var post = (PostHandler)handler;
			var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
			var hasBody = post.Content != null || ((post.Flags & RequestFlags.ExplicitlySetLength) != 0) || (post.Mode == TransferMode.ContentLength);

			if ((hasBody || !isWindows) && (code == HttpStatusCode.MovedPermanently || code == HttpStatusCode.Found))
				post.Flags = RequestFlags.RedirectedAsGet;
			else
				post.Flags = RequestFlags.Redirected;
			post.Description = string.Format ("{0}: {1}", code, post.Description);
			var redirect = new RedirectHandler (post, code) { Description = post.Description };

			return runner.Run (ctx, redirect, cancellationToken);
		}

		[AsyncTest]
		public async Task Test18750 (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			CancellationToken cancellationToken)
		{
			var post = new PostHandler {
				Description = "First post",
				Content = new StringContent ("var1=value&var2=value2"),
				Flags = RequestFlags.RedirectedAsGet
			};
			var redirect = new RedirectHandler (post, HttpStatusCode.Redirect);

			redirect.Register (ctx);

			var uri = redirect.RegisterRequest (runner.Listener);

			var wc = new WebClient ();
			var res = await wc.UploadStringTaskAsync (uri, post.Content.AsString ());
			ctx.LogDebug (2, "Test18750: {0}", res);

			var secondPost = new PostHandler {
				Description = "Second post", Content = new StringContent ("Should send this")
			};

			await runner.Run (ctx, secondPost, cancellationToken);
		}

		[AsyncTest]
		public Task TestChunked (
			InvocationContext ctx, [TestHost] HttpTestRunner runner,
			[TestParameter ("chunked")] Handler handler, CancellationToken cancellationToken)
		{
			return runner.Run (ctx, handler, cancellationToken);
		}
	}
}

