//-----------------------------------------------------------------------
// <copyright file="IsTestWithConfiguredLog4Net.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils
{
    using System;

    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;

    public class IsTestWithConfiguredLog4Net
    {
        private static bool initialized = false;
        private static object initLock = new object();

        public IsTestWithConfiguredLog4Net() {
            lock (initLock) {
                if (!initialized) {
                    InitLog4NetConfig();
                    initialized = true;
                }
            }
        }

        private static void InitLog4NetConfig() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%-4timestamp %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            ConsoleAppender console = new ConsoleAppender();
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }
    }
}