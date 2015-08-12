﻿using System;
using System.IO;
using System.Reflection;
using Xamarin.AsyncTests;

namespace Xamarin.WebTests.Resources
{
	using Portable;
	using Providers;

	public static class ResourceManager
	{
		static readonly ICertificateProvider provider;
		static readonly ICertificate cacert;
		static readonly IServerCertificate serverCert;
		static readonly IServerCertificate selfServer;
		static readonly IServerCertificate invalidServerCert;
		static readonly IClientCertificate invalidClientCert;
		static readonly IClientCertificate invalidClientCaCert;
		static readonly IClientCertificate monkeyCert;
		static readonly IClientCertificate penguinCert;

		static ResourceManager ()
		{
			provider = DependencyInjector.Get<ICertificateProvider> ();
			cacert = provider.GetCertificateFromData (ResourceManager.ReadResource ("CA.Hamiller-Tube-CA.pem"));
			selfServer = provider.GetServerCertificate (ReadResource ("CA.server-self.pfx"), "monkey");
			serverCert = provider.GetServerCertificate (ReadResource ("CA.server-cert.pfx"), "monkey");
			invalidServerCert = provider.GetServerCertificate (ReadResource ("CA.invalid-server-cert.pfx"), "monkey");
			invalidClientCert = provider.GetClientCertificate (ReadResource ("CA.invalid-client-cert.pfx"), "monkey");
			invalidClientCaCert = provider.GetClientCertificate (ReadResource ("CA.invalid-client-ca-cert.pfx"), "monkey");
			monkeyCert = provider.GetClientCertificate (ReadResource ("CA.monkey.pfx"), "monkey");
			penguinCert = provider.GetClientCertificate (ReadResource ("CA.penguin.pfx"), "penguin");
		}

		public static ICertificate LocalCACertificate {
			get { return cacert; }
		}

		public static IServerCertificate InvalidServerCertificate {
			get { return invalidServerCert; }
		}

		public static IServerCertificate SelfSignedServerCertificate {
			get { return selfServer; }
		}

		public static IServerCertificate ServerCertificateFromCA {
			get { return serverCert; }
		}

		public static IClientCertificate InvalidClientCertificate {
			get { return invalidClientCert; }
		}

		public static IClientCertificate InvalidClientCaCertificate {
			get { return invalidClientCaCert; }
		}

		public static IClientCertificate MonkeyCertificate {
			get { return monkeyCert; }
		}

		public static IClientCertificate PenguinCertificate {
			get { return penguinCert; }
		}

		public static IServerCertificate GetServerCertificate (ServerCertificateType type)
		{
			switch (type) {
			case ServerCertificateType.LocalCA:
				return serverCert;
			case ServerCertificateType.SelfSigned:
				return selfServer;
			default:
				throw new InvalidOperationException ();
			}
		}

		internal static byte[] ReadResource (string name)
		{
			var assembly = typeof(ResourceManager).GetTypeInfo ().Assembly;
			using (var stream = assembly.GetManifestResourceStream (assembly.GetName ().Name + "." + name)) {
				var data = new byte [stream.Length];
				var ret = stream.Read (data, 0, data.Length);
				if (ret != data.Length)
					throw new IOException ();
				return data;
			}
		}
	}
}

