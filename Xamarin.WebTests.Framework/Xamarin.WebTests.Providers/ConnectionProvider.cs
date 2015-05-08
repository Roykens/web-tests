﻿//
// ConnectionProvider.cs
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

namespace Xamarin.WebTests.Providers
{
	using ConnectionFramework;

	public abstract class ConnectionProvider
	{
		readonly ConnectionProviderFactory factory;
		readonly ConnectionProviderType type;
		readonly ConnectionProviderFlags flags;

		protected ConnectionProvider (ConnectionProviderFactory factory, ConnectionProviderType type, ConnectionProviderFlags flags)
		{
			this.factory = factory;
			this.type = type;
			this.flags = flags;
		}

		public ConnectionProviderFactory Factory {
			get { return factory; }
		}

		public ConnectionProviderType Type {
			get { return type; }
		}

		public ConnectionProviderFlags Flags {
			get { return flags; }
		}

		public abstract bool IsCompatibleWith (ConnectionProviderType type);

		public abstract IClient CreateClient (ClientParameters parameters);

		public abstract IServer CreateServer (ServerParameters parameters);

		public bool SupportsSslStreams {
			get { return (Flags & ConnectionProviderFlags.SupportsSslStream) != 0; }
		}

		public ISslStreamProvider SslStreamProvider {
			get {
				if (!SupportsSslStreams)
					throw new InvalidOperationException ();
				return GetSslStreamProvider ();
			}
		}

		protected abstract ISslStreamProvider GetSslStreamProvider ();

		public bool SupportsHttp {
			get { return (Flags & ConnectionProviderFlags.SupportsHttp) != 0; }
		}

		public IHttpProvider HttpProvider {
			get {
				if (!SupportsHttp)
					throw new InvalidOperationException ();
				return GetHttpProvider ();
			}
		}

		protected abstract IHttpProvider GetHttpProvider ();
	}
}

