//-----------------------------------------------------------------------
// <copyright file="SetupSubTutorialBegin.designer.cs" company="GRAU DATA AG">
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
	[Register ("SetupSubTutorialBeginController")]
	partial class SetupSubTutorialBeginController
	{
		[Outlet]
		MonoMac.AppKit.NSButton ContinueButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton SkipButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TutorialText { get; set; }

		[Outlet]
		MonoMac.AppKit.NSImageView TutorialView { get; set; }

		[Action ("OnContinue:")]
		partial void OnContinue (MonoMac.Foundation.NSObject sender);

		[Action ("OnSkip:")]
		partial void OnSkip (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (TutorialText != null) {
				TutorialText.Dispose ();
				TutorialText = null;
			}

			if (TutorialView != null) {
				TutorialView.Dispose ();
				TutorialView = null;
			}

			if (SkipButton != null) {
				SkipButton.Dispose ();
				SkipButton = null;
			}

			if (ContinueButton != null) {
				ContinueButton.Dispose ();
				ContinueButton = null;
			}
		}
	}

	[Register ("SetupSubTutorialBegin")]
	partial class SetupSubTutorialBegin
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
