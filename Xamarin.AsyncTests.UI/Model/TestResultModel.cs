﻿//
// ITestResultModel.cs
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	using Framework;

	public class TestResultModel : BindableObject
	{
		public UITestApp App {
			get;
			private set;
		}

		public TestResult Result {
			get;
			private set;
		}

		public bool IsRoot {
			get;
			private set;
		}

		public Color StatusColor {
			get { return TestResultConverter.GetColorForStatus (Result.Status); }
		}

		public TestResultModel (UITestApp app, TestResult result, bool isRoot)
		{
			App = app;
			Result = result;
			IsRoot = isRoot;

			result.PropertyChanged += (sender, e) => {
				OnPropertyChanged ("StatusColor");
			};
		}

		public IEnumerable<TestResultModel> GetChildren ()
		{
			foreach (var child in Result.Children) {
				if (Filter (child))
					yield return new TestResultModel (App, child, false);
			}
		}

		bool Filter (TestResult result)
		{
			if (App.Options.HideIgnoredTests) {
				if (result.Status == TestStatus.Ignored || result.Status == TestStatus.None)
					return false;
			}
			if (App.Options.HideSuccessfulTests && result.Status == TestStatus.Success)
				return false;
			return true;
		}
	}
}

