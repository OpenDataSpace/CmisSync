using System;

using CmisSync.Lib;

namespace CmisSync {

    /// <summary>
    /// Controller for the Setting dialog.
    /// </summary>
    public class SettingController {

        //===== Actions =====
        /// <summary>
        /// Show Setting Windows Action
        /// </summary>
        public event Action ShowWindowEvent = delegate { };
        /// <summary>
        /// Hide Setting Windows Action
        /// </summary>
        public event Action HideWindowEvent = delegate { };

        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingController()
        {
            Program.Controller.ShowSettingWindowEvent += delegate
            {
                ShowWindowEvent();
            };
        }

        public void ShowWindow()
        {
            ShowWindowEvent();
        }

        public void HideWindow()
        {
            HideWindowEvent();
        }
    }

}
