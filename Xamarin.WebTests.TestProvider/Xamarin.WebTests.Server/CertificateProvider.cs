﻿//
// CertificateProvider.cs
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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Xamarin.WebTests.Server
{
	using Portable;
	using Providers;
	using ConnectionFramework;

	class CertificateProvider : ICertificateProvider
	{
		static readonly CertificateValidator acceptAll = new CertificateValidator ((s, c, ch, e) => true);
		static readonly CertificateValidator rejectAll = new CertificateValidator ((s, c, ch, e) => false);
		static readonly CertificateValidator acceptNull = new CertificateValidator ((s, c, ch, e) => {
			return c == null && e == SslPolicyErrors.RemoteCertificateNotAvailable;
		});

		public static CertificateValidator AcceptAll {
			get { return acceptAll; }
		}

		public static CertificateValidator RejectAll {
			get { return rejectAll; }
		}

		public static CertificateValidator AcceptNull {
			get { return acceptNull; }
		}

		public CertificateValidator GetDefault ()
		{
			return RejectAll;
		}

		CertificateValidator ICertificateProvider.AcceptNull ()
		{
			return AcceptNull;
		}

		CertificateValidator ICertificateProvider.AcceptThisCertificate (IServerCertificate certificate)
		{
			return AcceptThisCertificate (certificate);
		}

		internal CertificateValidator AcceptThisCertificate (IServerCertificate certificate)
		{
			var cert = GetCertificate (certificate);
			var serverHash = cert.GetCertHash ();

			return new CertificateValidator ((s, c, ch, e) => {
				if (c == null || e == SslPolicyErrors.RemoteCertificateNotAvailable)
					return false;
				if (e == SslPolicyErrors.None)
					return true;
				return Compare (c.GetCertHash (), serverHash);
			});
		}

		CertificateValidator ICertificateProvider.AcceptFromCA (ICertificate certificate)
		{
			return AcceptFromCA (certificate);
		}

		internal CertificateValidator AcceptFromCA (ICertificate certificate)
		{
			var cert = GetCertificate (certificate);

			return new CertificateValidator ((s, c, ch, e) => {
				if (c == null || e == SslPolicyErrors.RemoteCertificateNotAvailable)
					return false;
				if (e == SslPolicyErrors.None)
					return true;
				return c.Issuer.Equals (cert.Issuer);
			});
		}

		void ICertificateProvider.InstallDefaultValidator (CertificateValidator validator)
		{
			InstallDefaultValidator ((CertificateValidator)validator);
		}

		public void InstallDefaultValidator (CertificateValidator validator)
		{
			if (validator != null)
				ServicePointManager.ServerCertificateValidationCallback = validator.ValidationCallback;
			else
				ServicePointManager.ServerCertificateValidationCallback = null;
		}

		static bool Compare (byte[] first, byte[] second)
		{
			if (first.Length != second.Length)
				return false;
			for (int i = 0; i < first.Length; i++) {
				if (first[i] != second[i])
					return false;
			}
			return true;
		}

		CertificateValidator ICertificateProvider.RejectAll ()
		{
			return RejectAll;
		}

		CertificateValidator ICertificateProvider.AcceptAll ()
		{
			return AcceptAll;
		}

		public IServerCertificate GetServerCertificate (byte[] data, string password)
		{
			return new CertificateFromPFX (data, password);
		}

		public IClientCertificate GetClientCertificate (byte[] data, string password)
		{
			return new CertificateFromPFX (data, password);
		}

		public static ICertificate GetCertificate (X509Certificate certificate)
		{
			var certificate2 = certificate as X509Certificate2;
			if (certificate2 != null)
				return new CertificateFromData2 (certificate2);
			return certificate != null ? new CertificateFromData (certificate) : null;
		}

		public static X509Certificate GetCertificate (ICertificate certificate)
		{
			return certificate != null ? ((CertificateFromData)certificate).Certificate : null;
		}

		public static byte[] GetRawCertificateData (IClientCertificate certificate, out string password)
		{
			var pfx = (CertificateFromPFX)certificate;
			password = pfx.Password;
			return pfx.Data;
		}

		public static byte[] GetRawCertificateData (IServerCertificate certificate, out string password)
		{
			var pfx = (CertificateFromPFX)certificate;
			password = pfx.Password;
			return pfx.Data;
		}

		public ICertificate GetCertificateFromData (byte[] data)
		{
			return new CertificateFromData (data);
		}

		ICertificate ICertificateProvider.GetCertificate (X509Certificate certificate)
		{
			return GetCertificate (certificate);
		}

		static ICertificate[] GetCertificateCollection (X509CertificateCollection collection)
		{
			if (collection == null)
				return null;
			var array = new ICertificate [collection.Count];
			for (int i = 0; i < array.Length; i++)
				array [i] = GetCertificate (collection [i]);
			return array;
		}

		public CertificateValidator GetCustomCertificateValidator (CertificateValidationDelegate func)
		{
			return new CertificateValidator ((s, c, ch, e) => func (GetCertificate (c)));
		}

		public CertificateSelector GetCustomCertificateSelector (CertificateSelectionDelegate func)
		{
			return new CertificateSelector ((s, t, lc, rc, ai) => {
				var localCertificates = GetCertificateCollection (lc);
				var remoteCertificate = GetCertificate (rc);
				var result = func (t, localCertificates, remoteCertificate, ai);
				return GetCertificate (result);
			});
		}

		public CertificateValidator GetCustomCertificateValidator (RemoteCertificateValidationCallback callback)
		{
			return new CertificateValidator (callback);
		}

		public CertificateSelector GetCustomCertificateSelector (LocalCertificateSelectionCallback callback)
		{
			return new CertificateSelector (callback);
		}

		public bool AreEqual (ICertificate a, ICertificate b)
		{
			if (a == b)
				return true;

			return AreEqual ((CertificateFromData)a, (CertificateFromData)b);
		}

		bool AreEqual (CertificateFromData a, CertificateFromData b)
		{
			var aHash = a.GetCertificateHash ();
			var bHash = b.GetCertificateHash ();
			return string.Equals (aHash, bHash);
		}

		public bool AreEqual (X509Certificate a, ICertificate b)
		{
			return AreEqual (new CertificateFromData (a), (CertificateFromData)b);
		}

		class CertificateFromData : ICertificate
		{
			public byte[] Data {
				get;
				private set;
			}

			public X509Certificate Certificate {
				get;
				private set;
			}

			public string Issuer {
				get { return Certificate.Issuer; }
			}

			public string Subject {
				get { return Certificate.Subject; }
			}

			public string GetSerialNumber ()
			{
				return Certificate.GetSerialNumberString ();
			}

			public string GetCertificateHash ()
			{
				return Certificate.GetCertHashString ();
			}

			public CertificateFromData (X509Certificate certificate)
			{
				Certificate = certificate;
				Data = certificate.GetRawCertData ();
			}

			public CertificateFromData (byte[] data)
			{
				Data = data;
				Certificate = new X509Certificate (data);
			}

			protected CertificateFromData (byte[] data, X509Certificate certificate)
			{
				Data = data;
				Certificate = certificate;
			}
		}

		class CertificateFromData2 : CertificateFromData, IServerCertificate, IClientCertificate
		{
			public string Password {
				get { throw new InvalidOperationException (); }
			}

			public CertificateFromData2 (X509Certificate2 certificate)
				: base (certificate)
			{
			}
		}

		class CertificateFromPFX : CertificateFromData, IServerCertificate, IClientCertificate
		{
			public string Password {
				get;
				private set;
			}

			new public X509Certificate2 Certificate {
				get { return (X509Certificate2)base.Certificate; }
			}

			public CertificateFromPFX (byte[] data, string password)
				: base (data, new X509Certificate2 (data, password))
			{
				Password = password;
			}
		}

	}
}

