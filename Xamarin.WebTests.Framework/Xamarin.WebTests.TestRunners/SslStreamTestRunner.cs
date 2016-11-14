﻿//
// SslStreamTestRunner.cs
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
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Constraints;
using Xamarin.AsyncTests.Framework;

namespace Xamarin.WebTests.TestRunners
{
	using ConnectionFramework;
	using TestFramework;
	using Resources;

	[SslStreamTestRunner]
	public class SslStreamTestRunner : ConnectionTestRunner
	{
		new public SslStreamTestParameters Parameters {
			get { return (SslStreamTestParameters)base.Parameters; }
		}

		public SslStreamTestRunner (IServer server, IClient client, ConnectionTestProvider provider, SslStreamTestParameters parameters)
			: base (server, client, provider, parameters)
		{
		}

		protected override ConnectionHandler CreateConnectionHandler ()
		{
			return new DefaultConnectionHandler (this);
		}

		static string GetTestName (ConnectionTestCategory category, ConnectionTestType type, params object[] args)
		{
			var sb = new StringBuilder ();
			sb.Append (type);
			foreach (var arg in args) {
				sb.AppendFormat (":{0}", arg);
			}
			return sb.ToString ();
		}

		public static SslStreamTestParameters GetParameters (TestContext ctx, ConnectionTestCategory category, ConnectionTestType type)
		{
			var certificateProvider = DependencyInjector.Get<ICertificateProvider> ();
			var acceptAll = certificateProvider.AcceptAll ();
			var rejectAll = certificateProvider.RejectAll ();
			var acceptNull = certificateProvider.AcceptNull ();
			var acceptSelfSigned = certificateProvider.AcceptThisCertificate (ResourceManager.SelfSignedServerCertificate);
			var acceptFromLocalCA = certificateProvider.AcceptFromCA (ResourceManager.LocalCACertificate);

			var name = GetTestName (category, type);

			SslStreamTestParameters parameters;

			switch (type) {
			case ConnectionTestType.Default:
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificateValidator = acceptAll
				};

			case ConnectionTestType.AcceptFromLocalCA:
				return new SslStreamTestParameters (category, type, name, ResourceManager.ServerCertificateFromCA) {
					ClientCertificateValidator = acceptFromLocalCA
				};

			case ConnectionTestType.NoValidator:
				// The default validator only allows ResourceManager.SelfSignedServerCertificate.
				return new SslStreamTestParameters (category, type, name, ResourceManager.ServerCertificateFromCA) {
					ExpectClientException = true
				};

			case ConnectionTestType.RejectAll:
				// Explicit validator overrides the default ServicePointManager.ServerCertificateValidationCallback.
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ExpectClientException = true, ClientCertificateValidator = rejectAll
				};

			case ConnectionTestType.UnrequestedClientCertificate:
				// Provide a client certificate, but do not require it.
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificate = ResourceManager.PenguinCertificate, ClientCertificateValidator = acceptSelfSigned,
					ServerCertificateValidator = acceptNull
				};

