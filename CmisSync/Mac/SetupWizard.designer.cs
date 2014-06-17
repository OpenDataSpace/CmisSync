//-----------------------------------------------------------------------
// <copyright file="SetupWizard.designer.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace CmisSync
{
	[Register ("SetupWizardController")]
	partial class SetupWizardController
	{
		[Outlet]
		MonoMac.AppKit.NSBox Content { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField Description { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField Header { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView SideSplashView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Content != null) {
				Content.Dispose ();
				Content = null;
			}

			if (Description != null) {
				Description.Dispose ();
				Description = null;
			}

			if (Header != null) {
				Header.Dispose ();
				Header = null;
			}

			if (SideSplashView != null) {
				SideSplashView.Dispose ();
				SideSplashView = null;
			}
		}
	}

	[Register ("SetupWizard")]
	partial class SetupWizard
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
