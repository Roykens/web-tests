﻿//
// CombinedClientAndServerParameters.cs
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
using System.Collections.Generic;
using Xamarin.AsyncTests;

namespace Xamarin.WebTests.ConnectionFramework
{
	using Portable;

	public class CombinedClientAndServerParameters : ClientAndServerParameters, IClientParameters, IServerParameters
	{
		bool askForCert;
		bool requireCert;

		public CombinedClientAndServerParameters (string identifier, IServerCertificate certificate)
			: base (identifier)
		{
			ServerCertificate = certificate;
		}

		CombinedClientAndServerParameters (CombinedClientAndServerParameters other)
			: base (other.Identifier)
		{
			ServerCertificate = other.ServerCertificate;
			ClientCertificate = other.ClientCertificate;
			askForCert = other.askForCert;
			requireCert = other.requireCert;
		}

		public override IClientParameters ClientParameters {
			get { return this; }
		}

		public override IServerParameters ServerParameters {
			get { return this; }
		}

		public override ClientAndServerParameters DeepClone ()
		{
			return new CombinedClientAndServerParameters (this);
		}

		public IServerCertificate ServerCertificate {
			get; set;
		}

		public bool AskForClientCertificate {
			get { return askForCert || requireCert; }
			set { askForCert = value; }
		}

		public bool RequireClientCertificate {
			get { return requireCert; }
			set {
				requireCert = value;
				if (value)
					askForCert = true;
			}
		}

		public IClientCertificate ClientCertificate {
			get; set;
		}
	}
}

