﻿//
// HttpServerAttribute.cs
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
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.WebTests.Features
{
	using ConnectionFramework;
	using HttpFramework;
	using Portable;
	using Providers;

	public class HttpServerAttribute : TestHostAttribute, ITestHost<HttpServer>
	{
		public HttpServerAttribute ()
			: base (typeof (HttpServerAttribute))
		{
		}

		public HttpServer CreateInstance (TestContext ctx)
		{
			var factory = DependencyInjector.Get<IHttpProviderFactory> ();
			var support = DependencyInjector.Get<IPortableEndPointSupport> ();
			var endpoint = support.GetLoopbackEndpoint (9999);

			ListenerFlags flags = ListenerFlags.None;

			bool reuseConnection;
			if (ctx.TryGetParameter<bool> (out reuseConnection, "ReuseConnection") && reuseConnection)
				flags |= ListenerFlags.ReuseConnection;

			bool useSSL;
			if (ctx.TryGetParameter<bool> (out useSSL, "UseSSL") && useSSL) {
				var webSupport = DependencyInjector.Get<IPortableWebSupport> ();
				var certificate = webSupport.GetDefaultServerCertificate ();
				return new HttpServer (factory.Default, endpoint, flags, certificate);
			}

			return new HttpServer (factory.Default, endpoint, flags);
		}
	}
}

