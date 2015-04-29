﻿//
// TestSslStream.cs
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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Portable;
using Xamarin.AsyncTests.Constraints;

namespace Xamarin.WebTests.Tests
{
	using ConnectionFramework;
	using Portable;
	using Providers;
	using Resources;

	public class ServerTestHostAttribute : TestHostAttribute, ITestHost<IServer>
	{
		public ServerTestHostAttribute ()
			: base (typeof (ServerTestHostAttribute))
		{
		}

		public IServer CreateInstance (TestContext ctx)
		{
			ConnectionProviderType providerType;
			if (!ctx.TryGetParameter<ConnectionProviderType> (out providerType))
				providerType = ConnectionProviderType.DotNet;

			var parameters = ctx.GetParameter<ClientAndServerParameters> ().ServerParameters;

			var factory = DependencyInjector.Get<IConnectionProviderFactory> ();
			var provider = factory.GetProvider (providerType);
			return provider.CreateServer (parameters);
		}
	}

	public class ClientTestHostAttribute : TestHostAttribute, ITestHost<IClient>
	{
		public ClientTestHostAttribute ()
			: base (typeof (ClientTestHostAttribute))
		{
		}

		public IClient CreateInstance (TestContext ctx)
		{
			ConnectionProviderType providerType;
			if (!ctx.TryGetParameter<ConnectionProviderType> (out providerType))
				providerType = ConnectionProviderType.DotNet;

			var parameters = ctx.GetParameter<ClientAndServerParameters> ().ClientParameters;

			var factory = DependencyInjector.Get<IConnectionProviderFactory> ();
			var provider = factory.GetProvider (providerType);
			return provider.CreateClient (parameters);
		}
	}

	class ConnectionParameterAttribute : TestParameterAttribute, ITestParameterSource<ClientAndServerParameters>
	{
		readonly ICertificateProvider certificateProvider;
		readonly ICertificateValidator acceptFromLocalCA;

		public ConnectionParameterAttribute (string filter = null)
			: base (filter)
		{
			certificateProvider = DependencyInjector.Get<ICertificateProvider> ();
			acceptFromLocalCA = certificateProvider.AcceptFromThisCA (ResourceManager.LocalCACertificate);
		}

		public IEnumerable<ClientAndServerParameters> GetParameters (TestContext ctx, string filter)
		{
			yield return new CombinedClientAndServerParameters ("simple", ResourceManager.SelfSignedServerCertificate) {
				VerifyPeerCertificate = false

			};
			yield return new CombinedClientAndServerParameters ("verify-certificate", ResourceManager.ServerCertificateFromCA) {
				VerifyPeerCertificate = true, CertificateValidator = acceptFromLocalCA
			};
			yield return new CombinedClientAndServerParameters ("ask-for-certificate", ResourceManager.ServerCertificateFromCA) {
				VerifyPeerCertificate = true, CertificateValidator = acceptFromLocalCA,
				AskForClientCertificate = true
			};
			yield return new CombinedClientAndServerParameters ("require-certificate", ResourceManager.ServerCertificateFromCA) {
				VerifyPeerCertificate = true, CertificateValidator = acceptFromLocalCA,
				RequireClientCertificate = true, ClientCertificate = ResourceManager.MonkeyCertificate
			};
		}
	}

	[SSL]
	[Martin]
	[AsyncTestFixture (Timeout = 5000)]
	public class TestSslStream
	{
		[AsyncTest]
		public async Task TestConnection (TestContext ctx,
			[ConnectionParameterAttribute] ClientAndServerParameters parameters,
			[ServerTestHost] IServer server, [ClientTestHost] IClient client)
		{
			var handler = ClientAndServerHandlerFactory.HandshakeAndDone.Create (server, client);
			await handler.WaitForConnection ();

			await handler.Run ();
		}
	}
}

