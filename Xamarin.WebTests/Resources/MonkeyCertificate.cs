﻿using System;
using Xamarin.WebTests.Portable;

namespace Xamarin.WebTests.Resources
{
	class MonkeyCertificate : IClientCertificate
	{
		public byte[] Data {
			get;
			private set;
		}

		public string Password {
			get;
			private set;
		}

		internal MonkeyCertificate ()
		{
			Password = "monkey";
			Data = ResourceManager.ReadResource ("CA.monkey.pfx");
		}
	}
}

