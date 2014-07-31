﻿//
// ReflectionTestSuite.cs
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
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework.Reflection
{
	class ReflectionTestSuite : TestSuite
	{
		List<ReflectionTest> tests;
		TestInvoker invoker;
		TestConfiguration configuration;

		ReflectionTestSuite (TestName name)
			: base (name)
		{
			tests = new List<ReflectionTest> ();
		}

		public override TestConfiguration Configuration {
			get { return configuration; }
		}

		public static Task<TestSuite> Create (TestApp ctx, Assembly assembly)
		{
			var tcs = new TaskCompletionSource<TestSuite> ();

			Task.Factory.StartNew (() => {
				try {
					var name = new TestName (assembly.GetName ().Name);
					var suite = new ReflectionTestSuite (name);
					suite.DoLoadAssembly (ctx, assembly);
					ctx.CurrentTestSuite = suite;
					tcs.SetResult (suite);
				} catch (Exception ex) {
					tcs.SetException (ex);
				}
			});

			return tcs.Task;
		}

		void DoLoadAssembly (TestApp ctx, Assembly assembly)
		{
			foreach (var type in assembly.ExportedTypes) {
				var tinfo = type.GetTypeInfo ();
				var attr = tinfo.GetCustomAttribute<AsyncTestFixtureAttribute> (true);
				if (attr == null)
					continue;

				var fixture = new ReflectionTestFixture (this, attr, tinfo);
				tests.Add (fixture);
			}

			var invokers = tests.Select (t => t.Invoker).ToArray ();
			invoker = AggregatedTestInvoker.Create (TestFlags.ContinueOnError, invokers);

			var suiteAttr = assembly.GetCustomAttribute<AsyncTestSuiteAttribute> ();
			if (suiteAttr != null) {
				var config = (ITestConfiguration)Activator.CreateInstance (suiteAttr.Type);
				configuration = TestConfiguration.FromTestSuite (config);
			}
		}

		internal override Task<bool> Run (TestContext ctx, CancellationToken cancellationToken)
		{
			return invoker.Invoke (ctx, null, cancellationToken);
		}
	}
}

