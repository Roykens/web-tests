﻿//
// SettingsHost.cs
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
using System.Collections.Generic;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.AsyncTests.UI.Forms;

using Android;
using Android.Content;
using Android.Preferences;

namespace Xamarin.WebTests.Async.Android
{
	public class SettingsHost : SettingsBag, ISettingsHost
	{
		const string DictionaryKey = "AppSettings";

		readonly ISharedPreferences preferences;

		public SettingsHost (ISharedPreferences preferences)
		{
			this.preferences = preferences;
		}

		#region ISettingsHost implementation
		public string GetValue (string name)
		{
			string value;
			if (!TryGetValue (name, out value))
				return null;
			return value;
		}

		public override bool TryGetValue (string name, out string value)
		{
			if (!preferences.Contains (name)) {
				value = null;
				return false;
			}

			value = preferences.GetString (name, null);
			return true;
		}

		public override void Add (string key, string value)
		{
			SetValue (key, value);
		}

		protected override void DoSetValue (string name, string value)
		{
			var editor = preferences.Edit ();
			editor.PutString (name, value);
			editor.Commit ();
		}

		public override void RemoveValue (string name)
		{
			var editor = preferences.Edit ();
			editor.Remove (name);
			editor.Commit ();
		}

		public override IReadOnlyDictionary<string, string> Settings {
			get {
				var dict = new Dictionary<string,string> ();
				foreach (var entry in preferences.All) {
					dict.Add (entry.Key, (string)entry.Value);
				}
				return dict;
			}
		}

		SettingsBag ISettingsHost.GetSettings ()
		{
			return this;
		}
		#endregion
	}
}