			case ConnectionTestType.RequestClientCertificate:
				/*
				 * Request client certificate, but do not require it.
				 *
				 * FIXME:
				 * SslStream with Mono's old implementation fails here.
				 */
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificate = ResourceManager.MonkeyCertificate, ClientCertificateValidator = acceptSelfSigned,
					AskForClientCertificate = true, ServerCertificateValidator = acceptFromLocalCA
				};

			case ConnectionTestType.RequireClientCertificate:
				// Require client certificate.
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificate = ResourceManager.MonkeyCertificate, ClientCertificateValidator = acceptSelfSigned,
					AskForClientCertificate = true, RequireClientCertificate = true,
					ServerCertificateValidator = acceptFromLocalCA
				};

			case ConnectionTestType.OptionalClientCertificate:
				/*
				 * Request client certificate without requiring one and do not provide it.
				 *
				 * To ask for an optional client certificate (without requiring it), you need to specify a custom validation
				 * callback and then accept the null certificate with `SslPolicyErrors.RemoteCertificateNotAvailable' in it.
				 *
				 * FIXME:
				 * Mono with the old TLS implementation throws SecureChannelFailure.
				 */
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificateValidator = acceptSelfSigned, AskForClientCertificate = true,
					ServerCertificateValidator = acceptNull
				};

			case ConnectionTestType.RejectClientCertificate:
				// Reject client certificate.
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificate = ResourceManager.MonkeyCertificate, ClientCertificateValidator = acceptSelfSigned,
					ServerCertificateValidator = rejectAll, AskForClientCertificate = true,
					ExpectClientException = true, ExpectServerException = true
				};

			case ConnectionTestType.MissingClientCertificate:
				// Missing client certificate.
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificateValidator = acceptSelfSigned,
					AskForClientCertificate = true, RequireClientCertificate = true,
					ExpectClientException = true, ExpectServerException = true
				};

			case ConnectionTestType.MartinTest:
				goto case ConnectionTestType.TrustedRootCA;

			case ConnectionTestType.MustNotInvokeGlobalValidator:
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificateValidator = acceptAll,
					GlobalValidationFlags = GlobalValidationFlags.MustNotInvoke
				};

			case ConnectionTestType.MustNotInvokeGlobalValidator2:
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					GlobalValidationFlags = GlobalValidationFlags.MustNotInvoke,
					ExpectClientException = true
				};

			case ConnectionTestType.TrustedRootCA:
				parameters = new SslStreamTestParameters (category, type, name, ResourceManager.ServerCertificateFromCA) {
					GlobalValidationFlags = GlobalValidationFlags.CheckChain,
					ExpectPolicyErrors = SslPolicyErrors.None, TargetHost = "Hamiller-Tube.local"
				};
				parameters.ValidationParameters = new ValidationParameters ();
				parameters.ValidationParameters.AddTrustedRoot (CertificateResourceType.HamillerTubeCA);
				parameters.ValidationParameters.ExpectSuccess = true;
				return parameters;

			case ConnectionTestType.CheckServerName:
				return new SslStreamTestParameters (category, type, name, ResourceManager.SelfSignedServerCertificate) {
					ClientCertificateValidator = acceptSelfSigned, TargetHost = "martin.Hamiller-Tube.local",
					ExpectServerName = "martin.Hamiller-Tube.local"
				};

			default:
				throw new InternalErrorException ();
			}
		}

		protected override async Task OnRun (TestContext ctx, CancellationToken cancellationToken)
		{
			await base.OnRun (ctx, cancellationToken);

			if (Parameters.ExpectServerException)
				ctx.AssertFail ("expecting server exception");
			if (Parameters.ExpectClientException)
				ctx.AssertFail ("expecting client exception");

			if (!IsManualServer) {
				ctx.Expect (Server.SslStream.IsAuthenticated, "server is authenticated");

				if (Server.Parameters.RequireClientCertificate) {
					ctx.LogDebug (1, "Client certificate required: {0} {1}", Server.SslStream.IsMutuallyAuthenticated, Server.SslStream.HasRemoteCertificate);
					ctx.Expect (Server.SslStream.IsMutuallyAuthenticated, "server is mutually authenticated");
					ctx.Expect (Server.SslStream.HasRemoteCertificate, "server has client certificate");
				}
			}

			if (!IsManualClient) {
				ctx.Expect (Client.SslStream.IsAuthenticated, "client is authenticated");

				ctx.Expect (Client.SslStream.HasRemoteCertificate, "client has server certificate");
			}

			if (!IsManualConnection && Server.Parameters.AskForClientCertificate && Client.Parameters.ClientCertificate != null)
				ctx.Expect (Client.SslStream.HasLocalCertificate, "client has local certificate");

			if (Parameters.ExpectServerName != null) {
				ctx.Assert (Server.SslStream.SupportsConnectionInfo, "Server.SslStream.SupportsConnectionInfo");
				var info = Server.SslStream.GetConnectionInfo ();
				if (ctx.Expect (info, Is.Not.Null, "Server.SslStream.GetConnectionInfo()"))
					ctx.Expect (info.ServerName, Is.EqualTo (Parameters.ExpectServerName), "ServerName");
			}
		}

		protected override void OnWaitForServerConnectionCompleted (TestContext ctx, Task task)
		{
			if (Parameters.ExpectServerException) {
				ctx.Assert (task.IsFaulted, "expecting exception");
				throw new ConnectionFinishedException ();
			}

			if (task.IsFaulted) {
				if (Parameters.ExpectClientException)
					throw new ConnectionFinishedException ();
				throw task.Exception;
			}

			base.OnWaitForServerConnectionCompleted (ctx, task);
		}

		protected override void OnWaitForClientConnectionCompleted (TestContext ctx, Task task)
		{
			if (task.IsFaulted) {
				if (Parameters.ExpectClientException)
					throw new ConnectionFinishedException ();
				throw task.Exception;
			}

			base.OnWaitForClientConnectionCompleted (ctx, task);
		}

		bool HasFlag (GlobalValidationFlags flag)
		{
			return (Parameters.GlobalValidationFlags & flag) == flag;
		}

		RemoteCertificateValidationCallback savedGlobalCallback;
		TestContext savedContext;
		bool restoreGlobalCallback;
		int localValidatorInvoked;

		void SetGlobalValidationCallback (TestContext ctx, RemoteCertificateValidationCallback callback)
		{
			savedGlobalCallback = ServicePointManager.ServerCertificateValidationCallback;
			ServicePointManager.ServerCertificateValidationCallback = callback;
			savedContext = ctx;
			restoreGlobalCallback = true;
		}

		void InstallTestRunnerCallback (TestContext ctx)
		{
			savedContext = ctx;
			Parameters.ClientCertificateValidator = new CertificateValidator (TestRunnerCallback);
		}

		bool GlobalValidator (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			savedContext.AssertFail ("Global validator has been invoked!");
			return false;
		}

		bool TestRunnerCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return LocalValidator (savedContext, certificate, chain, sslPolicyErrors);
		}

		bool LocalValidator (TestContext ctx, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			ctx.LogMessage ("Local validator: {0}", localValidatorInvoked);
			if (HasFlag (GlobalValidationFlags.MustNotInvoke)) {
				ctx.AssertFail ("Local validator has been invoked!");
				return false;
			}

			++localValidatorInvoked;

			bool result = errors == SslPolicyErrors.None;

			if (Parameters.ValidationParameters != null) {
				CertificateInfoTestRunner.CheckValidationResult (ctx, Parameters.ValidationParameters, certificate, chain, errors);
				result = true;
			}

			if (HasFlag (GlobalValidationFlags.CheckChain)) {
				CertificateInfoTestRunner.CheckCallbackChain (ctx, Parameters, certificate, chain, errors);
				result = true;
			}

			if (HasFlag (GlobalValidationFlags.AlwaysFail))
				return false;
			else if (HasFlag (GlobalValidationFlags.AlwaysSucceed))
				return true;

			return result;
		}

		protected override Task PreRun (TestContext ctx, CancellationToken cancellationToken)
		{
			if (HasFlag (GlobalValidationFlags.CheckChain))
				Parameters.GlobalValidationFlags |= GlobalValidationFlags.SetToTestRunner;

			if (HasFlag (GlobalValidationFlags.MustNotInvoke))
				SetGlobalValidationCallback (ctx, GlobalValidator);
			else if (HasFlag (GlobalValidationFlags.SetToTestRunner))
				InstallTestRunnerCallback (ctx);
			else if (Parameters.GlobalValidationFlags != 0)
				ctx.AssertFail ("Invalid GlobalValidationFlags");
			else {
				ctx.Assert (Parameters.ExpectChainStatus, Is.Null, "Parameters.ExpectChainStatus");
				ctx.Assert (Parameters.ExpectPolicyErrors, Is.Null, "Parameters.ExpectPolicyErrors");
			}

			return base.PreRun (ctx, cancellationToken);
		}

		protected override Task PostRun (TestContext ctx, CancellationToken cancellationToken)
		{
			if (restoreGlobalCallback)
				ServicePointManager.ServerCertificateValidationCallback = savedGlobalCallback;

			if (HasFlag (GlobalValidationFlags.MustInvoke) || HasFlag (GlobalValidationFlags.CheckChain))
				ctx.Assert (localValidatorInvoked, Is.EqualTo (1), "local validator has been invoked");

			return base.PostRun (ctx, cancellationToken);
		}
	}
}

