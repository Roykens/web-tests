﻿//
// RunCommand.cs
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
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.UI
{
	public class RunCommand : Command<TestResult>
	{
		public readonly TestRunner Runner;

		public RunCommand (TestRunner runner)
			: base (runner, runner.NotifyCanExecute)
		{
			Runner = runner;
		}

		internal sealed override Task<bool> Run (TestResult result, CancellationToken cancellationToken)
		{
			return Task.FromResult (false);
		}

		internal sealed override Task Stop (TestResult result, CancellationToken cancellationToken)
		{
			return Task.FromResult (false);
		}

		internal override Task<TestResult> Start (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public Task<TestResult> Run (TestCase test, TestName name, CancellationToken cancellationToken)
		{
			return Runner.Run (test, name, 0, cancellationToken);
		}
	}
}

