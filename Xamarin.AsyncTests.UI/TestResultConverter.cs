﻿//
// TestResultConverter.cs
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
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	using Framework;

	public class TestResultConverter : IValueConverter
	{
		#region IValueConverter implementation

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter != null) {
				switch ((string)parameter) {
				case "not-null":
					return value != null;
				case "test-summary":
					return GetTestSummary ((TestResult)value);
				case "not-empty":
					return !string.IsNullOrEmpty ((string)value);
				default:
					throw new InvalidOperationException ();
				}
			}

			if (value is TestStatus) {
				if (targetType.Equals (typeof(Color)))
					return GetColorForStatus ((TestStatus)value);
				else if (targetType.Equals (typeof(string)))
					return value.ToString ();
			} else if (value is TestName) {
				var name = (TestName)value;
				if (string.IsNullOrEmpty (name.FullName))
					return "<null>";
				return name.FullName;
			} else if (value is Exception) {
				if (!targetType.Equals (typeof(string)))
					throw new InvalidOperationException ();
				return value.ToString ();
			} else if (value is KeyValuePair<string,string>) {
				var kvp = (KeyValuePair<string,string>)value;
				if (string.IsNullOrEmpty (kvp.Key))
					return kvp.Value;
				return kvp.Key + " = " + kvp.Value;
			} else if (targetType.Equals (typeof(string))) {
				return value != null ? value.ToString () : "<null>";
			}

			throw new InvalidOperationException ();
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		string GetTestSummary (TestResult result)
		{
			if (!result.HasChildren || !string.IsNullOrEmpty (result.Message))
				return result.Message;

			var numSuccess = result.Children.Count (t => t.Status == TestStatus.Success);
			var numWarnings = result.Children.Count (t => t.Status == TestStatus.Warning);
			var numErrors = result.Children.Count (t => t.Status == TestStatus.Error);

			var sb = new StringBuilder ();
			sb.AppendFormat ("{0} tests passed", numSuccess);
			if (numWarnings > 0)
				sb.AppendFormat (", {0} warnings", numWarnings);
			if (numErrors > 0)
				sb.AppendFormat (", {0} errors", numErrors);
			return sb.ToString ();
		}

		Color GetColorForStatus (TestStatus status)
		{
			switch (status) {
			case TestStatus.Warning:
				return Color.Yellow;
			case TestStatus.Error:
				return Color.Red;
			case TestStatus.Success:
				return Color.Green;
			default:
				return Color.Black;
			}
		}

		#endregion
	}
}

