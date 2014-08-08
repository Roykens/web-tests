﻿//
// CollectionTestInvoker.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework
{
	class CollectionTestInvoker : AggregatedTestInvoker
	{
		public List<TestInvoker> InnerInvokers {
			get;
			private set;
		}

		public CollectionTestInvoker (TestFlags flags, IEnumerable<TestInvoker> invokers)
			: base (flags)
		{
			InnerInvokers = new List<TestInvoker> (invokers);
		}

		public sealed override async Task<bool> Invoke (
			TestContext ctx, TestInstance instance, CancellationToken cancellationToken)
		{
			if (InnerInvokers.Count == 0)
				return true;
			if (cancellationToken.IsCancellationRequested)
				return false;

			ctx.LogDebug (10, "Invoke({0}): {1} {2} {3}", ctx.Name,
				Flags, TestLogger.Print (instance), InnerInvokers.Count);

			bool success = true;
			foreach (var invoker in InnerInvokers) {
				if (cancellationToken.IsCancellationRequested)
					break;

				ctx.LogDebug (10, "InnerInvoke({0}): {1} {2} {3}", ctx.Name,
					TestLogger.Print (instance), invoker, InnerInvokers.Count);

				success = await InvokeInner (ctx, instance, invoker, cancellationToken);

				ctx.LogDebug (10, "InnerInvoke({0}) done: {1} {2}", ctx.Name,
					TestLogger.Print (instance), success);

				if (!success)
					break;
			}

			return success;
		}
	}
}

