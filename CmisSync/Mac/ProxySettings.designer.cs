//-----------------------------------------------------------------------
// <copyright file="ProxySettings.designer.cs" company="GRAU DATA AG">
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
	[Register ("ProxySettingsController")]
	partial class ProxySettingsController
	{
		[Outlet]
		MonoMac.AppKit.NSButton CredentialsCheckbox { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell CustomProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell DefaultProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButtonCell NoProxyButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField PasswordLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField PasswordTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ServerLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField ServerTextField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UsernameLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField UsernameTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ServerLabel != null) {
				ServerLabel.Dispose ();
				ServerLabel = null;
			}

			if (CredentialsCheckbox != null) {
				CredentialsCheckbox.Dispose ();
				CredentialsCheckbox = null;
			}

			if (UsernameLabel != null) {
				UsernameLabel.Dispose ();
				UsernameLabel = null;
			}

			if (PasswordLabel != null) {
				PasswordLabel.Dispose ();
				PasswordLabel = null;
			}

			if (UsernameTextField != null) {
				UsernameTextField.Dispose ();
				UsernameTextField = null;
			}

			if (PasswordTextField != null) {
				PasswordTextField.Dispose ();
				PasswordTextField = null;
			}

			if (ServerTextField != null) {
				ServerTextField.Dispose ();
				ServerTextField = null;
			}

			if (NoProxyButton != null) {
				NoProxyButton.Dispose ();
				NoProxyButton = null;
			}

			if (DefaultProxyButton != null) {
				DefaultProxyButton.Dispose ();
				DefaultProxyButton = null;
			}

			if (CustomProxyButton != null) {
				CustomProxyButton.Dispose ();
				CustomProxyButton = null;
			}
		}
	}

	[Register ("ProxySettings")]
	partial class ProxySettings
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
