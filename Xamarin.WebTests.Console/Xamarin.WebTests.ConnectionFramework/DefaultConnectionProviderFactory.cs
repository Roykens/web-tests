﻿//
// DefaultConnectionProviderFactory.cs
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
using System.Net;
using System.Collections.Generic;
using System.Security.Authentication;
using Xamarin.AsyncTests;

namespace Xamarin.WebTests.ConnectionFramework
{
	using Providers;

	class DefaultConnectionProviderFactory : IConnectionProviderFactory
	{
		readonly Dictionary<ConnectionProviderType,IConnectionProvider> providers;
		readonly IConnectionProvider dotNetProvider;

		public bool IsSupported (ConnectionProviderType type)
		{
			return providers.ContainsKey (type);
		}

		public IConnectionProvider GetProvider (ConnectionProviderType type)
		{
			return providers [type];
		}

		protected void Install (ConnectionProviderType type, IConnectionProvider provider)
		{
			providers.Add (type, provider);
		}

		public IEnumerable<ConnectionProviderType> GetSupportedProviders ()
		{
			return providers.Keys;
		}

		internal DefaultConnectionProviderFactory ()
		{
			providers = new Dictionary<ConnectionProviderType,IConnectionProvider> ();

			var factory = DependencyInjector.Get<ISslStreamProviderFactory> ();
			dotNetProvider = new DefaultConnectionProvider (factory.GetDefaultProvider ());
			Install (ConnectionProviderType.DotNet, dotNetProvider);

			if (factory.IsSupported (SslStreamProviderType.MonoNewTls)) {
				var newTlsStreamProvider = factory.GetProvider (SslStreamProviderType.MonoNewTls);
				var newTlsConnectionProvider = new DefaultConnectionProvider (newTlsStreamProvider);
				Install (ConnectionProviderType.NewTLS, newTlsConnectionProvider);
			}
		}
	}
}

