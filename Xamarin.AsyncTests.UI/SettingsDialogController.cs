﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Foundation;
using AppKit;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;
using Xamarin.AsyncTests.Portable;

namespace Xamarin.AsyncTests.MacUI
{
	public partial class SettingsDialogController : NSWindowController
	{
		NSMutableArray serverModeArray;
		ServerModeModel currentServerMode;
		NSMutableArray testCategoriesArray;
		NSMutableArray testFeaturesArray;
		TestCategoryModel currentCategory;

		public static MacUI MacUI {
			get { return MacUI.Instance; }
		}

		public static SettingsBag SettingsBag {
			get { return MacUI.Settings; }
		}

		public TestConfiguration Configuration {
			get;
			private set;
		}

		public SettingsDialogController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		[Export ("initWithCoder:")]
		public SettingsDialogController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		public SettingsDialogController () : base ("SettingsDialog")
		{
			Initialize ();
		}

		void Initialize ()
		{
			serverModeArray = new NSMutableArray ();
			serverModeArray.Add (new ServerModeModel (ServerMode.WaitForConnection, "Wait for connection"));
			serverModeArray.Add (new ServerModeModel (ServerMode.Local, "Run locally"));
			serverModeArray.Add (new ServerModeModel (ServerMode.Android, "Connect to Android"));
			serverModeArray.Add (new ServerModeModel (ServerMode.iOS, "Connect to iOS"));

			string currentMode;
			if (!SettingsBag.TryGetValue ("ServerMode", out currentMode))
				currentMode = "WaitForConnection";

			for (nuint i = 0; i < serverModeArray.Count; i++) {
				var model = serverModeArray.GetItem<ServerModeModel> (i);
				if (currentServerMode == null || model.Mode.ToString ().Equals (currentMode))
					currentServerMode = model;
			}

			testCategoriesArray = new NSMutableArray ();
			allCategory = new TestCategoryModel (TestCategory.All);
			currentCategory = allCategory;
			testCategoriesArray.Add (allCategory);

			testFeaturesArray = new NSMutableArray ();
		}

		TestCategoryModel allCategory;

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			var ui = MacUI.Instance;
			ui.ServerManager.TestSession.PropertyChanged += (sender, e) => OnTestSessionChanged (e);
			OnTestSessionChanged (ui.ServerManager.TestSession.Value);
		}

		void OnTestSessionChanged (TestSession session)
		{
			if (session != null) {
				Configuration = session.Configuration;
				foreach (var category in Configuration.Categories) {
					var model = new TestCategoryModel (category);
					CategoriesController.AddObject (model);
					if (category == Configuration.CurrentCategory)
						CurrentCategory = model;
				}

				foreach (var feature in Configuration.Features) {
					var model = new TestFeatureModel (feature);
					FeaturesController.AddObject (model);
				}
			} else {
				Configuration = null;
				var categoryRange = new NSRange (1, CategoriesController.ArrangedObjects ().Length - 1);
				CategoriesController.Remove (NSIndexSet.FromNSRange (categoryRange));

				var featureRange = new NSRange (0, FeaturesController.ArrangedObjects ().Length);
				FeaturesController.Remove (NSIndexSet.FromNSRange (featureRange));

				CurrentCategory = allCategory;
			}
		}

		public new SettingsDialog Window {
			get { return (SettingsDialog)base.Window; }
		}

		[Export ("TestCategoriesArray")]
		public NSMutableArray TestCategoriesArray {
			get { return testCategoriesArray; }
			set { testCategoriesArray = value; }
		}

		[Export ("ServerModeArray")]
		public NSMutableArray ServerModeArray {
			get { return serverModeArray; }
			set { serverModeArray = value; }
		}

		[Export ("CurrentServerMode")]
		public ServerModeModel CurrentServerMode {
			get { return currentServerMode; }
			set {
				WillChangeValue ("CurrentServerMode");
				currentServerMode = value;
				SettingsBag.SetValue ("ServerMode", value.Mode.ToString ());
				DidChangeValue ("CurrentServerMode");
			}
		}

		[Export ("CurrentCategory")]
		public TestCategoryModel CurrentCategory {
			get { return currentCategory; }
			set {
				WillChangeValue ("CurrentCategory");
				currentCategory = value;
				if (Configuration != null)
					Configuration.CurrentCategory = value.Category;
				DidChangeValue ("CurrentCategory");
			}
		}

		[Export ("TestFeaturesArray")]
		public NSMutableArray TestFeaturesArray {
			get { return testFeaturesArray; }
			set { testFeaturesArray = value; }
		}

		[Export ("RepeatCount")]
		public int RepeatCount {
			get { return SettingsBag.RepeatCount; }
			set {
				WillChangeValue ("RepeatCount");
				SettingsBag.RepeatCount = value;
				DidChangeValue ("RepeatCount");
			}
		}

