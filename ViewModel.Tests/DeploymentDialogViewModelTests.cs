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
        public void OpenLoginView_UpdatesCfInstancesFromCloudFoundryService()
        {
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Never);
            _sut.OpenLoginView(null);
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Once);
        }

        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedCfIsNull()
        {
            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);
            Assert.IsNull(_sut.SelectedCf);
            Assert.IsNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Target not specified"));
        }
        
        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedOrgIsNull()
        {
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];

            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);
            Assert.IsNotNull(_sut.SelectedCf);
            Assert.IsNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Org not specified"));
        }
        
        [TestMethod]
        public async Task DeployApp_SetsDeploymentStatus_WhenSelectedSpaceIsNull()
        {
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];
            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[0];

            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);
            Assert.IsNotNull(_sut.SelectedCf);
            Assert.IsNotNull(_sut.SelectedOrg);
            Assert.IsNull(_sut.SelectedSpace);

            await _sut.DeployApp(null);
            Assert.IsTrue(_sut.DeploymentStatus.Contains("Space not specified"));
        }

        [TestMethod]
        public void SetSelectedCf_ClearsCfOrgsAndCfSpaces()
        {
            _sut.CfOrgs = new List<CloudFoundryOrganization>
            {
                _fakeCloudFoundryOrganizations[0],
                _fakeCloudFoundryOrganizations[1]
            };

            _sut.CfSpaces = new List<CloudFoundrySpace>
            {
                _fakeCloudFoundrySpaces[0],
                _fakeCloudFoundrySpaces[1]
            };

            Assert.IsNull(_sut.SelectedCf);
            Assert.AreEqual(2, _sut.CfOrgs.Count);
            Assert.AreEqual(2, _sut.CfSpaces.Count);
            eventsRaised = new List<string>(); // Ignore initial events

            _sut.SelectedCf = _fakeCloudFoundryInstances[1];

            Assert.AreEqual(0, _sut.CfOrgs.Count);
            Assert.AreEqual(0, _sut.CfSpaces.Count);
            Assert.AreEqual(3, eventsRaised.Count);
            Assert.AreEqual(eventsRaised[0], "CfOrgs");
            Assert.AreEqual(eventsRaised[1], "CfSpaces");
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
        public void SetSelectedOrg_ClearsCfSpaces()
        {
            _sut.CfSpaces = new List<CloudFoundrySpace>
            {
                _fakeCloudFoundrySpaces[0],
                _fakeCloudFoundrySpaces[1]
            };

            Assert.IsNull(_sut.SelectedOrg);
            Assert.AreEqual(2, _sut.CfSpaces.Count);
            eventsRaised = new List<string>(); // Ignore initial events

            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[1];

            Assert.AreEqual(0, _sut.CfSpaces.Count);
            Assert.AreEqual(2, eventsRaised.Count);
            Assert.AreEqual(eventsRaised[0], "CfSpaces");
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
        public void UpdateCfInstances_GetsCfsFromCloudFoundryService()
        {
            _sut.UpdateCfInstances();
            mockCloudFoundryService.Verify(mock => mock.CloudFoundryInstances, Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfOrgs_GetsOrgsForCurrentSelectedCf()
        {
            _sut.SelectedCf = _fakeCloudFoundryInstances[0];
            mockCloudFoundryService.Verify(mock => mock.GetOrgsForCfInstanceAsync(It.IsAny<CloudFoundryInstance>()), Times.Never);

            await _sut.UpdateCfOrgs();
            mockCloudFoundryService.Verify(mock => mock.GetOrgsForCfInstanceAsync(_fakeCloudFoundryInstances[0]), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfOrgs_EmptiesCfOrgs_WhenSelectedCfIsNull()
        {
            Exception thrownException = null;
            try
            {
                Assert.IsNull(_sut.SelectedCf);
                await _sut.UpdateCfOrgs();
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            Assert.IsNull(thrownException);
            Assert.AreEqual(0, _sut.CfOrgs.Count);
        }

        [TestMethod]
        public async Task UpdateCfSpaces_GetsSpacesForCurrentSelectedOrg()
        {
            _sut.SelectedOrg = _fakeCloudFoundryOrganizations[0];
            mockCloudFoundryService.Verify(mock => mock.GetSpacesForOrgAsync(It.IsAny<CloudFoundryOrganization>()), Times.Never);

            await _sut.UpdateCfSpaces();
            mockCloudFoundryService.Verify(mock => mock.GetSpacesForOrgAsync(_fakeCloudFoundryOrganizations[0]), Times.Once);
        }

        [TestMethod]
        public async Task UpdateCfSpaces_EmptiesCfSpaces_WhenSelectedOrgIsNull()
        {
            Exception thrownException = null;
            try
            {
                Assert.IsNull(_sut.SelectedOrg);
                await _sut.UpdateCfSpaces();
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            Assert.IsNull(thrownException);
            Assert.AreEqual(0, _sut.CfSpaces.Count);
        }
        
    }
}