﻿//
// Assert.cs
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
using System.Text;

namespace Xamarin.AsyncTests
{
	using Constraints;

	public static class Assert
	{
		public static void That (object actual, Constraint constraint, string format, params object[] args)
		{
			That (actual, constraint, string.Format (format, args));
		}

		internal static string Print (object value)
		{
			if (value == null)
				return "<null>";
			var svalue = value as string;
			if (svalue != null && string.IsNullOrEmpty (svalue))
				return "<empty>";
			if (svalue != null)
				return '"' + svalue + '"';
			else
				return value.ToString ();
		}

		public static void That (object actual, Constraint constraint, string message)
		{
			string errorMessage;
			if (constraint.Evaluate (actual, out errorMessage))
				return;
			var sb = new StringBuilder ();
			sb.AppendFormat ("AssertionFailed ({0}:{1})", Print (actual), constraint.Print ());
			if (message != null)
				sb.AppendFormat (": {0}", message);
			if (errorMessage != null) {
				sb.AppendLine ();
				sb.Append (errorMessage);
			}
			throw new AssertionException (sb.ToString ());
		}

		public static void Fail (string format, params object[] args)
		{
			Fail (string.Format (format, args));
		}

		public static void Fail (string message)
		{
			throw new AssertionException (message);
		}
	}
}

