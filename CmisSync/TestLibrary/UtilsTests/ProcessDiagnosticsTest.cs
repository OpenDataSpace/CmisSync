//-----------------------------------------------------------------------
// <copyright file="ProcessDiagnosticsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class ProcessDiagnosticsTest
    {
        [Test, Category("Fast"), Ignore("Not used")]
        public void HandleCount()
        {
            Process p = System.Diagnostics.Process.GetCurrentProcess();
            int i = p.HandleCount;
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
            using (file.CreateText()) {
                p.Refresh();
                int j = p.HandleCount;
                Assert.That(j, Is.GreaterThan(i));
            }

            file.Delete();
        }

        [Test, Category("Fast"), Ignore("Not used")]
        public void MemoryCount()
        {
            Process p = System.Diagnostics.Process.GetCurrentProcess();
            Console.WriteLine(CmisSync.Lib.Utils.FormatSize(p.WorkingSet64));
        }
    }
}