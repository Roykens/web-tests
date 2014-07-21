﻿//
// InvocationContext.cs
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

namespace Xamarin.AsyncTests
{
	public sealed class InvocationContext : ITestLogger
	{
		public TestContext Context {
			get;
			private set;
		}

		public TestName Name {
			get;
			private set;
		}

		public TestResult Result {
			get;
			private set;
		}

		internal ITestLogger Logger {
			get;
			private set;
		}

		public InvocationContext (TestContext context, ITestLogger logger, TestName name, TestResult result)
		{
			Context = context;
			Logger = logger;
			Name = name;
			Result = result;
		}

		public void OnError (Exception error)
		{
			Result.AddError (error);
			Context.Statistics.OnException (Name, error);
			Logger.LogError (error);
		}

		#region ITestLogger implementation

		public void LogDebug (int level, string message)
		{
			Logger.LogDebug (level, message);
		}

		public void LogDebug (int level, string format, params object[] args)
		{
			Logger.LogDebug (level, format, args);
		}

		public void LogMessage (string message)
		{
			Logger.LogMessage (message);
		}

		public void LogMessage (string message, params object[] args)
		{
			Logger.LogMessage (string.Format (message, args));
		}

		public void LogError (Exception error)
		{
			Logger.LogError (error);
		}

		#endregion
	}
}
