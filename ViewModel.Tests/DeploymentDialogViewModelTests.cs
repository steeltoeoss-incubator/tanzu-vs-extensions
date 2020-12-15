using Microsoft.VisualStudio.TestTools.UnitTesting;
using TanzuForVS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TanzuForVS.Models;
using System.ComponentModel;
using System;

namespace TanzuForVS.ViewModelsTests
{
    [TestClass()]
    public class DeploymentDialogViewModelTests : ViewModelTestSupport
    {
        private DeploymentDialogViewModel _sut;
        private List<string> eventsRaised;

        [TestInitialize]
        public void TestInit()
        {
            mockCloudFoundryService.SetupGet(mock => mock.CloudFoundryInstances).Returns(_fakeCfDict);
            _sut = new DeploymentDialogViewModel(services);

            eventsRaised = new List<string>();
            _sut.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                eventsRaised.Add(e.PropertyName);
            };

            // Disregard any mock calls that occur in the DeploymentDialogViewModel constructor
            mockCloudFoundryService.Invocations.Clear();
        }

        [TestMethod]
        public void CanDeployApp_ReturnsTrue()
        {
            Assert.IsTrue(_sut.CanDeployApp(null));
        }

        [TestMethod]
        public void CanOpenLoginView_ReturnsTrue()
        {
            Assert.IsTrue(_sut.CanOpenLoginView(null));
        }

        [TestMethod]
        public void OpenLoginView_CallsDialogService_ShowDialog()
        {
            _sut.OpenLoginView(null);
            mockDialogService.Verify(ds => ds.ShowDialog(typeof(AddCloudDialogViewModel).Name, null), Times.Once);
        }

