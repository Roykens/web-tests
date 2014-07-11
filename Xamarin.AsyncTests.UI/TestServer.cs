﻿//
// TestServer.cs
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Xamarin.AsyncTests.UI
{
	using Framework;
	using Server;

	public class TestServer : ServerConnection
	{
		public TestApp App {
			get;
			private set;
		}

		public override TestContext Context {
			get { return App.Context; }
		}

		public IServerConnection Connection {
			get;
			private set;
		}

		public TestServer (TestApp app, Stream stream, IServerConnection connection)
			: base (stream)
		{
			App = app;
			Connection = connection;
		}

		public async Task Run (CancellationToken cancellationToken)
		{
			Context.Configuration.PropertyChanged += OnConfigChanged;

			var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			cts.Token.Register (() => {
				stopRequested = true;
				Stop ();
			});

			try {
				await MainLoop ();
			} catch (Exception ex) {
				if (!stopRequested)
					App.Context.Debug (0, "SERVER ERROR: {0}", ex);
			} finally {
				Context.Configuration.PropertyChanged -= OnConfigChanged;
				Context.Configuration.Clear ();
			}
		}

		public override void Stop ()
		{
			stopRequested = true;
			Context.Configuration.PropertyChanged -= OnConfigChanged;
			try {
				base.Stop ();
			} catch {
				;
			}
			try {
				Connection.Close ();
			} catch {
				;
			}
		}

		void Debug (string message, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine (message, args);
		}

		#region implemented abstract members of ServerConnection

		protected override Task OnLoadResult (TestResult result)
		{
			return Task.Run (() => {
				App.RootTestResult.Result.AddChild (result);
			});
		}

		protected override void OnMessage (string message)
		{
			Debug ("MESSAGE: {0}", message);
		}

		protected override void OnDebug (int level, string message)
		{
			Debug ("DEBUG ({0}): {1}", level, message);
		}

		protected override Task OnHello (CancellationToken cancellationToken)
		{
			return Task.Run (() => {
				Debug ("HELLO WORLD!");
			});
		}

		bool stopRequested;
		bool suppressConfigChanged;
		bool configChanging;

		async void OnConfigChanged (object sender, PropertyChangedEventArgs args)
		{
			lock (this) {
				Debug ("ON CONFIG CHANGED: {0} {1}", suppressConfigChanged, configChanging);
				if (suppressConfigChanged || configChanging || stopRequested)
					return;
				configChanging = true;
			}
			try {
				await SyncConfiguration (Context.Configuration, false);
			} catch {
				;
			}
			configChanging = false;
			Debug ("ON CONFIG CHANGED DONE");
		}

		protected override void OnSyncConfiguration (TestConfiguration configuration, bool fullUpdate)
		{
			try {
				suppressConfigChanged = true;
				Context.Configuration.Merge (configuration, fullUpdate);
			} finally {
				suppressConfigChanged = false;
			}
		}

		#endregion
	}
}

