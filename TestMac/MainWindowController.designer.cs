// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace TestMac
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSButton Clear { get; set; }

		[Outlet]
		AppKit.NSButton Repeat { get; set; }

		[Outlet]
		AppKit.NSButton Run { get; set; }

		[Outlet]
		AppKit.NSTextField ServerStatusMessage { get; set; }

		[Outlet]
		AppKit.NSButton Stop { get; set; }

		[Outlet]
		AppKit.NSOutlineView TestResultView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TestResultView != null) {
				TestResultView.Dispose ();
				TestResultView = null;
			}

			if (Clear != null) {
				Clear.Dispose ();
				Clear = null;
			}

			if (Repeat != null) {
				Repeat.Dispose ();
				Repeat = null;
			}

			if (Run != null) {
				Run.Dispose ();
				Run = null;
			}

			if (ServerStatusMessage != null) {
				ServerStatusMessage.Dispose ();
				ServerStatusMessage = null;
			}

			if (Stop != null) {
				Stop.Dispose ();
				Stop = null;
			}
		}
	}
}