        [TestMethod]
        public void OpenLoginView_UpdatesCfInstanceOptionsFromCloudFoundryService()
        {
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Never);
            _sut.OpenLoginView(null);
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Once);
        }

        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenAppNameIsNull()
        {
            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);
            Assert.IsNull(_sut.AppName);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("App name not specified"));
        }

        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedCfIsNull()
        {
            _sut.AppName = "filler";

            Assert.IsNotNull(_sut.AppName);
            Assert.IsNull(_sut.SelectedCf);
            Assert.IsNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);
            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Target not specified"));
        }
        
        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedOrgIsNull()
        {
            _sut.AppName = "filler";
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];

            Assert.IsNotNull(_sut.AppName);
            Assert.IsNotNull(_sut.SelectedCf);
            Assert.IsNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);
            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Org not specified"));
        }
        
        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedSpaceIsNull()
        {
            _sut.AppName = "filler";
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];
            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[0];
            
            Assert.IsNotNull(_sut.AppName);
            Assert.IsNotNull(_sut.SelectedCf);
            Assert.IsNotNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);
            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Space not specified"));
        }

        [TestMethod]
        public void SetSelectedCf_ClearsCfOrgOptionsAndCfSpaceOptions()
        {
            _sut.CfOrgOptions = new List<CloudFoundryOrganization>
            {
                _fakeCloudFoundryOrganizations[0],
                _fakeCloudFoundryOrganizations[1]
            };

            _sut.CfSpaceOptions = new List<CloudFoundrySpace>
            {
                _fakeCloudFoundrySpaces[0],
                _fakeCloudFoundrySpaces[1]
            };

            Assert.IsNull(_sut.SelectedCf);
            Assert.AreEqual(2, _sut.CfOrgOptions.Count);
            Assert.AreEqual(2, _sut.CfSpaceOptions.Count);
            eventsRaised = new List<string>(); // Ignore initial events

            _sut.SelectedCf = _fakeCloudFoundryInstances[1];

            Assert.AreEqual(0, _sut.CfOrgOptions.Count);
            Assert.AreEqual(0, _sut.CfSpaceOptions.Count);
            Assert.AreEqual(3, eventsRaised.Count);
            Assert.AreEqual(eventsRaised[0], "CfOrgOptions");
            Assert.AreEqual(eventsRaised[1], "CfSpaceOptions");
            Assert.AreEqual(eventsRaised[2], "SelectedCf");
        }

        [TestMethod]
        public void SetSelectedCf_DoesNotUpdateSelectedCf_WhenNewValueIsNotDifferent()
        {
            var _sut = new DeploymentDialogViewModel(services)
            {
                SelectedCf = _fakeCloudFoundryInstances[0]
            };
            mockCloudFoundryService.Invocations.Clear(); // ignore mock calls made in ctor

            _sut.SelectedCf = _fakeCloudFoundryInstances[0];

            Assert.AreEqual(0, eventsRaised.Count);
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Never);
        }

        [TestMethod]
        public void SetSelectedOrg_DoesNothing_WhenNewValueIsNotDifferent()
        {
            DeploymentDialogViewModel _sut = new DeploymentDialogViewModel(services)
            {
                SelectedOrg = _fakeCloudFoundryOrganizations[0],
            };

            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[0];

            Assert.AreEqual(0, eventsRaised.Count);
            mockCloudFoundryService.Verify(mock => mock.GetOrgsForCfInstanceAsync(It.IsAny<CloudFoundryInstance>()), Times.Never);
        }

        [TestMethod]
        public void SetSelectedOrg_ClearsCfSpaceOptions()
        {
            _sut.CfSpaceOptions = new List<CloudFoundrySpace>
            {
                _fakeCloudFoundrySpaces[0],
                _fakeCloudFoundrySpaces[1]
            };

            Assert.IsNull(_sut.SelectedOrg);
            Assert.AreEqual(2, _sut.CfSpaceOptions.Count);
            eventsRaised = new List<string>(); // Ignore initial events

            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[1];

            Assert.AreEqual(0, _sut.CfSpaceOptions.Count);
            Assert.AreEqual(2, eventsRaised.Count);
            Assert.AreEqual(eventsRaised[0], "CfSpaceOptions");
            Assert.AreEqual(eventsRaised[1], "SelectedOrg");
        }

        [TestMethod]
        public void SetSelectedSpace_DoesNothing_WhenNewValueIsNotDifferent()
        {
            DeploymentDialogViewModel _sut = new DeploymentDialogViewModel(services)
            {
                SelectedSpace = _fakeCloudFoundrySpaces[0],
            };

            _sut.SelectedSpace = _fakeCloudFoundrySpaces[0];

            Assert.AreEqual(0, eventsRaised.Count);
            mockCloudFoundryService.Verify(mock => mock.GetSpacesForOrgAsync(It.IsAny<CloudFoundryOrganization>()), Times.Never);
        }

        [TestMethod]
        public void UpdateCfInstanceOptions_GetsCfsFromCloudFoundryService()
        {
            _sut.UpdateCfInstanceOptions();
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfOrgOptions_GetsOrgsForCurrentSelectedCf()
        {
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];
            mockCloudFoundryService.Verify(mock => mock.GetOrgsForCfInstanceAsync(It.IsAny<CloudFoundryInstance>()), Times.Never);

            await _sut.UpdateCfOrgOptions();
            mockCloudFoundryService.Verify(mock => mock.GetOrgsForCfInstanceAsync(_fakeCloudFoundryInstances[0]), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfOrgOptions_EmptiesCfOrgOptions_WhenSelectedCfIsNull()
        {
            Exception thrownException = null;
            try
            {
                Assert.IsNull(_sut.SelectedCf);
                await _sut.UpdateCfOrgOptions();
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            Assert.IsNull(thrownException);
            Assert.AreEqual(0, _sut.CfOrgOptions.Count);
        }

        [TestMethod]
        public async Task UpdateCfSpaceOptions_GetsSpacesForCurrentSelectedOrg()
        {
            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[0];
            mockCloudFoundryService.Verify(mock => mock.GetSpacesForOrgAsync(It.IsAny<CloudFoundryOrganization>()), Times.Never);

            await _sut.UpdateCfSpaceOptions();
            mockCloudFoundryService.Verify(mock => mock.GetSpacesForOrgAsync(_fakeCloudFoundryOrganizations[0]), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfSpaceOptions_EmptiesCfSpaceOptions_WhenSelectedOrgIsNull()
        {
            Exception thrownException = null;
            try
            {
                Assert.IsNull(_sut.SelectedOrg);
                await _sut.UpdateCfSpaceOptions();
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            Assert.IsNull(thrownException);
            Assert.AreEqual(0, _sut.CfSpaceOptions.Count);
        }
        
    }
}