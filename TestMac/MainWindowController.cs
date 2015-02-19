﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Foundation;
using AppKit;

using Xamarin.AsyncTests.UI;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.AsyncTests.Server;

namespace TestMac
{
	public partial class MainWindowController : NSWindowController
	{
		public MainWindowController (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
		}

		public MainWindowController () : base ("MainWindow")
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			Initialize ();
		}

		void Initialize ()
		{
			var app = AppDelegate.Instance;

			app.MacUI.TestRunner.Run.NotifyStateChanged.StateChanged += (sender, e) => CanRun = e;
			app.MacUI.TestRunner.Stop.NotifyStateChanged.StateChanged += (sender, e) => CanStop = e;

			UIBinding.Bind (app.MacUI.ServerManager.StatusMessage, ServerStatusMessage);
			UIBinding.Bind (app.MacUI.TestRunner.StatusMessage, ServerStatusMessage);

			SplitView.AddSubview (TestResultList);
			SplitView.AddSubview (TestResultDetails);

			app.MacUI.TestRunner.TestResult.PropertyChanged += (sender, e) => OnTestSuiteLoaded (e);
		}

		void OnTestSuiteLoaded (TestResult result)
		{
			if (result != null) {
				var node = new TestResultNode (result);
				node.Model.IsRoot = true;
				TestResultController.AddObject (node);
			}
		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}

		[Export ("Run:testCase:indexPath:")]
		public async void Run (TestCaseModel test, NSIndexPath index)
		{
			Console.WriteLine ("RUN: {0} {1}", test, index);
			var ui = AppDelegate.Instance.MacUI;

			var parameters = new RunParameters (test.Test);
			var result = await ui.TestRunner.Run.Execute (parameters);
			Console.WriteLine ("RESULT: {0}", result);

			var model = new TestResultNode (result);
			TestResultController.InsertObject (model, index);
		}

		bool canRun;
		bool canStop;

		[Export ("CanRun")]
		public bool CanRun {
			get { return canRun; }
			set {
				WillChangeValue ("CanRun");
				canRun = value;
				DidChangeValue ("CanRun");
			}
		}

		[Export ("Clear:testResult:indexPath:")]
		public void Clear (TestResultModel result, NSIndexPath indexPath)
		{
			if (!result.IsRoot)
				TestResultController.RemoveObjectAtArrangedObjectIndexPath (indexPath);
		}

		[Export ("ClearAll:")]
		public void ClearAll ()
		{
		}

		[Export ("SaveTestResult:")]
		public void SaveResult ()
		{
			// Get root node from current selection
			var indexPath = TestResultController.SelectionIndexPath;
			var index = indexPath.IndexAtPosition (0);

			var internalArray = (NSArray)TestResultController.Content;
			var rootNode = internalArray.GetItem<TestResultNode> ((nint)index);

			var result = rootNode.Model.Result;
			var element = Connection.WriteTestResult (result);

			var settings = new XmlWriterSettings { Indent = true };
			using (var writer = XmlTextWriter.Create ("TestResult.xml", settings)) {
				element.WriteTo (writer);
			}
		}

		[Export ("LoadTestResult:")]
		public void LoadResult ()
		{
			using (var reader = XmlTextReader.Create ("TestResult.xml")) {
				var doc = XDocument.Load (reader);
				var root = doc.Root;
				var result = Connection.ReadTestResult (root);

				var model = new TestResultNode (result);
				TestResultController.AddObject (model);
			}
		}

		[Export ("CanStop")]
		public bool CanStop {
			get { return canStop; }
			set {
				WillChangeValue ("CanStop");
				canStop = value;
				DidChangeValue ("CanStop");
			}
		}

		[Export ("Stop:")]
		public async void Stop ()
		{
			var ui = AppDelegate.Instance.MacUI;
			await ui.TestRunner.Stop.Execute ();
		}
	}
}
