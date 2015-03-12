﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Foundation;
using AppKit;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Framework;

namespace Xamarin.AsyncTests.MacUI
{
	public partial class SettingsDialogController : NSWindowController
	{
		NSMutableArray testCategoriesArray;
		NSMutableArray testFeaturesArray;
		TestCategoryModel currentCategory;

		public static MacUI MacUI {
			get { return AppDelegate.Instance.MacUI; }
		}

		public static SettingsBag SettingsBag {
			get { return AppDelegate.Instance.MacUI.Settings; }
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

			var ui = AppDelegate.Instance.MacUI;
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

		[Export ("Arguments")]
		public string Arguments {
			get { return SettingsBag.Arguments; }
			set {
				WillChangeValue ("Arguments");
				SettingsBag.Arguments = value;
				DidChangeValue ("Arguments");
			}
		}

		[Export ("MonoRuntime")]
		public string MonoRuntime {
			get { return SettingsBag.MonoRuntime; }
			set {
				WillChangeValue ("MonoRuntime");
				SettingsBag.MonoRuntime = value;
				DidChangeValue ("MonoRuntime");
			}
		}

		[Export ("LauncherPath")]
		public string LauncherPath {
			get { return SettingsBag.LauncherPath; }
			set {
				WillChangeValue ("LauncherPath");
				SettingsBag.LauncherPath = value;
				DidChangeValue ("LauncherPath");
			}
		}

		[Export ("TestSuite")]
		public string TestSuite {
			get { return SettingsBag.TestSuite; }
			set {
				WillChangeValue ("TestSuite");
				SettingsBag.TestSuite = value;
				DidChangeValue ("TestSuite");
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

		[Export ("Apply:")]
		public void Apply ()
		{
			Console.WriteLine ("APPLY!");
			var session = AppDelegate.Instance.CurrentSession;
			if (session == null)
				return;
			session.Session.UpdateSettings (CancellationToken.None);
		}
	}
}
