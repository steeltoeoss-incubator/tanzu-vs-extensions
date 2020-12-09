using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TanzuForVS.Models;

namespace TanzuForVS.ViewModels
{
    public class DeploymentDialogViewModel : AbstractViewModel, IDeploymentDialogViewModel
    {
        public DeploymentDialogViewModel(IServiceProvider services)
            : base(services)
        {
            DeploymentStatus = "Deployment hasn't started yet.";

            //CfInstances = new List<CloudFoundryInstance>(CloudFoundryService.CloudFoundryInstances.Values);
            CfInstances = new List<CloudFoundryInstance>
            {
                new CloudFoundryInstance("name 1", "address 1", "token 1"),
                new CloudFoundryInstance("name 2", "address 2", "token 2"),
                new CloudFoundryInstance("name 3", "address 3", "token 3")
            };

        }

        private string status;
        private List<CloudFoundryInstance> cfInstances;
        private CloudFoundryInstance selectedCf;

        public string DeploymentStatus
        {

            get => this.status;

            set
            {
                this.status = value;
                this.RaisePropertyChangedEvent("DeploymentStatus");
            }
        }

        public List<CloudFoundryInstance> CfInstances
        {
            get => cfInstances;

            set
            {
                cfInstances = value;
                this.RaisePropertyChangedEvent("CfInstances");
            }
        }

        public CloudFoundryInstance SelectedCf
        {
            get => selectedCf;
            set
            {
                selectedCf = value;
                this.RaisePropertyChangedEvent("SelectedCf");
            }
        }

        public bool CanDeployApp(object arg)
        {
            return true;
        }

        public async Task DeployApp(object arg)
        {
            try
            {
                if (CloudFoundryService.ActiveCloud == null)
                {
                    throw new Exception("Unclear which CF to target; ActiveCloud == null");
                }
                bool appWasDeployed = await CloudFoundryService.DeployAppAsync();
                if (appWasDeployed) DeploymentStatus = "App was successfully deployed!";
                DeploymentStatus = "CloudFoundryService.DeployAppAsync returned false.";
            }
            catch (Exception e)
            {
                DeploymentStatus = $"An error occurred: \n{e}";
            }
        }
    }
}
