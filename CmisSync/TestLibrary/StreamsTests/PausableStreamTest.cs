//-----------------------------------------------------------------------
// <copyright file="PausableStreamTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CmisSync.Lib.Streams;

    using NUnit.Framework;

    [TestFixture]
    public class PausableStreamTest {
        [Test, Category("Medium"), Category("Streams"), Timeout(6000)]
        public void PauseAndResumeStream([Values(1, 2, 5)]int seconds) {
            int length = 1024 * 1024 * 10;
            var start = DateTime.Now;
            byte[] content = new byte[length];
            using (var inputStream = new MemoryStream(content))
            using (var underTest = new PausableStream(inputStream)) {
                underTest.Pause();
                var task = Task.Factory.StartNew(() => {
                    using (var outputStream = new MemoryStream()) {
                        underTest.CopyTo(outputStream);
                        Assert.That(outputStream.Length, Is.EqualTo(length));
                        var duration = DateTime.Now - start;
                        Assert.That(Math.Round(duration.TotalSeconds), Is.InRange(seconds, seconds + 1));
                    }
                });
                System.Threading.Thread.Sleep(seconds * 1000);
                underTest.Resume();
                task.Wait();
            }
        }

        [Test, Category("Fast"), Category("Streams"), Timeout(6000)]
        public void PausableStreamDoesNotPauseWithoutCallingPause() {
            int length = 1024 * 1024 * 10;
            byte[] content = new byte[length];
            using (var inputStream = new MemoryStream(content))
                using (var underTest = new PausableStream(inputStream)) {
                var task = Task.Factory.StartNew(() => {
                    using (var outputStream = new MemoryStream()) {
                        underTest.CopyTo(outputStream);
                        Assert.That(outputStream.Length, Is.EqualTo(length));
                    }
                });
                task.Wait();
            }
        }

        [Test, Category("Fast"), Category("Streams"), Timeout(2000)]
        public void PausableStreamDoesPauseAndResumeOnMultiplePauseCalls([Values(1)]int seconds) {
            int length = 1024 * 1024 * 10;
            var start = DateTime.Now;
            byte[] content = new byte[length];
            using (var inputStream = new MemoryStream(content))
                using (var underTest = new PausableStream(inputStream)) {
                underTest.Pause();
                underTest.Pause();
                underTest.Pause();
                underTest.Pause();
                var task = Task.Factory.StartNew(() => {
                    using (var outputStream = new MemoryStream()) {
                        underTest.CopyTo(outputStream);
                        Assert.That(outputStream.Length, Is.EqualTo(length));
                        var duration = DateTime.Now - start;
                        Assert.That(Math.Round(duration.TotalSeconds), Is.InRange(seconds, seconds + 1));
                    }
                });
                System.Threading.Thread.Sleep(seconds * 1000);
                underTest.Resume();
                underTest.Resume();
                task.Wait();
            }
        }
    }
}