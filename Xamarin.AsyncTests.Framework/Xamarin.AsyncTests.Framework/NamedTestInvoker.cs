﻿//
// NamedTestInvoker.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	class NamedTestInvoker : AggregatedTestInvoker
	{
		public NamedTestHost Host {
			get;
			private set;
		}

		public TestInvoker Inner {
			get;
			private set;
		}

		public NamedTestInvoker (NamedTestHost host, TestInvoker inner)
		{
			Host = host;
			Inner = inner;
		}

		public static NamedTestInvoker Create (string name, TestInvoker inner)
		{
			return new NamedTestInvoker (new NamedTestHost (name), inner);
		}

		public static NamedTestInvoker Create (TestName name, TestInvoker inner)
		{
			return new NamedTestInvoker (new NamedTestHost (name), inner);
		}

		NamedTestInstance SetUp (TestContext ctx, TestInstance instance, TestResult result)
		{
			ctx.Debug (3, "SetUp({0}): {1} {2}", TestInstance.GetTestName (instance),
				ctx.Print (Host), ctx.Print (instance));

			try {
				return (NamedTestInstance)Host.CreateInstance (ctx, instance);
			} catch (OperationCanceledException) {
				result.Status = TestStatus.Canceled;
				return null;
			} catch (Exception ex) {
				result.AddError (ex);
				return null;
			}
		}

		bool TearDown (TestContext ctx, NamedTestInstance instance, TestResult result)
		{
			ctx.Debug (3, "TearDown({0}): {1} {2}", TestInstance.GetTestName (instance),
				ctx.Print (Host), ctx.Print (instance));

			try {
				instance.Destroy (ctx);
				return true;
			} catch (OperationCanceledException) {
				result.Status = TestStatus.Canceled;
				return false;
			} catch (Exception ex) {
				result.AddError (ex);
				return false;
			}
		}

		public override async Task<bool> Invoke (
			TestContext ctx, TestInstance instance, TestResult result, CancellationToken cancellationToken)
		{
			var innerInstance = SetUp (ctx, instance, result);
			if (innerInstance == null)
				return false;

			var success = await InvokeInner (ctx, innerInstance, result, Inner, cancellationToken);

			if (!TearDown (ctx, innerInstance, result))
				success = false;

			return success;
		}
	}
}

