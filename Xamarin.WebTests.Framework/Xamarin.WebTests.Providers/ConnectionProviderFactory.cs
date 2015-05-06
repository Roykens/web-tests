﻿//
// ConnectionProviderFactory.cs
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
using System.Collections.Generic;

namespace Xamarin.WebTests.Providers
{
	public abstract class ConnectionProviderFactory : IConnectionProviderFactory
	{
		readonly Dictionary<ConnectionProviderType,ConnectionProvider> providers;

		protected ConnectionProviderFactory ()
		{
			providers = new Dictionary<ConnectionProviderType,ConnectionProvider> ();
		}

		public bool IsSupported (ConnectionProviderType type)
		{
			return providers.ContainsKey (type);
		}

		public ConnectionProviderFlags GetProviderFlags (ConnectionProviderType type)
		{
			ConnectionProvider provider;
			if (!providers.TryGetValue (type, out provider))
				return ConnectionProviderFlags.None;
			return provider.Flags;
		}

		public IEnumerable<ConnectionProviderType> GetSupportedProviders ()
		{
			return providers.Keys;
		}

		public IConnectionProvider GetProvider (ConnectionProviderType type)
		{
			return providers [type];
		}

		protected void Install (ConnectionProviderType type, ConnectionProvider provider)
		{
			providers.Add (type, provider);
		}

		public abstract IHttpProvider DefaultHttpProvider {
			get;
		}

		public abstract ISslStreamProvider DefaultSslStreamProvider {
			get;
		}
	}
}

