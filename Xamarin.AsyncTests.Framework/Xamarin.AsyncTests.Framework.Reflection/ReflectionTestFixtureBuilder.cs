﻿//
// ReflectionTestFixtureBuilder.cs
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
using System.Reflection;
using System.Collections.Generic;

namespace Xamarin.AsyncTests.Framework.Reflection
{
	class ReflectionTestFixtureBuilder : TestCollectionBuilder
	{
		new public ReflectionTestSuiteBuilder Suite {
			get;
			private set;
		}

		public TypeInfo Type {
			get;
			private set;
		}

		public HeavyTestHost FixtureHost {
			get;
			private set;
		}

		public AsyncTestAttribute Attribute {
			get;
			private set;
		}

		public override TestBuilder Parent {
			get { return Suite; }
		}

		public ReflectionTestFixtureBuilder (ReflectionTestSuiteBuilder suite, AsyncTestAttribute attr, TypeInfo type)
			: base (suite.Suite, new TestName (type.Name), ReflectionHelper.CreateTestFilter (null, ReflectionHelper.GetTypeInfo (type)))
		{
			Suite = suite;
			Type = type;
			Attribute = attr;
			Resolve ();
		}

		protected override IEnumerable<TestBuilder> ResolveChildren ()
		{
			foreach (var method in Type.DeclaredMethods) {
				if (method.IsStatic || !method.IsPublic)
					continue;
				var attr = method.GetCustomAttribute<AsyncTestAttribute> (true);
				if (attr == null)
					continue;

				yield return new ReflectionTestCaseBuilder (this, attr, method);
			}
		}

		protected override IEnumerable<TestHost> CreateParameterHosts ()
		{
			yield return new FixtureInstanceTestHost (this);

			var properties = Type.DeclaredProperties.ToArray ();
			for (int i = 0; i < properties.Length; i++) {
				var host = (ParameterizedTestHost)ReflectionHelper.ResolveParameter (this, properties [i]);
				if (host.Serializer == null) {
					ReflectionHelper.ResolveParameter (this, properties [i]);
				}
				yield return new ReflectionPropertyHost (this, properties [i], host);
			}

			if (Attribute.Repeat != 0)
				yield return ReflectionHelper.CreateRepeatHost (Attribute.Repeat);
		}

		class FixtureInstanceTestHost : HeavyTestHost
		{
			public ReflectionTestFixtureBuilder Builder {
				get;
				private set;
			}

			public FixtureInstanceTestHost (ReflectionTestFixtureBuilder builder)
				: base (null)
			{
				Flags = TestFlags.ContinueOnError;
				Builder = builder;
			}

			internal override TestInstance CreateInstance (TestInstance parent)
			{
				var instance = Activator.CreateInstance (Builder.Type.AsType ());
				return new FixtureTestInstance (this, instance, parent);
			}

			internal override bool Serialize (XElement node, TestInstance instance)
			{
				return true;
			}

			internal override TestInvoker Deserialize (XElement node, TestInvoker invoker)
			{
				return CreateInvoker (invoker);
			}
		}
	}
}
