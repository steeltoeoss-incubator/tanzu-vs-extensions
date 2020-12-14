using Microsoft.VisualStudio.TestTools.UnitTesting;
using TanzuForVS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TanzuForVS.Models;
using System.ComponentModel;

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
        public async Task DeployApp_SetsDeploymentStatus_IfArgTypeIsNotCloudFoundrySpace()
        {
            object argThatIsNotACloudFoundrySpace = new object();

            Assert.AreEqual(_sut.initialStatus, _sut.DeploymentStatus);
            Assert.IsFalse(_sut.DeploymentStatus.Contains("error occurred"));
 
            await _sut.DeployApp(argThatIsNotACloudFoundrySpace);

            Assert.IsTrue(_sut.DeploymentStatus.Contains("error occurred"));
        }

        [TestMethod]
        public void SetSelectedCf_ClearsCfOrgs_WhenSelectedCfIsNull()
        {
            DeploymentDialogViewModel _sut = new DeploymentDialogViewModel(services)
            {
                SelectedCf = _fakeCloudFoundryInstances[0],
                CfOrgs = new List<CloudFoundryOrganization>
                {
                    _fakeCloudFoundryOrganizations[0],
                    _fakeCloudFoundryOrganizations[1]
                }
            };

            _sut.SelectedCf = _fakeCloudFoundryInstances[1];

            Assert.AreEqual(1, eventsRaised.Count);
            // TODO: finish this test
        }
    }
}