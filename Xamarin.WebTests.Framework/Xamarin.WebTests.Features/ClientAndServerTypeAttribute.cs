﻿//
// ClientAndServerTypeAttribute.cs
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
using System.Linq;
using System.Collections.Generic;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.WebTests.Features
{
	using TestRunners;
	using ConnectionFramework;
	using HttpFramework;
	using Portable;
	using Providers;
	using Resources;

	[AttributeUsage (AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
	public class ClientAndServerTypeAttribute : TestParameterAttribute, ITestParameterSource<ClientAndServerType>
	{
		readonly ConnectionProviderFactory factory;

		public ClientAndServerTypeAttribute (string filter = null, TestFlags flags = TestFlags.Browsable)
			: base (filter, flags)
		{
			factory = DependencyInjector.Get<ConnectionProviderFactory> ();
		}

		public ConnectionProviderFlags ProviderFlags {
			get; set;
		}

		bool MatchesFilter (ConnectionProviderType type, string filter)
		{
			var supportedFlags = factory.GetProviderFlags (type);
			if ((supportedFlags & ProviderFlags) != ProviderFlags)
				return false;

			if (filter == null)
				return true;

			var parts = filter.Split (',');
			foreach (var part in parts) {
				if (type.ToString ().Equals (part))
					return true;
			}

			return false;
		}

		public IEnumerable<ConnectionProviderType> GetSupportedProviders (TestContext ctx, string filter)
		{
			return factory.GetSupportedProviders ().Where (p => MatchesFilter (p, filter));
		}

		public IEnumerable<ClientAndServerType> GetParameters (TestContext ctx, string filter)
		{
			var supportedProviders = GetSupportedProviders (ctx, filter);
			foreach (var server in supportedProviders) {
				foreach (var client in supportedProviders) {
					if (factory.IsCompatible (client, server))
						yield return new ClientAndServerType (client, server);
				}
			}
		}
	}
}
