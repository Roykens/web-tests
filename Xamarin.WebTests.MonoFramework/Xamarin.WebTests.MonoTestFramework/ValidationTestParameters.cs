﻿//
// ValidationTestParameters.cs
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
using System.Collections.Generic;
using Xamarin.AsyncTests;
using Xamarin.WebTests.Resources;

namespace Xamarin.WebTests.MonoTestFramework
{
	using MonoTestFeatures;

	[ValidationTestParameters]
	public class ValidationTestParameters : ITestParameter, ICloneable
	{
		public ValidationTestCategory Category {
			get;
			private set;
		}

		public ValidationTestType Type {
			get;
			private set;
		}

		public string Identifier {
			get;
			private set;
		}

		string ITestParameter.Value {
			get { return Identifier; }
		}

		List<CertificateResourceType> types = new List<CertificateResourceType> ();
		bool? expectSuccess;
		int? expectError;

		public IReadOnlyCollection<CertificateResourceType> Types {
			get {
				return types;
			}
		}

		public string Host {
			get; set;
		}

		public bool ExpectSuccess {
			get {
				if (expectSuccess == null)
					throw new InvalidOperationException ();
				return expectSuccess.Value;
			}
			set {
				expectSuccess = value;
			}
		}

		public int? ExpectError {
			get {
				return expectError;
			}
			set {
				if (expectSuccess != null && expectSuccess.Value)
					throw new InvalidOperationException ();
				expectError = value;
			}
		}

		internal void Add (CertificateResourceType type)
		{
			types.Add (type);
		}

		public ValidationTestParameters (ValidationTestCategory category, ValidationTestType type, string identifier)
		{
			Category = category;
			Type = type;
			Identifier = identifier;
		}

		protected virtual ValidationTestParameters Clone ()
		{
			var cloned = new ValidationTestParameters (Category, Type, Identifier);
			cloned.types.AddRange (types);
			cloned.Host = Host;
			cloned.expectSuccess = expectSuccess;
			cloned.expectError = expectError;
			return cloned;
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}
	}
}

