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
	[Register ("SettingsDialogController")]
	partial class SettingsDialogController
	{
		[Outlet]
		AppKit.NSArrayController CategoriesController { get; set; }

		[Outlet]
		AppKit.NSArrayController FeaturesController { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CategoriesController != null) {
				CategoriesController.Dispose ();
				CategoriesController = null;
			}

			if (FeaturesController != null) {
				FeaturesController.Dispose ();
				FeaturesController = null;
			}
		}
	}
}
