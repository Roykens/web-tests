﻿//
// TestFeaturesModel.cs
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	public class TestFeaturesModel : BindableObject
	{
		public TestApp App {
			get;
			private set;
		}

		public readonly BindableProperty ConfigurationProperty = BindableProperty.Create (
			"Configuration", typeof(TestConfiguration), typeof(TestFeaturesModel), null,
			propertyChanged: (bo, o, n) => ((TestFeaturesModel)bo).ReloadFeatures ());

		public TestConfiguration Configuration {
			get { return (TestConfiguration)GetValue (ConfigurationProperty); }
			set { SetValue (ConfigurationProperty, value); }
		}

		ObservableCollection<TestFeatureModel> features;
		public ObservableCollection<TestFeatureModel> Features {
			get { return features; }
		}

		public TestFeaturesModel (TestApp app)
		{
			App = app;
			features = new ObservableCollection<TestFeatureModel> ();
		}

		void ReloadFeatures ()
		{
			features.Clear ();
			if (Configuration == null)
				return;
			foreach (var feature in Configuration.Features.ToArray ()) {
				if (!feature.CanModify)
					continue;
				features.Add (new TestFeatureModel (App, Configuration, feature));
			}
		}
	}
}

