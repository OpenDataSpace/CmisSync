//-----------------------------------------------------------------------
// <copyright file="CmisRepoTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.IO;
    
    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync;
    
    using DotCMIS.Client;
    
    using Moq;
    
    using NUnit.Framework;
    
    [TestFixture]
    public class CmisRepoTest
    {
        private class CmisRepoWrapper : CmisRepo {
            public CmisRepoWrapper(RepoInfo repoInfo, IActivityListener activityListener, bool inMemory = false, ISessionFactory sessionFactory = null, IFileSystemInfoFactory fileSystemInfoFactory = null) :
                base(repoInfo, activityListener, inMemory, sessionFactory, fileSystemInfoFactory)
            {
            }
        }
        
        [Test]
        public void CmisRepoCanBeConstructed (){
            RepoInfo repoInfo = new RepoInfo
            {
                DisplayName = "name",
                Address = new Uri("http://example.com"),
                LocalPath = Path.GetTempPath(),
                RemotePath = "/"
            };
            var activityListener = new Mock<IActivityListener>().Object;
            var sessionFact = new Mock<ISessionFactory>();
            new CmisRepoWrapper(repoInfo, activityListener, true, sessionFact.Object);
        }
    }
}