﻿//
// AggregatedTestRunner2.cs
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

namespace Xamarin.AsyncTests.Framework.Internal
{
	class AggregatedTestInvoker : TestInvoker
	{
		public TestFlags Flags {
			get;
			private set;
		}

		public TestHost Host {
			get;
			private set;
		}

		public bool ContinueOnError {
			get { return (Flags & TestFlags.ContinueOnError) != 0; }
		}

		public AggregatedTestInvoker (string name, TestFlags flags, TestHost host, params TestInvoker[] invokers)
			: base (name)
		{
			Flags = flags;
			Host = host;
			innerTestInvokers = new List<TestInvoker> ();
			innerTestInvokers.AddRange (invokers);
		}

		public IList<TestInvoker> InnerTestInvokers {
			get { return innerTestInvokers; }
		}

		List<TestInvoker> innerTestInvokers;

		static string Print (object obj)
		{
			return obj != null ? obj.ToString () : "<null>";
		}

		async Task<bool> SetUp (TestContext context, TestResultCollection result, CancellationToken cancellationToken)
		{
			context.Debug (3, "SetUp({0}): {1} {2} {3}", Name, Print (Host), Flags, Print (context.Instance));

			if (Host == null)
				return true;

			try {
				context.CurrentTestName.PushName ("SetUp");
				await Host.CreateInstance (context, cancellationToken);
				return true;
			} catch (Exception ex) {
				var error = context.CreateTestResult (ex);
				result.AddChild (error);
				return false;
			} finally {
				context.CurrentTestName.PopName ();
			}
		}

		async Task<bool> ReuseInstance (TestContext context, TestResultCollection result, CancellationToken cancellationToken)
		{
			context.Debug (3, "ReuseInstance({0}): {1} {2} {3}", Name, Print (Host), Flags, Print (context.Instance));

			var parameterizedHost = Host as ParameterizedTestHost;
			if (parameterizedHost == null)
				return true;

			try {
				context.CurrentTestName.PushName ("ReuseInstance");
				await parameterizedHost.ReuseInstance (context, cancellationToken);
				return true;
			} catch (Exception ex) {
				var error = context.CreateTestResult (ex);
				result.AddChild (error);
				return false;
			} finally {
				context.CurrentTestName.PopName ();
			}
		}

		async Task<bool> InvokeInner (TestContext context, TestResultCollection result, TestInvoker invoker, CancellationToken cancellationToken)
		{
			context.Debug (3, "Running({0}): {1} {2}", context.CurrentTestName.GetFullName (), Print (Host), invoker);

			try {
				cancellationToken.ThrowIfCancellationRequested ();
				var success = await invoker.Invoke (context, result, cancellationToken);
				return success || ContinueOnError;
			} catch (Exception ex) {
				var error = context.CreateTestResult (ex);
				result.AddChild (error);
				return ContinueOnError;
			}
		}

		async Task<bool> TearDown (TestContext context, TestResultCollection result, CancellationToken cancellationToken)
		{
			context.Debug (3, "TearDown({0}): {1} {2} {3}", Name, Print (Host), Flags, Print (context.Instance));

			if (Host == null)
				return true;

			try {
				context.CurrentTestName.PushName ("TearDown");
				await Host.DestroyInstance (context, cancellationToken);
				return true;
			} catch (Exception ex) {
				var error = context.CreateTestResult (ex);
				result.AddChild (error);
				return false;
			} finally {
				context.CurrentTestName.PopName ();
			}
		}
			
		public sealed override async Task<bool> Invoke (
			TestContext context, TestResultCollection result, CancellationToken cancellationToken)
		{
			if (InnerTestInvokers.Count == 0)
				return true;

			var oldResult = context.CurrentResult;

			if ((Flags & (TestFlags.Browsable | TestFlags.FlattenHierarchy)) == TestFlags.Browsable) {
				var child = new TestResultCollection (context.GetCurrentTestName ());
				result.AddChild (child);
				result = child;
			}

			if (!await SetUp (context, result, cancellationToken)) {
				context.CurrentResult = oldResult;
				return false;
			}

			bool success = true;
			var innerRunners = new LinkedList<TestInvoker> (InnerTestInvokers);
			var current = innerRunners.First;

			while (success && current != null) {
				if (cancellationToken.IsCancellationRequested)
					break;

				success = await ReuseInstance (context, result, cancellationToken);
				if (!success)
					break;

				var innerResult = result;
				var parameterizedHost = Host as ParameterizedTestHost;
				if (parameterizedHost != null) {
					var parameterizedInstance = (ParameterizedTestInstance)context.Instance;
					context.CurrentTestName.PushParameter (parameterizedHost.ParameterName, parameterizedInstance.Current);
				}

				var invoker = current.Value;
				success = await InvokeInner (context, innerResult, invoker, cancellationToken);
				context.Debug (5, "TEST: {0} {1} {2}", this, Flags, success);

				if (parameterizedHost != null)
					context.CurrentTestName.PopParameter ();

				if (!success)
					break;

				if (parameterizedHost == null || !parameterizedHost.CanReuseInstance (context))
					current = current.Next;
			}

			if (!await TearDown (context, result, cancellationToken))
				success = false;

			context.CurrentResult = oldResult;

			cancellationToken.ThrowIfCancellationRequested ();
			return success;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: Flags={1}, Host={2}]", GetType ().Name, Flags, Host);
		}
	}
}

