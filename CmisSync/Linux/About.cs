//-----------------------------------------------------------------------
// <copyright file="About.cs" company="GRAU DATA AG">
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
//   Copyright (C) 2010  Hylke Bons (hylkebons@gmail.com)
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
//   along with this program. If not, see (http://www.gnu.org/licenses/).

namespace CmisSync {
    using System;
    using System.Globalization;
    using Gtk;
    using Mono.Unix;

    [CLSCompliant(false)]
    public class About : Window {
        public AboutController Controller = new AboutController();

        public About() : base(string.Empty) {
            this.DeleteEvent += delegate(object o, DeleteEventArgs args) {
                this.Controller.WindowClosed();
                args.RetVal = true;
            };

            this.DefaultSize    = new Gdk.Size(600, 260);
            this.Resizable      = false;
            this.BorderWidth    = 0;
            this.IconName       = "dataspacesyc-folder";
            this.WindowPosition = WindowPosition.Center;
            this.Title          = string.Format(Properties_Resources.About, Properties_Resources.ApplicationName);
            this.AppPaintable   = true;

            string image_path = UIHelpers.GetImagePath("about.png");

            this.Realize();
            Gdk.Pixbuf buf = new Gdk.Pixbuf(image_path);
            Gdk.Pixmap map, map2;
            buf.RenderPixmapAndMask(out map, out map2, 255);
            GdkWindow.SetBackPixmap(map, false);

            this.CreateAbout();

            this.Controller.HideWindowEvent += delegate {
                Application.Invoke(delegate {
                    this.HideAll();
                });
            };

            this.Controller.ShowWindowEvent += delegate {
                Application.Invoke(delegate {
                    this.ShowAll();
                    this.Present();
                });
            };
        }

        private void CreateAbout() {
            Gdk.Color fgcolor = new Gdk.Color();
            Gdk.Color.Parse("red", ref fgcolor);
            Label version = new Label() {
                Markup = string.Format(
                    "<span font_size='small' fgcolor='#729fcf'>{0}</span>",
                    string.Format(
                    Properties_Resources.Version,
                    this.Controller.RunningVersion)),
                Xalign = 0
            };

            Label credits = new Label() {
                LineWrap = true,
                LineWrapMode = Pango.WrapMode.Word,
                Markup = "<span font_size='small' fgcolor='#729fcf'>" +
                "Copyright © 2013–" + DateTime.Now.Year.ToString() + " GRAU DATA AG, Aegif and others.\n" +
                "\n" + Properties_Resources.ApplicationName +
                " is Open Source software. You are free to use, modify, " +
                "and redistribute it under the GNU General Public License version 3 or later." +
                "</span>",
                WidthRequest = 330,
                Wrap = true,
                Xalign = 0
            };

            LinkButton website_link = new LinkButton(this.Controller.WebsiteLinkAddress, Properties_Resources.Website);
            website_link.ModifyFg(StateType.Active, fgcolor);
            LinkButton credits_link = new LinkButton(this.Controller.CreditsLinkAddress, Properties_Resources.Credits);

            HBox layout_links = new HBox(false, 0);
            layout_links.PackStart(website_link, false, false, 0);
            layout_links.PackStart(credits_link, false, false, 0);

            VBox layout_vertical = new VBox(false, 0);
            layout_vertical.PackStart(new Label(string.Empty), false, false, 42);
            layout_vertical.PackStart(version, false, false, 0);
            layout_vertical.PackStart(credits, false, false, 9);
            layout_vertical.PackStart(new Label(string.Empty), false, false, 0);
            layout_vertical.PackStart(layout_links, false, false, 0);

            HBox layout_horizontal = new HBox(false, 0) {
                BorderWidth   = 0,
                HeightRequest = 260,
                WidthRequest  = 640
            };
            layout_horizontal.PackStart(new Label(string.Empty), false, false, 150);
            layout_horizontal.PackStart(layout_vertical, false, false, 0);

            this.Add(layout_horizontal);
        }
    }
}