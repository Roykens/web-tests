﻿//
// TestCaseCollection.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	using Internal;

	public class TestCaseCollection : TestCase
	{
		List<TestCase> tests = new List<TestCase> ();

		public TestCaseCollection (string name = null)
			: base (name)
		{
		}

		public void Add (TestCase test)
		{
			tests.Add (test);
		}

		public int Count {
			get { return tests.Count; }
		}

		public IList<TestCase> Tests {
			get { return tests; }
		}

		#region implemented abstract members of TestCase

		internal override TestInvoker CreateInvoker (TestContext context)
		{
			var invokers = tests.Select (t => t.CreateInvoker (context)).ToArray ();
			return new AggregatedTestInvoker (TestFlags.ContinueOnError, invokers);
		}

		public override Task<bool> Run (
			TestContext context, TestResult result, CancellationToken cancellationToken)
		{
			throw new InvalidOperationException ();
		}

		public override IEnumerable<string> Categories {
			get {
				return tests.SelectMany (t => t.Categories);
			}
		}

		#endregion
	}
}

