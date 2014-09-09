﻿//
// NamedTestHost.cs
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

namespace Xamarin.AsyncTests.Framework
{
	class NamedTestHost : TestHost
	{
		public string Name {
			get;
			private set;
		}

		public TestName TestName {
			get;
			private set;
		}

		public NamedTestHost (TestHost parent, string name)
			: base (parent)
		{
			Name = name;
		}

		public NamedTestHost (TestHost parent, TestName name)
			: base (parent)
		{
			TestName = name;
		}

		internal override TestInstance CreateInstance (TestInstance parent)
		{
			return new NamedTestInstance (this, parent);
		}

		internal override TestInvoker CreateInvoker (TestInvoker invoker)
		{
			return new NamedTestInvoker (this, invoker);
		}

		internal override bool Serialize (XElement node, TestInstance instance)
		{
			return true;
		}

		internal override TestHost Deserialize (XElement node, TestHost parent)
		{
			return new NamedTestHost (parent, Name);
		}

		public override string ToString ()
		{
			return string.Format ("[NamedTestHost: Name={0}]", Name);
		}
	}
}

