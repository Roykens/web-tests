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
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{
	public class OptionsModel : BindableObject
	{
		public TestApp App {
			get;
			private set;
		}

		bool repeat;
		int repeatCount;
		string repeatCountEntry;

		public bool Repeat {
			get { return repeat; }
			set {
				if (repeat == value)
					return;
				repeat = value;
				SaveSettings ();
				OnPropertyChanged ("Repeat");
			}
		}

		public int RepeatCount {
			get { return repeatCount; }
			set {
				if (repeatCount == value)
					return;
				repeatCount = value;
				RepeatCountEntry = value.ToString ();
				SaveSettings ();
				OnPropertyChanged ("RepeatCount");
			}
		}

		public string RepeatCountEntry {
			get { return repeatCountEntry; }
			set {
				repeatCountEntry = value;
				OnPropertyChanged ("RepeatCountEntry");
			}
		}

		public OptionsModel (TestApp app)
		{
			App = app;

			LoadSettings ();
		}

		void LoadSettings ()
		{
			if (App.SettingsHost == null)
				return;

			var repeatCountValue = App.SettingsHost.GetValue ("RepeatCount");
			if (repeatCountValue != null) {
				repeatCount = int.Parse (repeatCountValue);
				repeatCountEntry = repeatCount.ToString ();
			}

			var repeatValue = App.SettingsHost.GetValue ("Repeat");
			if (repeatValue != null)
				repeat = bool.Parse (repeatValue);

			OnPropertyChanged ("RepeatCount");
			OnPropertyChanged ("RepeatCountEntry");
			OnPropertyChanged ("Repeat");
		}

		void SaveSettings ()
		{
			if (App.SettingsHost == null)
				return;

			App.SettingsHost.SetValue ("RepeatCount", RepeatCount.ToString ());
			App.SettingsHost.SetValue ("Repeat", Repeat.ToString ());
		}
	}
}

