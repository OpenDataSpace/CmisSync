//-----------------------------------------------------------------------
// <copyright file="DotCMISLogListener.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis
{
    using System;
    using log4net.Core;

    /// <summary>
    /// Dot CMIS log listener enables the logging inside of DotCMIS and passes the log messages to log4net.
    /// </summary>
    public class DotCMISLogListener : System.Diagnostics.TraceListener
    {
        private readonly log4net.ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.DotCMISLogListener"/> class.
        /// </summary>
        public DotCMISLogListener() : this(log4net.LogManager.GetLogger(typeof(DotCMISLogListener)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.DotCMISLogListener"/> class.
        /// </summary>
        /// <param name="log">log4net logger, which should receive the log messages of DotCMIS.</param>
        public DotCMISLogListener(log4net.ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("Given logger is null");
            }

            this.log = log;
            this.SetLog4NetLevelToTraceLevel();
            this.log.Logger.Repository.ConfigurationChanged += delegate(object sender, EventArgs e) {
                this.SetLog4NetLevelToTraceLevel();
            };
        }

        /// <summary>
        /// Write the specified message to log4net logger.
        /// </summary>
        /// <param name="message">Message to be written.</param>
        public override void Write(string message)
        {
            this.log.Debug(message);
        }

        /// <summary>
        /// Writes the line to log4net logger.
        /// </summary>
        /// <param name="message">Message to be written.</param>
        public override void WriteLine(string message)
        {
            this.log.Debug(message);
        }

        private void SetLog4NetLevelToTraceLevel()
        {
            if (this.log.IsDebugEnabled)
            {
                DotCMIS.Util.DotCMISDebug.DotCMISTraceLevel = System.Diagnostics.TraceLevel.Verbose;
            }
            else
            {
                DotCMIS.Util.DotCMISDebug.DotCMISTraceLevel = System.Diagnostics.TraceLevel.Off;
            }
        }
    }
}
