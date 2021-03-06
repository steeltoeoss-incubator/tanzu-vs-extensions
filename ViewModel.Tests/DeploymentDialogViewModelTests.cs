﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Tanzu.Toolkit.VisualStudio.Models;
using Tanzu.Toolkit.VisualStudio.Services;
using static Tanzu.Toolkit.VisualStudio.Services.OutputHandler.OutputHandler;

namespace Tanzu.Toolkit.VisualStudio.ViewModels.Tests
{
    [TestClass()]
    public class DeploymentDialogViewModelTests : ViewModelTestSupport
    {
        private static CloudFoundryInstance _fakeCfInstance = new CloudFoundryInstance("", "", "");
        private static CloudFoundryOrganization _fakeOrg = new CloudFoundryOrganization("", "", _fakeCfInstance);
        private CloudFoundrySpace _fakeSpace = new CloudFoundrySpace("", "", _fakeOrg);
        private const string _fakeAppName = "fake app name";
        private const string _fakeProjPath = "this\\is\\a\\fake\\path\\to\\a\\project\\directory";
        private DeploymentDialogViewModel _sut;

        [TestInitialize]
        public void TestInit()
        {
            //* return empty dictionary of CloudFoundryInstances
            mockCloudFoundryService.SetupGet(mock =>
                mock.CloudFoundryInstances)
                    .Returns(new Dictionary<string, CloudFoundryInstance>());

            //* return fake view/viewmodel for output window
            mockViewLocatorService.Setup(mock =>
                mock.NavigateTo(nameof(OutputViewModel), null))
                    .Returns(new FakeOutputView());

            _sut = new DeploymentDialogViewModel(services, _fakeProjPath);
        }

        [TestMethod()]
        public void DeploymentDialogViewModel_GetsListOfCfsFromCfService_WhenConstructed()
        {
            var vm = new DeploymentDialogViewModel(services, _fakeProjPath);

            mockCloudFoundryService.VerifyAll();
        }

        [TestMethod]
        public void DeployApp_UpdatesDeploymentStatus_WhenAppNameEmpty()
        {
            var receivedEvents = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            _sut.AppName = string.Empty;
            _sut.SelectedCf = _fakeCfInstance;
            _sut.SelectedOrg = _fakeOrg;
            _sut.SelectedSpace = _fakeSpace;

            _sut.DeployApp(null);

            Assert.IsTrue(receivedEvents.Contains("DeploymentStatus"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains("An error occurred:"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains(DeploymentDialogViewModel.appNameEmptyMsg));
        }

        [TestMethod]
        public void DeployApp_UpdatesDeploymentStatus_WhenTargetCfEmpty()
        {
            var receivedEvents = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            _sut.AppName = "fake app name";
            _sut.SelectedCf = null;
            _sut.SelectedOrg = _fakeOrg;
            _sut.SelectedSpace = _fakeSpace;

            _sut.DeployApp(null);

            Assert.IsTrue(receivedEvents.Contains("DeploymentStatus"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains("An error occurred:"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains(DeploymentDialogViewModel.targetEmptyMsg));
        }

        [TestMethod]
        public void DeployApp_UpdatesDeploymentStatus_WhenTargetOrgEmpty()
        {
            var receivedEvents = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            _sut.AppName = "fake app name";
            _sut.SelectedCf = _fakeCfInstance;
            _sut.SelectedOrg = null;
            _sut.SelectedSpace = _fakeSpace;

            _sut.DeployApp(null);

            Assert.IsTrue(receivedEvents.Contains("DeploymentStatus"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains("An error occurred:"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains(DeploymentDialogViewModel.orgEmptyMsg));
        }

        [TestMethod]
        public void DeployApp_UpdatesDeploymentStatus_WhenTargetSpaceEmpty()
        {
            var receivedEvents = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            _sut.AppName = "fake app name";
            _sut.SelectedCf = _fakeCfInstance;
            _sut.SelectedOrg = _fakeOrg;
            _sut.SelectedSpace = null;

            _sut.DeployApp(null);

            Assert.IsTrue(receivedEvents.Contains("DeploymentStatus"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains("An error occurred:"));
            Assert.IsTrue(_sut.DeploymentStatus.Contains(DeploymentDialogViewModel.spaceEmptyMsg));
        }

        [TestMethod]
        public void DeployApp_ClosesDeploymentDialog()
        {
            var dw = new object();

            _sut.AppName = _fakeAppName;
            _sut.SelectedCf = _fakeCfInstance;
            _sut.SelectedOrg = _fakeOrg;
            _sut.SelectedSpace = _fakeSpace;

            _sut.DeployApp(dw);
            mockDialogService.Verify(mock => mock.CloseDialog(dw, true), Times.Once);
        }

        [TestMethod]
        public async Task StartDeploymentTask_UpdatesDeploymentInProgress_WhenComplete()
        {
            var fakeExMsg = "I was thrown by cf service!";
            var fakeExTrace = "this is a stack trace: a<b<c<d<e";
            var fakeException = new FakeException(fakeExMsg, fakeExTrace);

            var receivedEvents = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            mockCloudFoundryService.Setup(mock =>
                mock.DeployAppAsync(_fakeCfInstance, _fakeOrg, _fakeSpace, _fakeAppName, _fakeProjPath, It.IsAny<StdOutDelegate>()))
                    .ThrowsAsync(fakeException);

            _sut.AppName = _fakeAppName;
            _sut.SelectedCf = _fakeCfInstance;
            _sut.SelectedOrg = _fakeOrg;
            _sut.SelectedSpace = _fakeSpace;

            Exception shouldStayNull = null;
            try
            {
                _sut.DeploymentInProgress = true;
                await _sut.StartDeployment();
            }
            catch (Exception ex)
            {
                shouldStayNull = ex;
            }

            Assert.IsNull(shouldStayNull);
            Assert.IsFalse(_sut.DeploymentInProgress);

            mockCloudFoundryService.VerifyAll(); // ensure DeployAppAsync was called with proper params
            mockViewLocatorService.VerifyAll(); // ensure we're using a mock output view/viewmodel
        }
    }

    class FakeException : Exception
    {
        private readonly string message;
        private readonly string stackTrace;

        public FakeException(string message = "", string stackTrace = "")
        {
            this.message = message;
            this.stackTrace = stackTrace;
        }

        public override string Message
        {
            get
            {
                return message;
            }
        }

        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }
    }

    class FakeOutputView : ViewModelTestSupport, IView
    {
        public IViewModel ViewModel { get; }

        public FakeOutputView()
        {
            ViewModel = new OutputViewModel(services);
        }
    }
}