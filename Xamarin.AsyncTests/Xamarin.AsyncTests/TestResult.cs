//
// Xamarin.AsyncTests.Framework.TestResultItem
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.AsyncTests
{
	public class TestResult : INotifyPropertyChanged
	{
		TestName name;
		TestStatus status = TestStatus.None;
		Exception error;
		TestCase test;
		TestResult parent;

		public TestName Name {
			get { return name; }
			protected set {
				lock (this) {
					WantToModify ();
					name = value;
				}
				OnPropertyChanged ("Name");
			}
		}

		public TestStatus Status {
			get { return status; }
			set {
				lock (this) {
					if (value == status)
						return;
					WantToModify ();
					status = value;
				}
				OnPropertyChanged ("Status");
			}
		}

		public Exception Error {
			get { return error; }
			private set {
				lock (this) {
					WantToModify ();
					error = value;
					if (error != null)
						Status = TestStatus.Error;
				}
				OnPropertyChanged ("Error");
			}
		}

		public TestCase Test {
			get { return test; }
			set {
				lock (this) {
					WantToModify ();
					test = value;
				}
				OnPropertyChanged ("Test");
				OnPropertyChanged ("CanRun");
			}
		}

		public bool CanRun {
			get { return test != null; }
		}

		public TestResult (TestName name, Exception error)
			: this (name, TestStatus.Error)
		{
			this.error = error;
		}

		public TestResult (TestName name, TestStatus status = TestStatus.None)
		{
			this.name = name;
			this.status = status;

			messages = new ObservableCollection<string> ();
			((INotifyPropertyChanged)messages).PropertyChanged += (sender, e) => OnMessagesChanged ();
			children = new ObservableCollection<TestResult> ();
			((INotifyPropertyChanged)children).PropertyChanged += (sender, e) => OnChildrenChanged ();
		}

		ObservableCollection<TestResult> children;
		ObservableCollection<string> messages;

		public bool HasChildren {
			get { return children.Count > 0; }
		}

		public IReadOnlyCollection<TestResult> Children {
			get { return children; }
		}

		public IReadOnlyCollection<string> Messages {
			get { return messages; }
		}

		void OnMessagesChanged ()
		{
			OnPropertyChanged ("Messages");
		}

		void OnChildrenChanged ()
		{
			OnPropertyChanged ("Children");
			OnPropertyChanged ("HasChildren");
		}

		public void AddMessage (string message)
		{
			lock (this) {
				WantToModify ();
				messages.Add (message);
			}
		}

		public void AddMessage (string format, params object[] args)
		{
			lock (this) {
				WantToModify ();
				messages.Add (string.Format (format, args));
			}
		}

		public void AddChild (TestResult child)
		{
			lock (this) {
				WantToModify ();
				child.parent = this;
				children.Add (child);
				MergeStatus (child.Status);
			}
		}

		public void Clear ()
		{
			lock (this) {
				children.Clear ();
				messages.Clear ();
				error = null;
				status = TestStatus.None;
			}
			OnPropertyChanged ("Children");
			OnPropertyChanged ("Messages");
			OnPropertyChanged ("Error");
			OnPropertyChanged ("Status");
		}

		void MergeStatus (TestStatus childStatus)
		{
			if (childStatus == TestStatus.None)
				return;

			switch (status) {
			case TestStatus.Canceled:
			case TestStatus.Error:
				break;

			case TestStatus.Ignored:
			case TestStatus.None:
				Status = childStatus;
				break;

			case TestStatus.Success:
				if (childStatus == TestStatus.Error || childStatus == TestStatus.Canceled)
					Status = childStatus;
				break;

			default:
				throw new InvalidOperationException ();
			}
		}

		public void AddError (Exception exception)
		{
			lock (this) {
				WantToModify ();
				if (error == null) {
					Error = exception;
					return;
				}
				var list = new List<Exception> ();
				var aggregated = error as AggregateException;
				if (aggregated != null) {
					list.AddRange (aggregated.InnerExceptions);
					list.Add (exception);
					Error = new AggregateException (aggregated.Message, list);
				} else {
					Error = new AggregateException (exception);
				}
				Status = TestStatus.Error;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

		protected void WantToModify ()
		{
			if (parent != null)
				throw new InvalidOperationException ("Cannot modify TestResult after it's been added to a parent.");
		}

		public override string ToString ()
		{
			return string.Format ("[TestResult: Name={0}, Status={1}, Error={2}]", Name, Status, Error);
		}
	}
}
