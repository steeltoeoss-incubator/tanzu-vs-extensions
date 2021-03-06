﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Tanzu.Toolkit.CloudFoundryApiClient.Models.AppsResponse;
using Tanzu.Toolkit.CloudFoundryApiClient.Models.OrgsResponse;
using Tanzu.Toolkit.CloudFoundryApiClient.Models.SpacesResponse;
using Tanzu.Toolkit.VisualStudio.Models;
using Tanzu.Toolkit.VisualStudio.Services;
using Tanzu.Toolkit.VisualStudio.Services.CloudFoundry;
using static Tanzu.Toolkit.VisualStudio.Services.OutputHandler.OutputHandler;

namespace Tanzu.Toolkit.VisualStudio.Services.Tests.CloudFoundry
{
    [TestClass()]
    public class CloudFoundryServiceTests : ServicesTestSupport
    {
        ICloudFoundryService cfService;
        readonly string fakeValidTarget = "https://my.fake.target";
        readonly string fakeValidUsername = "junk";
        readonly SecureString fakeValidPassword = new SecureString();
        readonly string fakeHttpProxy = "junk";
        readonly bool skipSsl = true;
        readonly string fakeValidAccessToken = "valid token";
        readonly string fakeProjectPath = "this\\is\\a\\fake\\path";
        CloudFoundryInstance fakeCfInstance;
        CloudFoundryOrganization fakeOrg;
        CloudFoundrySpace fakeSpace;
        CloudFoundryApp fakeApp;

        [TestInitialize()]
        public void TestInit()
        {
            cfService = new CloudFoundryService(services);
            fakeCfInstance = new CloudFoundryInstance("fake cf", fakeValidTarget, fakeValidAccessToken);
            fakeOrg = new CloudFoundryOrganization("fake org", "fake org guid", fakeCfInstance);
            fakeSpace = new CloudFoundrySpace("fake space", "fake space guid", fakeOrg);
            fakeApp = new CloudFoundryApp("fake app", "fake app guid", fakeSpace);
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
            mockCfCliService.Setup(mock => mock.ExecuteCfCliCommandAsync(It.IsAny<string>(), It.IsAny<StdOutDelegate>(), It.IsAny<string>())).ReturnsAsync(new DetailedResult(true));

            ConnectResult result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            Assert.IsTrue(result.IsLoggedIn);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(fakeValidAccessToken, result.Token);
            mockCfApiClient.Verify(mock => mock.LoginAsync(fakeValidTarget, fakeValidUsername, It.IsAny<string>()), Times.Once);
            mockCfCliService.VerifyAll();
        }

        [TestMethod()]
        public async Task ConnectToCFAsync_ReturnsConnectResult_WhenLoginFails()
        {
            mockCfApiClient.Setup(mock => mock.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string)null);
            mockCfCliService.Setup(mock => mock.ExecuteCfCliCommandAsync(It.IsAny<string>(), It.IsAny<StdOutDelegate>(), It.IsAny<string>())).ReturnsAsync(new DetailedResult(true));

            ConnectResult result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            Assert.IsFalse(result.IsLoggedIn);
            Assert.IsTrue(result.ErrorMessage.Contains(cfService.LoginFailureMessage));
            Assert.IsNull(result.Token);
            mockCfApiClient.Verify(mock => mock.LoginAsync(fakeValidTarget, fakeValidUsername, It.IsAny<string>()), Times.Once);
            mockCfCliService.VerifyAll();
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

        [TestMethod]
        public async Task ConnectToCfAsync_InvokesCfApiAndCfAuthCommands()
        {
            var cfApiArgs = $"api {fakeValidTarget}{(skipSsl ? " --skip-ssl-validation" : string.Empty)}";
            var fakeCfApiResponse = new DetailedResult(true);
            var fakePasswordStr = new System.Net.NetworkCredential(string.Empty, fakeValidPassword).Password;
            var cfAuthArgs = $"auth {fakeValidUsername} {fakePasswordStr}";
            var fakeCfAuthResponse = new DetailedResult(true);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfApiArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfApiResponse);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfAuthArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfAuthResponse);

