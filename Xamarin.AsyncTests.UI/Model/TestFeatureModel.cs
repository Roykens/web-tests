﻿//
// TestFeatureModel.cs
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
using Xamarin.Forms;
using Xamarin.AsyncTests;

namespace Xamarin.AsyncTests.UI
{
	public class TestFeatureModel : BindableObject
	{
		public TestApp App {
			get;
			private set;
		}

		public TestConfiguration Configuration {
			get;
			private set;
		}

		public TestFeature Feature {
			get;
			private set;
		}

		public bool IsEnabled {
			get {
				return Configuration.IsEnabled (Feature);
			}
			set {
				if (value == IsEnabled)
					return;
				if (value)
					Configuration.Enable (Feature);
				else
					Configuration.Disable (Feature);
				if (App.SettingsHost != null)
					App.SettingsHost.SetValue (Path, value.ToString ());
				OnPropertyChanged ("Feature");
			}
		}

		public string Path {
			get;
			private set;
		}

		public TestFeatureModel (TestApp app, TestConfiguration config, TestFeature feature)
		{
			App = app;
			Configuration = config;
			Feature = feature;
			Path = "/Feature/" + feature.Name;
			LoadConfiguration ();
		}

		void LoadConfiguration ()
		{
			if (App.SettingsHost == null)
				return;

			var value = App.SettingsHost.GetValue (Path);
			if (value != null) {
				if (bool.Parse (value))
					Configuration.Enable (Feature);
				else
					Configuration.Disable (Feature);
			}
		}
	}
}

