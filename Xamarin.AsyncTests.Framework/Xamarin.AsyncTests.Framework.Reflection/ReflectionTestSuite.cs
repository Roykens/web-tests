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
	class ReflectionTestSuite : TestSuite, IPathResolvable, IPathResolver
	{
		public TestApp App {
			get;
			private set;
		}

		public Assembly Assembly {
			get;
			private set;
		}

		public ReflectionTestSuiteBuilder Builder {
			get;
			private set;
		}

		public override TestConfiguration Configuration {
			get { return Builder.Configuration; }
		}

		ReflectionTestSuite (TestApp app, TestName name, Assembly assembly)
			: base (name)
		{
			App = app;
			Assembly = assembly;
			Builder = new ReflectionTestSuiteBuilder (this);

			var rootPath = new TestPath (Builder.Host, null);
			var rootNode = new TestPathNode (Builder.TreeRoot, rootPath);
			test = new PathBasedTestCase (rootNode);
		}

		TestCase test;

		public override TestCase Test {
			get { return test; }
		}

		public IPathResolver GetResolver ()
		{
			return this;
		}

		IPathNode IPathResolver.Node {
			get { return null; }
		}

		public IPathResolvable Resolve (IPathNode node, string parameter)
		{
			if (!node.Identifier.Equals (TestSerializer.TestSuiteIdentifier))
				throw new InternalErrorException ();
			if (!node.ParameterType.Equals (TestSerializer.TestSuiteIdentifier))
				throw new InternalErrorException ();
			return Builder;
		}

		public static Task<TestSuite> Create (TestApp app, Assembly assembly)
		{
			var tcs = new TaskCompletionSource<TestSuite> ();

			Task.Factory.StartNew (() => {
				try {
					var name = new TestName (assembly.GetName ().Name);
					var suite = new ReflectionTestSuite (app, name, assembly);
					tcs.SetResult (suite);
				} catch (Exception ex) {
					tcs.SetException (ex);
				}
			});

			return tcs.Task;
		}
	}
}

