//-----------------------------------------------------------------------
// <copyright file="SetupSubLogin.designer.cs" company="GRAU DATA AG">
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
	[Register ("SetupSubLoginController")]
	partial class SetupSubLoginController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField AddressHelp { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField AddressLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField AddressText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton CancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton ContinueButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSProgressIndicator LoginProgress { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField PasswordText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UserText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField WarnText { get; set; }

		[Action ("OnCancel:")]
		partial void OnCancel (MonoMac.Foundation.NSObject sender);

		[Action ("OnContinue:")]
		partial void OnContinue (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AddressHelp != null) {
				AddressHelp.Dispose ();
				AddressHelp = null;
			}

			if (AddressLabel != null) {
				AddressLabel.Dispose ();
				AddressLabel = null;
			}

			if (AddressText != null) {
				AddressText.Dispose ();
				AddressText = null;
			}

			if (CancelButton != null) {
				CancelButton.Dispose ();
				CancelButton = null;
			}

			if (ContinueButton != null) {
				ContinueButton.Dispose ();
				ContinueButton = null;
			}

			if (PasswordLabel != null) {
				PasswordLabel.Dispose ();
				PasswordLabel = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}

			if (UserLabel != null) {
				UserLabel.Dispose ();
				UserLabel = null;
			}

			if (UserText != null) {
				UserText.Dispose ();
				UserText = null;
			}

			if (WarnText != null) {
				WarnText.Dispose ();
				WarnText = null;
			}

			if (LoginProgress != null) {
				LoginProgress.Dispose ();
				LoginProgress = null;
			}
		}
	}

	[Register ("SetupSubLogin")]
	partial class SetupSubLogin
	{
		[Outlet]
		MonoMac.AppKit.NSProgressIndicator LoginProgress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoginProgress != null) {
				LoginProgress.Dispose ();
				LoginProgress = null;
			}
		}
	}
}
