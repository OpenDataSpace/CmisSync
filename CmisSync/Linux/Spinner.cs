//-----------------------------------------------------------------------
// <copyright file="Spinner.cs" company="GRAU DATA AG">
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
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync
{
    using System;
    using System.Timers;

    using Gtk;

    // This is a close implementation of GtkSpinner
    [CLSCompliant(false)]
    public class Spinner : Image {

        public bool Active;

        private Gdk.Pixbuf[] Images;
        private Timer Timer;
        private int CycleDuration;
        private int CurrentStep;
        private int NumSteps;
        private int Size;

        public Spinner(int size) : base()
        {
            this.Size = size;

            this.CycleDuration = 600;
            this.CurrentStep = 0;

            Gdk.Pixbuf spinner_gallery = UIHelpers.GetIcon("process-working", this.Size);

            int frames_in_width  = spinner_gallery.Width  / this.Size;
            int frames_in_height = spinner_gallery.Height / this.Size;

            this.NumSteps = frames_in_width * frames_in_height;
            this.Images   = new Gdk.Pixbuf[this.NumSteps - 1];

            int i = 0;

            for (int y = 0; y < frames_in_height; y++) {
                for (int x = 0; x < frames_in_width; x++) {
                    if (!(y == 0 && x == 0)) {
                        this.Images[i] = new Gdk.Pixbuf(spinner_gallery, x * this.Size, y * this.Size, this.Size, this.Size);
                        i++;
                    }
                }
            }

            this.Timer = new Timer() {
                Interval = (double)this.CycleDuration / this.NumSteps
            };

            this.Timer.Elapsed += delegate {
                this.NextImage();
            };

            this.Start();
        }

        private void NextImage()
        {
            if (this.CurrentStep < this.NumSteps - 2) {
                this.CurrentStep++;
            } else {
                this.CurrentStep = 0;
            }

            Application.Invoke(delegate { this.SetImage(); });
        }

        private void SetImage()
        {
            this.Pixbuf = this.Images[this.CurrentStep];
        }

        public bool IsActive()
        {
            return this.Active;
        }

        public void Start()
        {
            this.CurrentStep = 0;
            this.Active = true;
            this.Timer.Start();
        }

        public void Stop()
        {
            this.Active = false;
            this.Timer.Stop();
        }
    }
}