		[Export ("LogLevel")]
		public int LogLevel {
			get { return SettingsBag.LogLevel; }
			set {
				WillChangeValue ("LogLevel");
				SettingsBag.LogLevel = value;
				DidChangeValue ("LogLevel");
			}
		}

		const string ArgumentsKey = "Arguments";

		[Export (ArgumentsKey)]
		public string Arguments {
			get {
				string arguments;
				if (!SettingsBag.TryGetValue (ArgumentsKey, out arguments))
					arguments = null;
				return arguments;
			}
			set {
				WillChangeValue (ArgumentsKey);
				SettingsBag.SetValue (ArgumentsKey, value);
				DidChangeValue (ArgumentsKey);
			}
		}

		const string MonoRuntimeKey = "MonoRuntime";
		const string DefaultMonoRuntime = "/Library/Frameworks/Mono.framework/Versions/Current";

		[Export (MonoRuntimeKey)]
		public string MonoRuntime {
			get {
				string runtime;
				if (SettingsBag.TryGetValue (MonoRuntimeKey, out runtime))
					return runtime;

				runtime = DefaultMonoRuntime;
				SettingsBag.SetValue (MonoRuntimeKey, runtime);
				return runtime;
			}
			set {
				WillChangeValue (MonoRuntimeKey);
				SettingsBag.SetValue (MonoRuntimeKey, value);
				DidChangeValue (MonoRuntimeKey);
			}
		}

		const string LauncherPathKey = "LauncherPath";

		[Export (LauncherPathKey)]
		public string LauncherPath {
			get {
				string launcher;
				if (SettingsBag.TryGetValue (LauncherPathKey, out launcher))
					return launcher;
				return null;
			}
			set {
				WillChangeValue (LauncherPathKey);
				SettingsBag.SetValue (LauncherPathKey, value);
				DidChangeValue (LauncherPathKey);
			}
		}

		const string TestSuiteKey = "TestSuite";
		const string DefaultTestSuite = "Xamarin.WebTests.Console.exe";

		[Export (TestSuiteKey)]
		public string TestSuite {
			get {
				string testsuite;
				if (SettingsBag.TryGetValue (TestSuiteKey, out testsuite))
					return testsuite;

				testsuite = DefaultTestSuite;
				SettingsBag.SetValue (TestSuiteKey, testsuite);
				return testsuite;
			}
			set {
				WillChangeValue (TestSuiteKey);
				SettingsBag.SetValue (TestSuiteKey, value);
				DidChangeValue (TestSuiteKey);
			}
		}

		const string ListenAddressKey = "ListenAddress";

		[Export (ListenAddressKey)]
		public string ListenAddress {
			get {
				string address;
				if (SettingsBag.TryGetValue (ListenAddressKey, out address))
					return address;
				var support = DependencyInjector.Get<IPortableEndPointSupport> ();
				var endpoint = support.GetEndpoint (8888);
				address = string.Format ("{0}:{1}", endpoint.Address, endpoint.Port);
				SettingsBag.SetValue (ListenAddressKey, address);
				return address;
			}
			set {
				WillChangeValue (ListenAddressKey);
				SettingsBag.SetValue (ListenAddressKey, value);
				DidChangeValue (ListenAddressKey);
			}
		}

		const string AndroidEndpointKey = "AndroidEndpoint";

		[Export (AndroidEndpointKey)]
		public string AndroidEndpoint {
			get {
				string endpoint;
				if (!SettingsBag.TryGetValue (AndroidEndpointKey, out endpoint))
					endpoint = null;
				return endpoint;
			}
			set {
				WillChangeValue (AndroidEndpointKey);
				SettingsBag.SetValue (AndroidEndpointKey, value);
				DidChangeValue (AndroidEndpointKey);
			}
		}

		const string IOSEndpointKey = "IOSEndpoint";

		[Export ("IOSEndpoint")]
		public string IOSEndpoint {
			get {
				string endpoint;
				if (!SettingsBag.TryGetValue (IOSEndpointKey, out endpoint))
					endpoint = null;
				return endpoint;
			}
			set {
				WillChangeValue (IOSEndpointKey);
				SettingsBag.SetValue (IOSEndpointKey, value);
				DidChangeValue (IOSEndpointKey);
			}
		}

		public bool IsEnabled (TestFeature feature)
		{
			if (Configuration != null)
				return Configuration.IsEnabled (feature);
			return feature.Constant ?? feature.DefaultValue ?? false;
		}

		public void SetIsEnabled (TestFeature feature, bool value)
		{
			if (Configuration != null)
				Configuration.SetIsEnabled (feature, value);
		}

		[Export ("Apply")]
		public void Apply ()
		{
			var session = MacUI.AppDelegate.CurrentSession;
			if (session == null)
				return;
			session.Session.UpdateSettings (CancellationToken.None);
		}
	}
}
