﻿//
// DeleteHandler.cs
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
using System.Text;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

namespace Xamarin.WebTests.Handlers
{
	using Framework;
	using Server;

	public class DeleteHandler : Handler
	{
		string body;

		public string Body {
			get { return body; }
			set {
				WantToModify ();
				body = value;
			}
		}

		public override object Clone ()
		{
			var delete = new DeleteHandler ();
			delete.body = body;
			return delete;
		}

		protected internal override HttpResponse HandleRequest (HttpConnection connection, HttpRequest request, RequestFlags effectiveFlags)
		{
			if (!request.Method.Equals ("DELETE"))
				return HttpResponse.CreateError ("Wrong method: {0}", request.Method);

			string value;
			var hasLength = request.Headers.TryGetValue ("Content-Length", out value);
			var hasExplicitLength = (Flags & RequestFlags.ExplicitlySetLength) != 0;

			if (hasLength) {
				var length = int.Parse (value);

				if (Body != null) {
					request.ReadBody ();
					return HttpResponse.CreateSuccess ();
				} else if (hasExplicitLength) {
					if (length != 0)
						return HttpResponse.CreateError ("Content-Length must be zero");
				} else {
					return HttpResponse.CreateError ("Content-Length not allowed.");
				}
			} else if (hasExplicitLength || Body != null) {
				return HttpResponse.CreateError ("Missing Content-Length");
			}

			return HttpResponse.CreateSuccess ();
		}

		public override Request CreateRequest (Uri uri)
		{
			var traditional = new TraditionalRequest (uri);
			traditional.Request.Method = "DELETE";

			if (Flags == RequestFlags.ExplicitlySetLength)
				traditional.Request.ContentLength = Body != null ? Body.Length : 0;

			traditional.Content = StringContent.CreateMaybeNull (body);

			return traditional;
		}
	}
}
