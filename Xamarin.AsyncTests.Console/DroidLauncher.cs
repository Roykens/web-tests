﻿//
// DroidLauncher.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Console
{
	using Remoting;
	using Portable;

	class DroidLauncher : ApplicationLauncher
	{
		public string SdkRoot {
			get;
			private set;
		}

		public string Adb {
			get;
			private set;
		}

		public string AndroidTool {
			get;
			private set;
		}

		public string Application {
			get;
			private set;
		}

		public string RedirectStdout {
			get;
			private set;
		}

		public string RedirectStderr {
			get;
			private set;
		}

		DroidHelper helper;
		Process process;
		TaskCompletionSource<bool> tcs;

		public DroidLauncher (string app, string stdout, string stderr)
		{
			Application = app;
			RedirectStdout = stdout;
			RedirectStderr = stderr;

			SdkRoot = Environment.GetEnvironmentVariable ("ANDROID_SDK_PATH");
			if (String.IsNullOrEmpty (SdkRoot)) {
				var home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				SdkRoot = Path.Combine (home, "Library", "Developer", "Xamarin", "android-sdk-macosx");
			}

			Adb = Path.Combine (SdkRoot, "platform-tools", "adb");
			AndroidTool = Path.Combine (SdkRoot, "tools", "android");
		}

		Process Launch (string launchArgs)
		{
			var args = new StringBuilder ();
			args.Append ("shell am start ");
			args.Append ("-W -S ");
			args.AppendFormat (" -e XAMARIN_ASYNCTESTS_OPTIONS \\'{0}\\' ", launchArgs);
			args.Append (Application);

			Program.Debug ("Launching apk: {0} {1}", Adb, args);

			var psi = new ProcessStartInfo (Adb, args.ToString ());
			psi.UseShellExecute = false;
			// psi.RedirectStandardInput = true;

			var process = Process.Start (psi);

			Program.Debug ("Started: {0}", process);

			return process;
		}

		public Task<bool> CheckAvd (CancellationToken cancellationToken)
		{
			return CheckAvd ("XamarinWebTests", "android-23", "armeabi-v7a", "Galaxy Nexus", cancellationToken);
		}

		public async Task<bool> CheckAvd (string name, string target, string abi, string device, CancellationToken cancellationToken)
		{
			Program.Debug ("Check Avd: {0}", Adb);
			var avds = await helper.GetAvds (cancellationToken);
			Program.Debug ("Check Avd #1: {0}", string.Join (" ", avds));

			if (avds.Contains (name))
				return true;

			await helper.CreateAvd (name, target, abi, device, true, cancellationToken);
			return true;
		}

		public override void LaunchApplication (string args)
		{
			process = Launch (args);
		}

		public override Task<bool> WaitForExit ()
		{
			var oldTcs = Interlocked.CompareExchange (ref tcs, new TaskCompletionSource<bool> (), null);
			if (oldTcs != null)
				return oldTcs.Task;

			ThreadPool.QueueUserWorkItem (_ => {
				try {
					process.WaitForExit ();
					tcs.TrySetResult (process.ExitCode == 0);
				} catch (Exception ex) {
					tcs.TrySetException (ex);
				}
			});

			return tcs.Task;
		}

		public override void StopApplication ()
		{
			try {
				if (!process.HasExited)
					process.Kill ();
			} catch {
				;
			}
		}
	}
}
