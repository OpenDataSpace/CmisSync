//-----------------------------------------------------------------------
// <copyright file="UrlWidget.cs" company="GRAU DATA AG">
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

namespace CmisSync.Widgets {
    using System;
    using System.Text.RegularExpressions;

    [System.ComponentModel.ToolboxItem(true)]
    public partial class UrlWidget : Gtk.Bin {
        private bool isValidUrl = true;

        private static Gdk.Color RED;

        static UrlWidget() {
            Gdk.Color.Parse("red", ref RED);
        }

        /// <summary>
        /// Regex to check an HTTP/HTTPS URL.
        /// </summary>
        private Regex UrlRegex = new Regex(
            @"^" +
            "(https?)://" +                                                 // protocol
            "(([a-z\\d$_\\.\\+!\\*'\\(\\),;\\?&=-]|%[\\da-f]{2})+" +        // username
            "(:([a-z\\d$_\\.\\+!\\*'\\(\\),;\\?&=-]|%[\\da-f]{2})+)?" +     // password
            "@)?(?#" +                                                      // auth delimiter
            ")((([a-z\\d]\\.|[a-z\\d][a-z\\d-]*[a-z\\d]\\.)*" +             // domain segments AND
            "[a-z][a-z\\d-]*[a-z\\d]" +                                     // top level domain OR
            "|((\\d|\\d\\d|1\\d{2}|2[0-4]\\d|25[0-5])\\.){3}" +             // IP address
            "(\\d|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])" +                    //
            ")(:\\d+)?" +                                                   // port
            ")((/+([a-z\\d$_\\.\\+!\\*'\\(\\),;:@&=-]|%[\\da-f]{2})*)*?)" + // path
            "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public event EventHandler Changed;

        public UrlWidget(bool urlValidation = true) {
            this.Build();
            this.IsUrlEditable = true;
            this.ValidationActivated = urlValidation;
        }

        public string Url {
            get {
                return this.urlEntry.Text.Trim();
            }

            set {
                this.urlEntry.Text = value;
                this.UrlChanged(this, null);
            }
        }

        public bool IsValidUrl {
            get {
                return this.isValidUrl;
            }
        }

        public bool IsUrlEditable {
            get {
                return this.urlEntry.IsEditable;
            }

            set {
                this.urlEntry.IsEditable = value;
                this.urlEntry.CanFocus = value;
                this.urlEntry.Sensitive = value;
                this.urlEntry.CanDefault = value;
            }
        }

        public bool ValidationActivated { get; set; }

        private void ValidateUrl(object sender, EventArgs args) {
            if (this.ValidationActivated) {
                if (string.IsNullOrEmpty(this.Url)) {
                    this.isValidUrl = false;
                    this.urlEntry.ModifyText(Gtk.StateType.Normal, RED);
                    this.urlEntry.TooltipText = CmisSync.Properties_Resources.EmptyURLNotAllowed;
                } else if (!this.UrlRegex.IsMatch(this.urlEntry.Text)) {
                    this.isValidUrl = false;
                    this.urlEntry.ModifyText(Gtk.StateType.Normal, RED);
                    this.urlEntry.TooltipText = CmisSync.Properties_Resources.InvalidURL;
                } else {
                    this.isValidUrl = true;
                    this.urlEntry.ModifyText(Gtk.StateType.Normal);
                }
            } else {
                this.urlEntry.ModifyText(Gtk.StateType.Normal);
            }
        }

        private void UrlChanged(object sender, EventArgs args) {
            this.ValidateUrl(sender, args);
            var handler = this.Changed;
            if (handler != null) {
                handler(this, args);
            }
        }
    }
}