﻿//
// AppDelegate.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Foundation;
using AppKit;
using Xamarin.AsyncTests;
using Xamarin.AsyncTests.Portable;
using Xamarin.AsyncTests.Framework;

namespace Xamarin.AsyncTests.MacUI
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;
		SettingsDialogController settingsDialogController;
		TestSessionModel currentSession;
		bool isStopped;
		bool hasServer;
		MacUI ui;

		public MacUI MacUI {
			get { return ui; }
		}

		[Export ("MainController")]
		public MainWindowController MainController {
			get { return mainWindowController; }
		}

		public static AppDelegate Instance {
			get { return (AppDelegate)NSApplication.SharedApplication.Delegate; }
		}

		public static SettingsDialogController Settings {
			get { return Instance.settingsDialogController; }
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			ui = new MacUI ();

			isStopped = true;
			ui.TestRunner.Stop.NotifyStateChanged.StateChanged += (sender, e) => IsStopped = !e;
			ui.ServerManager.Start.NotifyStateChanged.StateChanged += (sender, e) => HasServer = !e;
			IsStopped = true;

			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);

			settingsDialogController = new SettingsDialogController ();

			ui.ServerManager.TestSession.PropertyChanged += (sender, e) => {
				if (e == null)
					CurrentSession = null;
				else
					CurrentSession = new TestSessionModel (e);
			};

			StartServer ();
		}

		ServerParameters GetParameters ()
		{
			var endpointSupport = DependencyInjector.Get<IPortableEndPointSupport> ();

			if (string.IsNullOrEmpty (ui.Settings.MonoRuntime))
				throw new AlertException ("Must set Mono Runtime in Settings Dialog.");
			if (string.IsNullOrEmpty (ui.Settings.LauncherPath))
				throw new AlertException ("Must set Launcher Path in Settings Dialog.");
			if (string.IsNullOrEmpty (ui.Settings.TestSuite))
				throw new AlertException ("Must set Test Suite in Settings Dialog.");

			var pipeArgs = new PipeArguments ();

			pipeArgs.MonoPrefix = ui.Settings.MonoRuntime;
			var monoPath = Path.Combine (pipeArgs.MonoPrefix, "bin", "mono");
			if (!File.Exists (monoPath))
				throw new AlertException ("Invalid runtime prefix: {0}", pipeArgs.MonoPrefix);

			pipeArgs.ConsolePath = FindFile (ui.Settings.LauncherPath);
			pipeArgs.Assembly = FindFile (ui.Settings.TestSuite);
			pipeArgs.ExtraArguments = ui.Settings.Arguments;

			var endpoint = endpointSupport.GetLoopbackEndpoint (8888);
			return ServerParameters.CreatePipe (endpoint, pipeArgs);
		}

		static string FindFile (string filename)
		{
			if (Path.IsPathRooted (filename))
				return filename;

			var appDir = Path.GetDirectoryName (Path.GetDirectoryName (Environment.CurrentDirectory));
			appDir = Path.GetDirectoryName (appDir);
			var projectDir = Path.GetDirectoryName (Path.GetDirectoryName (appDir));
			var solutionDir = Path.GetDirectoryName (projectDir);
			var outDir = Path.Combine (solutionDir, "out");

			var outFile = Path.Combine (outDir, filename);

			if (!File.Exists (outFile))
				throw new AlertException ("Cannot find file: {0}", filename);

			return outFile;
		}

		static IPortableEndPoint ParseEndPoint (string address)
		{
			var endpointSupport = DependencyInjector.Get<IPortableEndPointSupport> ();
			try {
				return endpointSupport.ParseEndpoint (address);
			} catch {
				throw new AlertException ("Failed to parse endpoint: '{0}'", address);
			}
		}

		static void ShowAlertForException (string message, Exception ex)
		{
			var alert = NSAlert.WithMessage (message, "Ok", string.Empty, string.Empty, ex.Message);
			alert.RunModal ();
		}

		[Export ("ShowPreferences:")]
		public void ShowPreferences ()
		{
			settingsDialogController.Window.MakeKeyAndOrderFront (this);
		}

		[Export ("ListenForConnection:")]
		public async void ListenForConnection ()
		{
			var endpointSupport = DependencyInjector.Get<IPortableEndPointSupport> ();
			var parameters = ServerParameters.WaitForConnection (endpointSupport.GetLoopbackEndpoint (8888));

			try {
				await ui.ServerManager.Start.Execute (parameters);
			} catch (Exception ex) {
				ShowAlertForException ("Failed to start server", ex);
				return;
			}
		}

		[Export ("StartServer:")]
		public async void StartServer ()
		{
			ServerParameters parameters;
			try {
				parameters = GetParameters ();
			} catch (AlertException ex) {
				var alert = NSAlert.WithMessage ("Failed to start server", "Ok", string.Empty, string.Empty, ex.Message);
				alert.RunModal ();
				return;
			}

			try {
				await ui.ServerManager.Start.Execute (parameters);
			} catch (Exception ex) {
				ShowAlertForException ("Failed to start server", ex);
				return;
			}
		}

		[Export ("StopServer:")]
		public async void StopServer ()
		{
			try {
				await ui.ServerManager.Stop.Execute ();
			} catch (Exception ex) {
				var alert = NSAlert.WithMessage ("Failed to stop server", "Ok", string.Empty, string.Empty, ex.Message);
				alert.RunModal ();
				return;
			}
		}

		[Export ("ClearSession:")]
		public void ClearSession ()
		{
			if (CurrentSession != null)
				CurrentSession.RemoveAllChildren ();
		}

		[Export ("LoadSession:")]
		public void LoadSession ()
		{
			var open = new NSOpenPanel {
				CanChooseDirectories = false, CanChooseFiles = true, AllowsMultipleSelection = false,
				AllowedFileTypes = new string[] { "xml" }
			};
			var ret = open.RunModal ();
			if (ret == 0 || open.Urls.Length != 1)
				return;

			try {
				MainController.LoadSession (open.Urls [0].Path);
			} catch (Exception ex) {
				ShowAlertForException ("Failed to load session", ex);
				return;
			}
		}

		[Export ("SaveSession:")]
		public void SaveSession ()
		{
			var save = new NSSavePanel {
				CanCreateDirectories = false, CanSelectHiddenExtension = false,
				AllowedFileTypes = new string[] { "xml" }
			};
			var ret = save.RunModal ();
			if (ret == 0 || save.Url == null)
				return;

			try {
				MainController.SaveSession (save.Url.Path);
			} catch (Exception ex) {
				ShowAlertForException ("Failed to save session", ex);
				return;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[AppDelegate: {0:x}]", Handle.ToInt64 ());
		}

		public const string CurrentSessionKey = "CurrentSession";

		[Export (CurrentSessionKey)]
		public TestSessionModel CurrentSession {
			get { return currentSession; }
			set {
				WillChangeValue (CurrentSessionKey);
				currentSession = value;
				DidChangeValue (CurrentSessionKey);
			}
		}

		public const string IsStoppedKey = "IsStopped";

		[Export (IsStoppedKey)]
		public bool IsStopped {
			get { return isStopped; }
			set {
				WillChangeValue (IsStoppedKey);
				isStopped = value;
				DidChangeValue (IsStoppedKey);
			}
		}

		public const string HasServerKey = "HasServer";

		[Export (HasServerKey)]
		public bool HasServer {
			get { return hasServer; }
			set {
				WillChangeValue (HasServerKey);
				hasServer = value;
				DidChangeValue (HasServerKey);
			}
		}
	}
}

