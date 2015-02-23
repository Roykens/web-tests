﻿//
// TestCollectionBuilder.cs
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
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	abstract class TestCollectionBuilder : TestBuilder
	{
		public override TestFilter Filter {
			get { return filter; }
		}

		public TestCollectionBuilder (TestSuite suite, string identifier, string name, ITestParameter parameter, TestFilter filter)
			: base (suite, identifier, name, parameter)
		{
			this.filter = filter;
		}

		TestFilter filter;
		List<TestBuilder> innerBuilders;
		Dictionary<string,TestBuilder> testByName;

		protected sealed override IEnumerable<TestBuilder> CreateChildren ()
		{
			innerBuilders = new List<TestBuilder> ();
			testByName = new Dictionary<string,TestBuilder> ();

			foreach (var builder in ResolveChildren ()) {
				testByName.Add (builder.FullName, builder);
				innerBuilders.Add (builder);
				yield return builder;
			}
		}

		protected abstract IEnumerable<TestBuilder> ResolveChildren ();

		protected sealed override TestBuilderHost CreateHost ()
		{
			return new TestCollectionHost (this);
		}
	}
}

