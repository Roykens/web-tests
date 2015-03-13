﻿//
// MyClass.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
using System.Threading;
using Xamarin.AsyncTests;
#if !__MOBILE__
using Xamarin.AsyncTests.Console;
#endif
using Xamarin.WebTests.Portable;

[assembly: DependencyProvider (typeof (Xamarin.WebTests.Console.WebDependencyProvider))]
[assembly: AsyncTestSuite (typeof (Xamarin.WebTests.WebTestFeatures), true)]

namespace Xamarin.WebTests.Console
{
	using Server;

	class WebDependencyProvider : IDependencyProvider
	{
		public void Initialize ()
		{
			DependencyInjector.RegisterDependency<IPortableWebSupport> (() => new PortableWebSupportImpl ());
			DependencyInjector.RegisterDependency<NTLMHandler> (() => new NTLMHandler ());
		}

#if !__MOBILE__
		static void Main (string[] args)
		{
			Program.Run (typeof (WebDependencyProvider).Assembly, args);
		}
#endif
	}
}

