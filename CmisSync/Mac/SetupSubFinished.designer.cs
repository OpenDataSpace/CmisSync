//-----------------------------------------------------------------------
// <copyright file="SetupSubFinished.designer.cs" company="GRAU DATA AG">
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
	[Register ("SetupSubFinishedController")]
	partial class SetupSubFinishedController
	{
		[Outlet]
		MonoMac.AppKit.NSButton FinishButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField FinishText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton OpenButton { get; set; }

		[Action ("OnFinish:")]
		partial void OnFinish (MonoMac.Foundation.NSObject sender);

		[Action ("OnOpen:")]
		partial void OnOpen (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (FinishText != null) {
				FinishText.Dispose ();
				FinishText = null;
			}

			if (FinishButton != null) {
				FinishButton.Dispose ();
				FinishButton = null;
			}

			if (OpenButton != null) {
				OpenButton.Dispose ();
				OpenButton = null;
			}
		}
	}

	[Register ("SetupSubFinished")]
	partial class SetupSubFinished
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
