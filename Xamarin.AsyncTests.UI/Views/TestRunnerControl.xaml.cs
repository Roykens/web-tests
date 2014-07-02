﻿//
// TestRunnerControl.xaml.cs
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
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	public partial class TestRunnerControl : ContentView
	{
		public TestRunnerControl ()
		{
			InitializeComponent ();
		}

		void OnRun (object sender, EventArgs args)
		{
			var model = (TestRunnerModel)BindingContext;
			model.App.Run (false);
		}

		void OnRepeat (object sender, EventArgs args)
		{
			var model = (TestRunnerModel)BindingContext;
			model.App.Run (true);
		}

		void OnStop (object sender, EventArgs args)
		{
			var model = (TestRunnerModel)BindingContext;
			model.App.Stop ();
		}

		void OnClear (object sender, EventArgs args)
		{
			var model = (TestRunnerModel)BindingContext;
			model.ResultModel.Result.Clear ();
			model.App.Context.ResetStatistics ();
		}

		async void OnOptions (object sender, EventArgs args)
		{
			var model = (TestRunnerModel)BindingContext;
			await Navigation.PushAsync (model.App.OptionsPage);
		}
	}
}

