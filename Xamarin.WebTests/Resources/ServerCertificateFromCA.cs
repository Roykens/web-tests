﻿using System;
using Xamarin.WebTests.Portable;

namespace Xamarin.WebTests.Resources
{
	class ServerCertificateFromCA : IServerCertificate
	{
		public byte[] Data {
			get;
			private set;
		}

		public string Password {
			get;
			private set;
		}

		internal ServerCertificateFromCA ()
		{
			Data = ResourceManager.ReadResource ("CA.server-cert.pfx");
			Password = "monkey";
		}
	}
}

