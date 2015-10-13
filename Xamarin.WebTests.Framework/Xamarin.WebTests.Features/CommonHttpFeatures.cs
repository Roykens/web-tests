﻿//
// CommonHttpFeatures.cs
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

	static class CommonHttpFeatures
	{
		internal static IHttpProvider GetHttpProvider (TestContext ctx)
		{
			var factory = DependencyInjector.Get<ConnectionProviderFactory> ();
			IHttpProvider provider;
			ConnectionProviderType providerType;
			if (ctx.TryGetParameter (out providerType))
				provider = factory.GetProvider (providerType).HttpProvider;
			else
				provider = factory.DefaultHttpProvider;

			return provider;
		}

		internal static IPortableEndPoint GetEndPoint (TestContext ctx)
		{
			var support = DependencyInjector.Get<IPortableEndPointSupport> ();
			var port = ctx.GetUniquePort ();
			return support.GetLoopbackEndpoint (port);
		}

		internal static bool IsMicrosoftRuntime {
			get { return DependencyInjector.Get<IPortableSupport> ().IsMicrosoftRuntime; }
		}

		internal static bool IsNewTls (ConnectionProviderType type)
		{
			switch (type) {
			case ConnectionProviderType.DotNet:
				return IsMicrosoftRuntime;
			case ConnectionProviderType.NewTLS:
			case ConnectionProviderType.MonoWithNewTLS:
			case ConnectionProviderType.OpenSsl:
				return true;
			default:
				return false;
			}
		}

		internal static void GetUniqueEndPoint (TestContext ctx, ClientAndServerParameters parameters)
		{
			if (parameters.EndPoint == null)
				parameters.EndPoint = GetEndPoint (ctx);

			if (parameters.ServerParameters.EndPoint == null)
				parameters.ServerParameters.EndPoint = parameters.EndPoint;
			if (parameters.ClientParameters.EndPoint == null)
				parameters.ClientParameters.EndPoint = parameters.EndPoint ?? parameters.ServerParameters.EndPoint;

		}
	}
}

