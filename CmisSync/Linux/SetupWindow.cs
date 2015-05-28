//-----------------------------------------------------------------------
// <copyright file="SetupWindow.cs" company="GRAU DATA AG">
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
//   CmisSync, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Timers;

    using Gtk;
    using Mono.Unix;

    [CLSCompliant(false)]
    public class SetupWindow : Window {
        // TODO: capscmi
        private HBox hBox;
        private VBox vBox;
        private VBox wrapper;
        private VBox optionArea;
        private HBox buttons;

        public string Header;
        public string Description;
        public string SecondaryTextColor;
        public string SecondaryTextColorSelected;

        public Container Content;

        public SetupWindow() : base(string.Empty) {
            this.Title          = string.Format("{0} {1}", Properties_Resources.ApplicationName, Catalog.GetString("Setup"));
            this.BorderWidth    = 0;
            this.IconName       = "dataspacesync-app";
            this.Resizable      = false;
            this.WindowPosition = WindowPosition.Center;
            this.Deletable      = false;

            this.DeleteEvent += delegate(object sender, DeleteEventArgs args) {
                args.RetVal = true;
            };

            this.SecondaryTextColor = Style.Foreground(StateType.Insensitive).ToHex();

            this.SecondaryTextColorSelected = this.MixColors(
                new TreeView().Style.Foreground(StateType.Selected),
                new TreeView().Style.Background(StateType.Selected),
                0.15).ToHex();

            this.SetSizeRequest(680, 400);

            this.hBox = new HBox(false, 0);

            this.vBox = new VBox(false, 0);

            this.wrapper = new VBox(false, 0) {
                BorderWidth = 0
            };

            this.optionArea = new VBox(false, 0) {
                BorderWidth = 0
            };

            this.buttons = this.CreateButtonBox();

            HBox layout_horizontal = new HBox(false, 0) {
                BorderWidth = 0
            };

            layout_horizontal.PackStart(this.optionArea, true, true, 0);
            layout_horizontal.PackStart(this.buttons, false, false, 0);

            this.vBox.PackStart(this.wrapper, true, true, 0);
            this.vBox.PackStart(layout_horizontal, false, false, 15);

            EventBox box = new EventBox();
            Gdk.Color bg_color = new Gdk.Color();
            Gdk.Color.Parse("#000", ref bg_color);
            box.ModifyBg(StateType.Normal, bg_color);

            Image side_splash = UIHelpers.GetImage("side-splash.png");
            side_splash.Yalign = 1;

            box.Add(side_splash);

            this.hBox.PackStart(box, false, false, 0);
            this.hBox.PackStart(this.vBox, true, true, 30);

            base.Add(this.hBox);
        }

        private HBox CreateButtonBox() {
            return new HBox() {
                BorderWidth = 0,
                //Layout      = ButtonBoxStyle.End,
                Homogeneous = false,
                Spacing     = 6
            };
        }

        [CLSCompliant(false)]
        public void AddButton(Button button) {
            (button.Child as Label).Xpad = 15;
            this.buttons.Add(button);
        }

        [CLSCompliant(false)]
        public void AddOption(Widget widget) {
            this.optionArea.Add(widget);
        }

        [CLSCompliant(false)]
        new public void Add(Widget widget) {
            Label header = new Label("<span size='large'><b>" + this.Header + "</b></span>") {
                UseMarkup = true,
                Xalign = 0,
            };

            VBox layout_vertical = new VBox(false, 0);
            layout_vertical.PackStart(new Label(string.Empty), false, false, 6);
            layout_vertical.PackStart(header, false, false, 0);

            if (!string.IsNullOrEmpty(this.Description)) {
                Label description = new Label(this.Description) {
                    Xalign = 0,
                    LineWrap = true,
                    LineWrapMode = Pango.WrapMode.WordChar
                };

                layout_vertical.PackStart(description, false, false, 21);
            }

            if (widget != null) {
                layout_vertical.PackStart(widget, true, true, 0);
            }

            this.wrapper.PackStart(layout_vertical, true, true, 0);
            this.ShowAll();
        }

        public void Reset() {
            this.Header = string.Empty;
            this.Description = string.Empty;

            if (this.optionArea.Children.Length > 0) {
                this.optionArea.Remove(this.optionArea.Children[0]);
            }

            if (this.wrapper.Children.Length > 0) {
                this.wrapper.Remove(this.wrapper.Children[0]);
            }

            foreach (Button button in this.buttons) {
                this.buttons.Remove(button);
            }

            this.ShowAll();
        }

        new public void ShowAll() {
            if (this.buttons.Children.Length > 0) {
                Button default_button = (Button)this.buttons.Children[this.buttons.Children.Length - 1];

                default_button.CanDefault = true;
                this.Default = default_button;
            }

            base.ShowAll();
            base.Present();
        }

        private Gdk.Color MixColors(Gdk.Color first_color, Gdk.Color second_color, double ratio) {
            return new Gdk.Color(
                Convert.ToByte((255 * (Math.Min(65535, first_color.Red * (1.0 - ratio) +
                    second_color.Red * ratio))) / 65535),
                Convert.ToByte((255 * (Math.Min(65535, first_color.Green * (1.0 - ratio) +
                    second_color.Green * ratio))) / 65535),
                Convert.ToByte((255 * (Math.Min(65535, first_color.Blue * (1.0 - ratio) +
                    second_color.Blue * ratio))) / 65535));
        }
    }
}