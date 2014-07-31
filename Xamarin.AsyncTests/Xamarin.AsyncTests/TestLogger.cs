﻿//
// TestLogger.cs
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
	public abstract class TestLogger
	{
		protected internal abstract void OnLogEvent (LogEntry entry);

		public void LogDebug (int level, string message)
		{
			OnLogEvent (new LogEntry (LogEntry.EntryKind.Debug, level, message));
		}

		public void LogDebug (int level, string format, params object[] args)
		{
			LogDebug (level, string.Format (format, args));
		}

		public void LogMessage (string message)
		{
			OnLogEvent (new LogEntry (LogEntry.EntryKind.Message, 0, message));
		}

		public void LogMessage (string format, params object[] args)
		{
			LogMessage (string.Format (format, args));
		}

		public void LogError (Exception error)
		{
			OnLogEvent (new LogEntry (LogEntry.EntryKind.Error, 0, error.Message, error));
		}

		public static string Print (object obj)
		{
			return obj != null ? obj.ToString () : "<null>";
		}

		public static TestLogger CreateForResult (TestResult result, TestLogger parent)
		{
			return new TestResultLogger (result, parent);
		}

		class TestResultLogger : TestLogger
		{
			public TestResult Result {
				get;
				private set;
			}

			public TestLogger Parent {
				get;
				private set;
			}

			public TestResultLogger (TestResult result, TestLogger parent = null)
			{
				Result = result;
				Parent = parent;
			}

			protected internal override void OnLogEvent (LogEntry entry)
			{
				if (Parent != null) {
					Parent.OnLogEvent (entry);
					return;
				}

				if (entry.Kind == LogEntry.EntryKind.Error) {
					if (entry.Error != null)
						Result.AddError (entry.Error);
					else
						Result.AddError (new AssertionException (entry.Text));
				} else {
					Result.AddMessage (entry.Text);
				}
			}
		}

	}
}

