//
// Xamarin.AsyncTests.Framework.TestCase
//
// Authors:
//      Martin Baulig (martin.baulig@gmail.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework {
	using Internal;

	public abstract class TestCase
	{
		public TestName Name {
			get;
			private set;
		}

		public TestCase (TestName name)
		{
			Name = name;
		}

		internal abstract TestInvoker CreateInvoker ();

		public Task<bool> Run (TestContext ctx, TestResult result,
			CancellationToken cancellationToken)
		{
			var invoker = CreateInvoker ();
			return invoker.Invoke (ctx, null, result, cancellationToken);
		}

		public TestCase CreateRepeatedTest (TestContext ctx, int count)
		{
			var invoker = CreateInvoker ();
			var repeatHost = new RepeatedTestHost (count, TestFlags.ContinueOnError | TestFlags.Browsable, "$iteration");
			var repeatInvoker = new AggregatedTestInvoker (repeatHost, invoker);
			var outerInvoker = new ProxyTestInvoker (Name, repeatInvoker);
			return new InvokableTestCase (this, outerInvoker);
		}

		public TestCase CreateProxy (TestContext ctx, TestName proxy)
		{
			var invoker = CreateInvoker ();
			var proxyInvoker = new ProxyTestInvoker (proxy, invoker);
			return new InvokableTestCase (this, proxyInvoker);
		}
	}
}
