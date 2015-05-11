//-----------------------------------------------------------------------
// <copyright file="MockedContentStream.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer {
    using System;
    using System.IO;
    using System.Text;

    using DotCMIS.Data;

    using Moq;

    public class MockedContentStream : Mock<IContentStream> {
        public MockedContentStream(string fileName = null, string content = null, string mimeType = null, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.FileName = fileName;
            this.MimeType = mimeType ?? CmisSync.Lib.Cmis.MimeType.GetMIMEType(this.FileName);
            this.Setup(m => m.FileName).Returns(() => this.FileName);
            this.Setup(m => m.MimeType).Returns(() => this.MimeType);
            this.Setup(m => m.Length).Returns(() => this.Content != null ? (long?)this.Content.Length : null);
            this.Setup(m => m.Stream).Returns(() => this.Content != null ? new MemoryStream(this.Content, false) : null);
            this.SetContent(content);
        }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public byte[] Content { get; set; }

        public void SetContent(string content) {
            this.Content = content == null ? null : Encoding.UTF8.GetBytes(content);
        }
    }
}