﻿//
// OptionsModel.cs
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
using Xamarin.AsyncTests.Framework;

namespace Xamarin.AsyncTests.UI
{
	using Binding;

	public class OptionsModel : BindableObject
	{
		public UITestApp App {
			get;
			private set;
		}

		bool repeat;
		int repeatCount;
		int logLevel;
		bool hideIgnored, hideSuccessful;

		public bool Repeat {
			get { return repeat; }
			set {
				if (repeat == value)
					return;
				repeat = value;
				App.Settings.Repeat = value;
				OnPropertyChanged ();
			}
		}

		public int RepeatCount {
			get { return repeatCount; }
			set {
				if (repeatCount == value)
					return;
				repeatCount = value;
				App.Settings.RepeatCount = value;
				OnPropertyChanged ();
			}
		}

		public int LogLevel {
			get { return logLevel; }
			set {
				if (logLevel == value)
					return;
				logLevel = value;
				App.Settings.LogLevel = value;
				App.Logger.LogLevel = value;
				OnPropertyChanged ();
			}
		}

		public bool HideIgnoredTests {
			get { return hideIgnored; }
			set {
				if (hideIgnored == value)
					return;
				hideIgnored = value;
				App.Settings.HideIgnoredTests = value;
				OnPropertyChanged ();
			}
		}

		public bool HideSuccessfulTests {
			get { return hideSuccessful; }
			set {
				if (hideSuccessful == value)
					return;
				hideSuccessful = value;
				App.Settings.HideSuccessfulTests = value;
				OnPropertyChanged ();
			}
		}

		public OptionsModel (UITestApp app)
		{
			App = app;

			app.Settings.PropertyChanged += (sender, e) => LoadSettings ();
			LoadSettings ();
		}

		void LoadSettings ()
		{
			repeat = App.Settings.Repeat;
			repeatCount = App.Settings.RepeatCount;
			logLevel = App.Settings.LogLevel;
			hideIgnored = App.Settings.HideIgnoredTests;
			hideSuccessful = App.Settings.HideSuccessfulTests;
			App.Logger.LogLevel = LogLevel;

			OnPropertyChanged ("Repeat");
			OnPropertyChanged ("RepeatCount");
			OnPropertyChanged ("LogLevel");
			OnPropertyChanged ("HideIgnoredTests");
			OnPropertyChanged ("HideSuccessfulTests");
		}
	}
}

