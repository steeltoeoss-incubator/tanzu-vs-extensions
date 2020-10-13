﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using TanzuForVS.CloudFoundryApiClient.Models.OrgsResponse;

namespace TanzuForVS.Services.CloudFoundry
{
    [TestClass()]
    public class CloudFoundryServiceTests : ServicesTestSupport
    {
        ICloudFoundryService cfService;
        string fakeValidTarget = "https://my.fake.target";
        string fakeValidUsername = "junk";
        SecureString fakeValidPassword = new SecureString();
        string fakeHttpProxy = "junk";
        bool skipSsl = true;
        string fakeLoginSuccessResponse = "login success!";
        string fakeValidAccessToken = "valid token";

        [TestInitialize()]
        public void TestInit()
        {
            cfService = new CloudFoundryService(services);
        }

        [TestMethod()]
        public async Task ConnectToCFAsync_ThrowsExceptions_WhenParametersAreInvalid()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => cfService.ConnectToCFAsync(null, null, null, null, false));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => cfService.ConnectToCFAsync(string.Empty, null, null, null, false));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => cfService.ConnectToCFAsync("Junk", null, null, null, false));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => cfService.ConnectToCFAsync("Junk", string.Empty, null, null, false));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => cfService.ConnectToCFAsync("Junk", "Junk", null, null, false));
        }

        [TestMethod()]
        public async Task ConnectToCFAsync_ReturnsConnectResult_WhenLoginSucceeds()
        {
            mockCfApiClient.Setup(mock => mock.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(fakeValidAccessToken);

            ConnectResult result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            Assert.IsTrue(result.IsLoggedIn);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(fakeValidAccessToken, result.Token);
            mockCfApiClient.Verify(mock => mock.LoginAsync(fakeValidTarget, fakeValidUsername, It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task ConnectToCFAsync_ReturnsConnectResult_WhenLoginFails()
        {
            mockCfApiClient.Setup(mock => mock.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string)null);

            ConnectResult result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            Assert.IsFalse(result.IsLoggedIn);
            Assert.IsTrue(result.ErrorMessage.Contains(cfService.LoginFailureMessage));
            Assert.IsNull(result.Token);
            mockCfApiClient.Verify(mock => mock.LoginAsync(fakeValidTarget, fakeValidUsername, It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public async Task ConnectToCfAsync_IncludesNestedExceptionMessages_WhenExceptionIsThrown()
        {
            string baseMessage = "base exception message";
            string innerMessage = "inner exception message";
            string outerMessage = "outer exception message";
            Exception multilayeredException = new Exception(outerMessage, new Exception(innerMessage, new Exception(baseMessage)));

            mockCfApiClient.Setup(mock => mock.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(multilayeredException);

            ConnectResult result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            Assert.IsNull(result.Token);
            Assert.IsTrue(result.ErrorMessage.Contains(baseMessage));
            Assert.IsTrue(result.ErrorMessage.Contains(innerMessage));
            Assert.IsTrue(result.ErrorMessage.Contains(outerMessage));
        }

        [TestMethod()]
        public async Task GetOrgNamesAsync_ReturnsListOfStrings_WhenListOrgsSuceeds()
        {
            const string org1Name = "org1";
            const string org2Name = "org2";
            const string org3Name = "org3";
            const string org4Name = "org4";

            var list = new List<Resource>
            {
                new Resource(org1Name),
                new Resource(org2Name),
                new Resource(org3Name),
                new Resource(org4Name)
            };

            var expectedResult = new List<string>
            {
                org1Name,
                org2Name,
                org3Name,
                org4Name
            };

            mockCfApiClient.Setup(mock => mock.ListOrgs(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(list);

            var result = await cfService.GetOrgNamesAsync(fakeValidTarget, fakeValidAccessToken);

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            CollectionAssert.AreEqual(expectedResult, result);
            mockCfApiClient.Verify(mock => mock.ListOrgs(fakeValidTarget, fakeValidAccessToken), Times.Once);
        }

        [TestMethod()]
        public void AddCloudFoundryInstance_ThrowsException_WhenNameAlreadyExists()
        {
            var duplicateName = "fake name";
            cfService.AddCloudFoundryInstance(duplicateName, null, null);
            Exception expectedException = null;

            try
            {
                cfService.AddCloudFoundryInstance(duplicateName, null, null);

            }
            catch (Exception e)
            {
                expectedException = e;
            }

            Assert.IsNotNull(expectedException);
            Assert.IsTrue(expectedException.Message.Contains(duplicateName));
            Assert.IsTrue(expectedException.Message.Contains("already exists"));
        }
    }
}
