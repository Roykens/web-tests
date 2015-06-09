﻿//
// ReflectionTestFramework.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework.Reflection
{
	using Portable;

	class ReflectionTestFramework : TestFramework
	{
		public Assembly RootAssembly {
			get;
			private set;
		}

		public Assembly Assembly {
			get;
			private set;
		}

		public Assembly[] Dependencies {
			get;
			private set;
		}

		public override string Name {
			get { return name; }
		}

		public override ITestConfigurationProvider ConfigurationProvider {
			get { return provider; }
		}

		string name;
		ITestConfigurationProvider provider;

		public ReflectionTestFramework (Assembly assembly, params Assembly[] dependencies)
		{
			Assembly = RootAssembly = assembly;
			Dependencies = dependencies;

			ResolveDependencies ();
			CheckDependencies ();
			Resolve ();
		}

		void ResolveDependencies ()
		{
			foreach (var asm in Dependencies) {
				foreach (var cattr in asm.GetCustomAttributes<DependencyProviderAttribute> ()) {
					var provider = (IDependencyProvider)Activator.CreateInstance (cattr.Type);
					provider.Initialize ();
				}
			}
		}

		void Resolve ()
		{
			DependencyInjector.RegisterAssembly (Assembly);
			var cattrs = RootAssembly.GetCustomAttributes<AsyncTestSuiteAttribute> ().ToList ();
			if (cattrs.Count == 0)
				throw new InternalErrorException ("Assembly '{0}' is not a Xamarin.AsyncTests test suite.", Assembly);
			else if (cattrs.Count > 1)
				throw new InternalErrorException ("Assembly '{0}' contains multiple '[AsyncTestSuite]' attributes.", Assembly);
			var cattr = cattrs.First ();
			var type = cattr.Type;

			if (cattr.IsReference) {
				Assembly = type.GetTypeInfo ().Assembly;
				DependencyInjector.RegisterAssembly (Assembly);
				cattrs = Assembly.GetCustomAttributes<AsyncTestSuiteAttribute> ().ToList ();
				if (cattrs.Count == 0)
					throw new InternalErrorException ("Referenced assembly '{0}' (referenced by '{1}') is not a Xamarin.AsyncTests test suite.", Assembly, RootAssembly);
				else if (cattrs.Count > 1)
					throw new InternalErrorException ("Referenced assembly '{0}' (referenced by '{1}') contains multiple '[AsyncTestSuite]' attributes.", Assembly, RootAssembly);
				cattr = cattrs.First ();

				if (cattr.IsReference)
					throw new InternalErrorException ("Assembly '{0}' references '{1}', which is a reference itself.", RootAssembly, Assembly);
				type = cattr.Type;
			} else {
				Assembly = RootAssembly;
			}

			provider = (ITestConfigurationProvider)DependencyInjector.Get (type);
			name = Assembly.GetName ().Name;
		}

		void CheckDependencies ()
		{
			var cattrs = Assembly.GetCustomAttributes<RequireDependencyAttribute> ();
			foreach (var cattr in cattrs) {
				if (!DependencyInjector.IsAvailable (cattr.Type))
					throw new InternalErrorException ("Missing '{0}' dependency.", cattr.Type);
			}
		}
	}
}

