using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TanzuForVS.Models;

namespace TanzuForVS.ViewModels
{
    public class DeploymentDialogViewModel : AbstractViewModel, IDeploymentDialogViewModel
    {
        private string status;
        private List<CloudFoundryInstance> cfInstances;
        private List<CloudFoundryOrganization> cfOrgs;
        private List<CloudFoundrySpace> cfSpaces;
        private CloudFoundryInstance selectedCf;
        private CloudFoundryOrganization selectedOrg;
        private CloudFoundrySpace selectedSpace;

        public string initialStatus = "Deployment hasn't started yet.";

        public DeploymentDialogViewModel(IServiceProvider services)
            : base(services)
        {
            DeploymentStatus = initialStatus;
            SelectedCf = null;
            UpdateCfInstances(); // Test that ctor gets Cfs from CfService
        }


        public string DeploymentStatus
        {

            get => this.status;

            set
            {
                this.status = value;
                this.RaisePropertyChangedEvent("DeploymentStatus");
            }
        }

        public CloudFoundryInstance SelectedCf
        {
            get => selectedCf;

            set
            {
                if (value != selectedCf)
                {
                    selectedCf = value;

                    // clear orgs & spaces
                    CfOrgs = new List<CloudFoundryOrganization>();
                    CfSpaces = new List<CloudFoundrySpace>();

                    RaisePropertyChangedEvent("SelectedCf");
                }
            }
        }

        public CloudFoundryOrganization SelectedOrg
        {
            get => selectedOrg;

            set
            {
                if (value != selectedOrg)
                {
                    selectedOrg = value;

                    // clear spaces
                    CfSpaces = new List<CloudFoundrySpace>();
                }

                RaisePropertyChangedEvent("SelectedOrg");
            }
        }

        public CloudFoundrySpace SelectedSpace
        {
            get => selectedSpace;

            set
            {
                if (value != selectedSpace)
                {
                    selectedSpace = value;
                }

                RaisePropertyChangedEvent("SelectedSpace");
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

        public List<CloudFoundryOrganization> CfOrgs
        {
            get => cfOrgs;

            set
            {
                cfOrgs = value;
                this.RaisePropertyChangedEvent("CfOrgs");
            }
        }

        public List<CloudFoundrySpace> CfSpaces
        {
            get => cfSpaces;

            set
            {
                cfSpaces = value;
                this.RaisePropertyChangedEvent("CfSpaces");
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
                if (SelectedCf == null) throw new Exception("Target not specified");
                if (SelectedOrg == null) throw new Exception("Org not specified");
                if (SelectedSpace == null) throw new Exception("Space not specified");

                bool appWasDeployed = await CloudFoundryService.DeployAppAsync(SelectedSpace.ParentOrg.ParentCf, SelectedSpace.ParentOrg, SelectedSpace);

                if (appWasDeployed) DeploymentStatus = "App was successfully deployed!";

                DeploymentStatus = "CloudFoundryService.DeployAppAsync returned false.";
            }
            catch (Exception e)
            {
                DeploymentStatus = $"An error occurred: \n{e}";
            }
        }

        // TODO: Consolidate duplicate code: these methods already live on CloudExplorerViewModel
        // (maybe pull up to base view model class?)
        public bool CanOpenLoginView(object arg)
        {
            return true;
        }

        public void OpenLoginView(object arg)
        {
            DialogService.ShowDialog(typeof(AddCloudDialogViewModel).Name);
            UpdateCfInstances();
        }


        public void UpdateCfInstances()
        {
            CfInstances = new List<CloudFoundryInstance>(CloudFoundryService.CloudFoundryInstances.Values);
        }

        public async Task UpdateCfOrgs()
        {
            if (SelectedCf == null) CfOrgs = new List<CloudFoundryOrganization>();

            else
            {
                var orgs = await CloudFoundryService.GetOrgsForCfInstanceAsync(SelectedCf);
                CfOrgs = orgs;
            }
        }

        public async Task UpdateCfSpaces()
        {
            if (SelectedOrg == null) CfSpaces = new List<CloudFoundrySpace>();

            else
            {
                var spaces = await CloudFoundryService.GetSpacesForOrgAsync(SelectedOrg);
                CfSpaces = spaces;
            }
        }

    }
}
