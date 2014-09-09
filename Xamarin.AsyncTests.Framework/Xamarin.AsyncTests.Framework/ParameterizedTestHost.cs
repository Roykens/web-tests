﻿//
// ParameterizedTestHost.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	abstract class ParameterizedTestHost : TestHost
	{
		public string ParameterName {
			get;
			private set;
		}

		public TypeInfo ParameterType {
			get;
			private set;
		}

		public IParameterSerializer Serializer {
			get;
			private set;
		}

		protected ParameterizedTestHost (TestHost parent, string name, TypeInfo type,
			IParameterSerializer serializer, TestFlags flags = TestFlags.None)
			: base (parent)
		{
			ParameterName = name;
			ParameterType = type;
			Serializer = serializer;
			Flags = flags;
		}

		internal override TestInvoker CreateInvoker (TestInvoker invoker)
		{
			return new ParameterizedTestInvoker (this, invoker);
		}

		internal override bool Serialize (XElement node, TestInstance instance)
		{
			if (Serializer == null)
				return false;

			var parameterizedInstance = (ParameterizedTestInstance)instance;
			return Serializer.Serialize (node, parameterizedInstance.Current);
		}

		internal override TestHost Deserialize (XElement node, TestHost parent)
		{
			if (Serializer == null)
				return null;

			var value = Serializer.Deserialize (node);
			if (value == null)
				return null;

			return new CapturedTestHost (parent, this, value);
		}

		public override string ToString ()
		{
			return string.Format ("[ParameterizedTestHost: ParameterName={0}, ParameterType={1}]", ParameterName, ParameterType);
		}
	}
}