            var result = await cfService.ConnectToCFAsync(fakeValidTarget, fakeValidUsername, fakeValidPassword, fakeHttpProxy, skipSsl);

            mockCfCliService.VerifyAll();
        }

        [TestMethod()]
        public async Task GetOrgsForCfInstanceAsync_ReturnsEmptyList_WhenListOrgsFails()
        {
            var expectedResult = new List<CloudFoundryOrganization>();

            mockCfApiClient.Setup(mock => mock.ListOrgs(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((List<Org>)null);

            var result = await cfService.GetOrgsForCfInstanceAsync(fakeCfInstance);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            CollectionAssert.AreEqual(expectedResult, result);
            Assert.AreEqual(typeof(List<CloudFoundryOrganization>), result.GetType());
            mockCfApiClient.Verify(mock => mock.ListOrgs(fakeValidTarget, fakeValidAccessToken), Times.Once);
        }

        [TestMethod()]
        public async Task GetOrgsForCfInstanceAsync_ReturnsListOfOrgs_WhenListOrgsSuceeds()
        {
            const string org1Name = "org1";
            const string org2Name = "org2";
            const string org3Name = "org3";
            const string org4Name = "org4";
            const string org1Guid = "org-1-id";
            const string org2Guid = "org-2-id";
            const string org3Guid = "org-3-id";
            const string org4Guid = "org-4-id";

            var mockOrgsResponse = new List<Org>
            {
                new Org
                {
                    name = org1Name,
                    guid = org1Guid
                },
                new Org
                {
                    name = org2Name,
                    guid = org2Guid
                },
                new Org
                {
                    name = org3Name,
                    guid = org3Guid
                },
                new Org
                {
                    name = org4Name,
                    guid = org4Guid
                }
            };

            var expectedResult = new List<CloudFoundryOrganization>
            {
                new CloudFoundryOrganization(org1Name, org1Guid, fakeCfInstance),
                new CloudFoundryOrganization(org2Name, org2Guid, fakeCfInstance),
                new CloudFoundryOrganization(org3Name, org3Guid, fakeCfInstance),
                new CloudFoundryOrganization(org4Name, org4Guid, fakeCfInstance)
            };

            mockCfApiClient.Setup(mock => mock.ListOrgs(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockOrgsResponse);

            var result = await cfService.GetOrgsForCfInstanceAsync(fakeCfInstance);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Count, result.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].OrgId, result[i].OrgId);
                Assert.AreEqual(expectedResult[i].OrgName, result[i].OrgName);
                Assert.AreEqual(expectedResult[i].ParentCf, result[i].ParentCf);
            }

            mockCfApiClient.Verify(mock => mock.ListOrgs(fakeValidTarget, fakeValidAccessToken), Times.Once);
        }

        [TestMethod()]
        public async Task GetSpacesForOrgAsync_ReturnsEmptyList_WhenListSpacesFails()
        {
            var expectedResult = new List<CloudFoundrySpace>();

            mockCfApiClient.Setup(mock => mock.ListSpacesForOrg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((List<Space>)null);

            var result = await cfService.GetSpacesForOrgAsync(fakeOrg);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            CollectionAssert.AreEqual(expectedResult, result);
            Assert.AreEqual(typeof(List<CloudFoundrySpace>), result.GetType());
            mockCfApiClient.Verify(mock => mock.ListSpacesForOrg(fakeValidTarget, fakeValidAccessToken, fakeOrg.OrgId),
                Times.Once);
        }

        [TestMethod()]
        public async Task GetSpacesForOrgAsync_ReturnsListOfSpaces_WhenListSpacesSuceeds()
        {
            const string space1Name = "space1";
            const string space2Name = "space2";
            const string space3Name = "space3";
            const string space4Name = "space4";
            const string space1Guid = "space-1-id";
            const string space2Guid = "space-2-id";
            const string space3Guid = "space-3-id";
            const string space4Guid = "space-4-id";

            var mockSpacesResponse = new List<Space>
            {
                new Space
                {
                    name = space1Name,
                    guid = space1Guid
                },
                new Space
                {
                    name = space2Name,
                    guid = space2Guid
                },
                new Space
                {
                    name = space3Name,
                    guid = space3Guid
                },
                new Space
                {
                    name = space4Name,
                    guid = space4Guid
                }
            };

            var expectedResult = new List<CloudFoundrySpace>
            {
                new CloudFoundrySpace(space1Name, space1Guid, fakeOrg),
                new CloudFoundrySpace(space2Name, space2Guid, fakeOrg),
                new CloudFoundrySpace(space3Name, space3Guid, fakeOrg),
                new CloudFoundrySpace(space4Name, space4Guid, fakeOrg)
            };

            mockCfApiClient.Setup(mock => mock.ListSpacesForOrg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockSpacesResponse);

            var result = await cfService.GetSpacesForOrgAsync(fakeOrg);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Count, result.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].SpaceId, result[i].SpaceId);
                Assert.AreEqual(expectedResult[i].SpaceName, result[i].SpaceName);
                Assert.AreEqual(expectedResult[i].ParentOrg, result[i].ParentOrg);
            }

            mockCfApiClient.Verify(mock => mock.ListSpacesForOrg(fakeValidTarget, fakeValidAccessToken, fakeOrg.OrgId),
                Times.Once);
        }

        [TestMethod()]
        public async Task GetAppsForSpaceAsync_ReturnsEmptyList_WhenListAppsFails()
        {
            var expectedResult = new List<CloudFoundryApp>();

            mockCfApiClient.Setup(mock => mock.ListAppsForSpace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((List<App>)null);

            var result = await cfService.GetAppsForSpaceAsync(fakeSpace);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            CollectionAssert.AreEqual(expectedResult, result);
            Assert.AreEqual(typeof(List<CloudFoundryApp>), result.GetType());
            mockCfApiClient.Verify(mock => mock.ListAppsForSpace(fakeValidTarget, fakeValidAccessToken, fakeSpace.SpaceId),
                Times.Once);
        }

        [TestMethod()]
        public async Task GetAppsForSpaceAsync_ReturnsListOfApps_WhenListAppsSuceeds()
        {
            const string app1Name = "app1";
            const string app2Name = "app2";
            const string app3Name = "app3";
            const string app4Name = "app4";
            const string app1Guid = "app-1-id";
            const string app2Guid = "app-2-id";
            const string app3Guid = "app-3-id";
            const string app4Guid = "app-4-id";

            var mockAppsResponse = new List<App>
            {
                new App
                {
                    name = app1Name,
                    guid = app1Guid
                },
                new App
                {
                    name = app2Name,
                    guid = app2Guid
                },
                new App
                {
                    name = app3Name,
                    guid = app3Guid
                },
                new App
                {
                    name = app4Name,
                    guid = app4Guid
                }
            };

            var expectedResult = new List<CloudFoundryApp>
            {
                new CloudFoundryApp(app1Name, app1Guid, fakeSpace),
                new CloudFoundryApp(app2Name, app2Guid, fakeSpace),
                new CloudFoundryApp(app3Name, app3Guid, fakeSpace),
                new CloudFoundryApp(app4Name, app4Guid, fakeSpace)
            };

            mockCfApiClient.Setup(mock => mock.ListAppsForSpace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockAppsResponse);

            var result = await cfService.GetAppsForSpaceAsync(fakeSpace);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Count, result.Count);

            for (int i = 0; i < expectedResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i].AppId, result[i].AppId);
                Assert.AreEqual(expectedResult[i].AppName, result[i].AppName);
                Assert.AreEqual(expectedResult[i].ParentSpace, result[i].ParentSpace);
            }

            mockCfApiClient.Verify(mock => mock.ListAppsForSpace(fakeValidTarget, fakeValidAccessToken, fakeSpace.SpaceId),
                Times.Once);
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

        [TestMethod]
        public async Task StopAppAsync_ReturnsFalseAndDoesNotThrowException_WhenRequestInfoIsMissing()
        {
            var fakeApp = new CloudFoundryApp("fake app name", "fake app guid", parentSpace: null);

            mockCfApiClient.Setup(mock => mock.StopAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            bool stopResult = true;
            Exception shouldStayNull = null;
            try
            {
                stopResult = await cfService.StopAppAsync(fakeApp);
            }
            catch (Exception e)
            {
                shouldStayNull = e;
            }

            Assert.IsFalse(stopResult);
            Assert.IsNull(shouldStayNull);
        }

        [TestMethod]
        public async Task StartAppAsync_UpdatesAppState_WhenAppStateChanges()
        {
            fakeApp.State = "STOPPED";

            mockCfApiClient.Setup(mock => mock.StartAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            await cfService.StartAppAsync(fakeApp);

            Assert.AreEqual("STARTED", fakeApp.State);
        }

        [TestMethod]
        public async Task StartAppAsync_ReturnsFalseAndDoesNotThrowException_WhenRequestInfoIsMissing()
        {
            var fakeApp = new CloudFoundryApp("fake app name", "fake app guid", parentSpace: null);

            mockCfApiClient.Setup(mock => mock.StartAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            bool startResult = true;
            Exception shouldStayNull = null;
            try
            {
                startResult = await cfService.StartAppAsync(fakeApp);
            }
            catch (Exception e)
            {
                shouldStayNull = e;
            }

            Assert.IsFalse(startResult);
            Assert.IsNull(shouldStayNull);
        }

        [TestMethod]
        public async Task StopAppAsync_UpdatesAppState_WhenAppStateChanges()
        {
            fakeApp.State = "STARTED";

            mockCfApiClient.Setup(mock => mock.StopAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            await cfService.StopAppAsync(fakeApp);

            Assert.AreEqual("STOPPED", fakeApp.State);
        }

        [TestMethod]
        public async Task DeleteAppAsync_ReturnsFalseAndDoesNotThrowException_WhenRequestInfoIsMissing()
        {
            var fakeApp = new CloudFoundryApp("fake app name", "fake app guid", parentSpace: null);

            mockCfApiClient.Setup(mock => mock.DeleteAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            bool deleteResult = true;
            Exception shouldStayNull = null;
            try
            {
                deleteResult = await cfService.DeleteAppAsync(fakeApp);
            }
            catch (Exception e)
            {
                shouldStayNull = e;
            }

            Assert.IsFalse(deleteResult);
            Assert.IsNull(shouldStayNull);
            mockCfApiClient.Verify(mock => mock.DeleteAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId), Times.Never);
        }

        [TestMethod]
        public async Task DeleteAppAsync_UpdatesAppState_WhenAppStateChangesFromStarted()
        {
            fakeApp.State = "STARTED";

            mockCfApiClient.Setup(mock => mock.DeleteAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            await cfService.DeleteAppAsync(fakeApp);

            Assert.AreEqual("DELETED", fakeApp.State);
        }

        [TestMethod]
        public async Task DeleteAppAsync_UpdatesAppState_WhenAppStateChangesFromStopped()
        {
            fakeApp.State = "STOPPED";

            mockCfApiClient.Setup(mock => mock.DeleteAppWithGuid(It.IsAny<string>(), It.IsAny<string>(), fakeApp.AppId))
                .ReturnsAsync(true);

            await cfService.DeleteAppAsync(fakeApp);

            Assert.AreEqual("DELETED", fakeApp.State);
        }

        [TestMethod]
        public async Task DeployAppAsync_ReturnsFalseResult_WhenCfTargetCommandFails()
        {
            const string fakeFailureExplanation = "cf target failed";
            var fakeCfCmdResponse = new DetailedResult(false, fakeFailureExplanation);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(It.IsAny<string>(), It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfCmdResponse);

            mockFileLocatorService.Setup(mock => mock.DirContainsFiles(It.IsAny<string>())).Returns(true);

            DetailedResult result = await cfService.DeployAppAsync(fakeCfInstance, fakeOrg, fakeSpace, fakeApp.AppName, fakeProjectPath, stdOutHandler: null);

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Explanation.Contains(fakeFailureExplanation));

            var cfTargetArgs = $"target -o {fakeOrg.OrgName} -s {fakeSpace.SpaceName}";
            mockCfCliService.Verify(mock =>
                mock.ExecuteCfCliCommandAsync(cfTargetArgs, null, null),
                    Times.Once);
        }

        [TestMethod]
        public async Task DeployAppAsync_ReturnsFalseResult_WhenCfPushCommandFails()
        {
            const string fakeFailureExplanation = "cf push failed";
            var cfTargetArgs = $"target -o {fakeOrg.OrgName} -s {fakeSpace.SpaceName}";
            var cfPushArgs = $"push {fakeApp.AppName}";
            var fakeCfTargetResponse = new DetailedResult(true);
            var fakeCfPushResponse = new DetailedResult(false, fakeFailureExplanation);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfTargetArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfTargetResponse);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfPushArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfPushResponse);

            mockFileLocatorService.Setup(mock => mock.DirContainsFiles(It.IsAny<string>())).Returns(true);

            DetailedResult result = await cfService.DeployAppAsync(fakeCfInstance, fakeOrg, fakeSpace, fakeApp.AppName, fakeProjectPath, stdOutHandler: null);

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Explanation.Contains(fakeFailureExplanation));

            mockCfCliService.Verify(mock =>
                mock.ExecuteCfCliCommandAsync(cfTargetArgs, null, null),
                    Times.Once);

            mockCfCliService.Verify(mock =>
                mock.ExecuteCfCliCommandAsync(cfPushArgs, null, fakeProjectPath),
                    Times.Once);
        }

        [TestMethod]
        public async Task DeployAppAsync_ReturnsTrueResult_WhenCfTargetAndPushCommandsSucceed()
        {
            var cfTargetArgs = $"target -o {fakeOrg.OrgName} -s {fakeSpace.SpaceName}";
            var cfPushArgs = $"push {fakeApp.AppName}";
            var fakeCfTargetResponse = new DetailedResult(true);
            var fakeCfPushResponse = new DetailedResult(true);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfTargetArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfTargetResponse);

            mockCfCliService.Setup(mock =>
                mock.ExecuteCfCliCommandAsync(cfPushArgs, It.IsAny<StdOutDelegate>(), It.IsAny<string>()))
                    .ReturnsAsync(fakeCfPushResponse);

            mockFileLocatorService.Setup(mock => mock.DirContainsFiles(It.IsAny<string>())).Returns(true);

            DetailedResult result = await cfService.DeployAppAsync(fakeCfInstance, fakeOrg, fakeSpace, fakeApp.AppName, fakeProjectPath, stdOutHandler: null);

            Assert.IsTrue(result.Succeeded);

            mockCfCliService.Verify(mock =>
                mock.ExecuteCfCliCommandAsync(cfTargetArgs, null, null),
                    Times.Once);

            mockCfCliService.Verify(mock =>
                mock.ExecuteCfCliCommandAsync(cfPushArgs, null, fakeProjectPath),
                    Times.Once);
        }

        [TestMethod]
        public async Task DeployAppAsync_ReturnsFalseResult_WhenProjDirContainsNoFiles()
        {
            mockFileLocatorService.Setup(mock => mock.DirContainsFiles(It.IsAny<string>())).Returns(false);

            var result = await cfService.DeployAppAsync(fakeCfInstance, fakeOrg, fakeSpace, fakeApp.AppName, fakeProjectPath, null);

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Explanation.Contains(CloudFoundryService.emptyOutputDirMessage));
            mockFileLocatorService.VerifyAll();
        }

    }
}
