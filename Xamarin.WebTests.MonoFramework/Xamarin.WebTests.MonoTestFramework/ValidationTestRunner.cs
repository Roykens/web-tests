﻿//
// ValidationTestRunner.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)

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
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Interface;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Constraints;
using Xamarin.WebTests.Resources;
using Xamarin.WebTests.ConnectionFramework;
using Xamarin.WebTests.MonoConnectionFramework;

namespace Xamarin.WebTests.MonoTestFramework
{
	using MonoTestFeatures;

	public abstract class ValidationTestRunner : ITestInstance, IDisposable
	{
		public ValidationTestParameters Parameters {
			get;
			private set;
		}

		public ValidationTestRunner (ValidationTestParameters parameters)
		{
			Parameters = parameters;
		}


		public void Run (TestContext ctx)
		{
			ctx.LogMessage ("RUN: {0}", this);

			var validator = GetValidator (ctx);
			ctx.Assert (validator, Is.Not.Null, "has validator");

			var certificates = GetCertificates ();

			var result = validator.ValidateCertificate (Parameters.Host, false, certificates);
			AssertResult (ctx, result);
		}

		ICertificateValidator GetValidator (TestContext ctx)
		{
			if (Parameters.Category == ValidationTestCategory.UseProvider) {
				var factory = DependencyInjector.Get<ConnectionProviderFactory> ();
				ConnectionProviderType providerType;
				if (!ctx.TryGetParameter<ConnectionProviderType> (out providerType))
					providerType = ConnectionProviderType.DotNet;

				var provider = (MonoConnectionProvider)factory.GetProvider (providerType);
				return CertificateValidationHelper.GetValidator (provider.MonoTlsProvider, null);
			} else {
				return CertificateValidationHelper.GetValidator (null);
			}
		}

		X509CertificateCollection GetCertificates ()
		{
			var certs = new X509CertificateCollection ();
			foreach (var type in Parameters.Types)
				certs.Add (new X509Certificate (ResourceManager.GetCertificateData (type)));
			return certs;
		}

		void AssertResult (TestContext ctx, ValidationResult result)
		{
			if (Parameters.ExpectSuccess) {
				ctx.Assert (result, Is.Not.Null, "has result");
				ctx.Assert (result.Trusted, Is.True, "trusted");
				ctx.Assert (result.UserDenied, Is.False, "not user denied");
				ctx.Assert (result.ErrorCode, Is.EqualTo (0), "error code");
			} else {
				ctx.Assert (result, Is.Not.Null, "has result");
				ctx.Assert (result.Trusted, Is.False, "not trusted");
				ctx.Assert (result.UserDenied, Is.False, "not user denied");
				if (Parameters.ExpectError != null)
					ctx.Assert (result.ErrorCode, Is.EqualTo (Parameters.ExpectError.Value), "error code");
				else
					ctx.Assert (result.ErrorCode, Is.Not.EqualTo (0), "error code");
			}
		}

		protected internal static Task FinishedTask {
			get { return Task.FromResult<object> (null); }
		}

		#region ITestInstance implementation

		public Task Initialize (TestContext ctx, CancellationToken cancellationToken)
		{
			return FinishedTask;
		}

		public Task PreRun (TestContext ctx, CancellationToken cancellationToken)
		{
			return FinishedTask;
		}

		public virtual Task PostRun (TestContext ctx, CancellationToken cancellationToken)
		{
			return FinishedTask;
		}

		public Task Destroy (TestContext ctx, CancellationToken cancellationToken)
		{
			return Task.Run (() => {
				Dispose ();
			});
		}

		#endregion

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		bool disposed;

		protected virtual void Dispose (bool disposing)
		{
			lock (this) {
				if (disposed)
					return;
				disposed = true;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: {1}]", GetType ().Name, Parameters.Identifier);
		}
	}
}
