//
// AsyncTests.Framework.TestContext
//
// Authors:
//      Martin Baulig (martin.baulig@gmail.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Linq;
using SD = System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTests.Framework {
	using Internal;

	public class TestContext : IDisposable {
		List<TestError> errors;
		List<TestWarning> warnings;
		List<IDisposable> disposables;

		public int DebugLevel {
			get; set;
		}

		public void Debug (int level, string message, params object[] args)
		{
			if (level <= DebugLevel)
				Log (message, args);
		}

		public void Log (string message, params object[] args)
		{
			SD.Debug.WriteLine (string.Format (message, args), "TestSuite");
		}

		public void LogError (TestError error)
		{
			Log (error.ToString ());
		}

		internal void ClearErrors ()
		{
			errors = null;
			warnings = null;
		}

		internal void AddError (string name, Exception error)
		{
			if (errors == null)
				errors = new List<TestError> ();
			errors.Add (new TestError (name, null, error));
		}

		public bool HasErrors {
			get { return errors != null; }
		}

		internal IList<TestError> Errors {
			get {
				return HasErrors ? errors : null;
			}
		}

		public bool HasWarnings {
			get { return warnings != null; }
		}

		public IList<TestWarning> Warnings {
			get {
				return HasWarnings ? warnings : null;
			}
		}

		internal void CheckErrors (string message)
		{
			if (errors == null)
				return;
			throw new TestErrorException (message, errors.ToArray ());
		}

		#region Internal

		internal TestInstance Instance {
			get; set;
		}

		#endregion

		#region Assertions

		/*
		 * By default, Exepct() is non-fatal.  Multiple failed expections will be
		 * collected and a TestErrorException will be thrown when the test method
		 * returns.
		 * 
		 * Use Assert() to immediately abort the test method or set 'AlwaysFatal = true'.
		 * 
		 */

		public bool AlwaysFatal {
			get;
			set;
		}

		public void Warning (string message, params object[] args)
		{
			Warning (string.Format (message, args));
		}

		public void Warning (string message)
		{
			if (warnings == null)
				warnings = new List<TestWarning> ();
			warnings.Add (new TestWarning (message));
		}

		#endregion

		#region Disposing

		public void AutoDispose (IDisposable disposable)
		{
			if (disposable == null)
				return;
			if (disposables == null)
				disposables = new List<IDisposable> ();
			disposables.Add (disposable);
		}

		public void AutoDispose ()
		{
			if (disposables == null)
				return;
			foreach (var disposable in disposables) {
				try {
					disposable.Dispose ();
				} catch (Exception ex) {
					AddError ("Auto-dispose failed", ex);
				}
			}
			disposables = null;
		}

		~TestContext ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			AutoDispose ();
		}

		#endregion
	}
}
