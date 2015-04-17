﻿using System;
using System.IO;
using System.Reflection;
using Xamarin.AsyncTests;
using Xamarin.WebTests.Portable;

namespace Xamarin.WebTests.Resources
{
	public static class ResourceManager
	{
		static readonly ICertificateProvider provider;
		static readonly ICertificate cacert;
		static readonly IServerCertificate selfServer;
		static readonly IServerCertificate serverCert;
		static readonly IClientCertificate monkeyCert;

		static ResourceManager ()
		{
			provider = DependencyInjector.Get<ICertificateProvider> ();
			cacert = provider.GetCertificateFromData (ResourceManager.ReadResource ("CA.Hamiller-Tube-CA.pem"));				
			selfServer = provider.GetServerCertificate (ReadResource ("CA.server-self.pfx"), "monkey");
			serverCert = provider.GetServerCertificate (ReadResource ("CA.server-cert.pfx"), "monkey");
			monkeyCert = provider.GetClientCertificate (ReadResource ("CA.monkey.pfx"), "monkey");
		}

		public static ICertificate LocalCACertificate {
			get { return cacert; }
		}

		public static IServerCertificate SelfSignedServerCertificate {
			get { return selfServer; }
		}

		public static IServerCertificate ServerCertificateFromCA {
			get { return serverCert; }
		}

		public static IServerCertificate DefaultServerCertificate {
			get { return serverCert; }
		}

		public static IClientCertificate MonkeyCertificate {
			get { return monkeyCert; }
		}

		public static IServerCertificate GetServerCertificate (ServerCertificateType type)
		{
			switch (type) {
			case ServerCertificateType.Default:
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

