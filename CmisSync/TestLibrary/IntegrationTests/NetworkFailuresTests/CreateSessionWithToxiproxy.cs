//-----------------------------------------------------------------------
// <copyright file="CreateSessionWithToxiproxy.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("Slow")]
    public class CreateSessionWithToxiproxy : IsFullTestWithToxyProxy {
        [Test]
        public void ConnectToRepoAndSimulateConnectionProblems([Range(-1, 20)]int blockedRequest, [Values(10)]int numberOfSyncEvents) {
            this.RetryOnInitialConnection = true;

            int reqNumber = 0;
            this.AuthProviderWrapper.OnAuthenticate += (object obj) => {
                if (reqNumber == blockedRequest) {
                    this.Proxy.Disable();
                } else {
                    this.Proxy.Enable();
                }

                reqNumber ++;
            };

            this.InitializeAndRunRepo(swallowExceptions: true);

            for (int i = 0; i < numberOfSyncEvents; i++ ) {
                int actualReq = reqNumber;
                this.AddStartNextSyncEvent();
                this.repo.Run();
                Assert.That(reqNumber, Is.GreaterThan(actualReq));
            }
        }
    }
}