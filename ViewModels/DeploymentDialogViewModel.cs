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
            UpdateCfInstanceOptions(); // Test that ctor gets Cfs from CfService
        }


        private string appName;

        public string AppName
        {
            get => appName;

            set
            {
                appName = value;
                RaisePropertyChangedEvent("AppName");
            }
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
                    CfOrgOptions = new List<CloudFoundryOrganization>();
                    CfSpaceOptions = new List<CloudFoundrySpace>();

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
                    CfSpaceOptions = new List<CloudFoundrySpace>();
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

        public List<CloudFoundryInstance> CfInstanceOptions
        {
            get => cfInstances;

            set
            {
                cfInstances = value;
                this.RaisePropertyChangedEvent("CfInstanceOptions");
            }
        }

        public List<CloudFoundryOrganization> CfOrgOptions
        {
            get => cfOrgs;

            set
            {
                cfOrgs = value;
                this.RaisePropertyChangedEvent("CfOrgOptions");
            }
        }

        public List<CloudFoundrySpace> CfSpaceOptions
        {
            get => cfSpaces;

            set
            {
                cfSpaces = value;
                this.RaisePropertyChangedEvent("CfSpaceOptions");
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
                if (string.IsNullOrEmpty(AppName)) throw new Exception("App name not specified");
                if (SelectedCf == null) throw new Exception("Target not specified");
                if (SelectedOrg == null) throw new Exception("Org not specified");
                if (SelectedSpace == null) throw new Exception("Space not specified");

                bool appWasDeployed = await CloudFoundryService.DeployAppAsync(SelectedSpace.ParentOrg.ParentCf,
                                                                               SelectedSpace.ParentOrg,
                                                                               SelectedSpace,
                                                                               AppName);

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
            UpdateCfInstanceOptions();
        }


        public void UpdateCfInstanceOptions()
        {
            CfInstanceOptions = new List<CloudFoundryInstance>(CloudFoundryService.CloudFoundryInstances.Values);
        }

        public async Task UpdateCfOrgOptions()
        {
            if (SelectedCf == null) CfOrgOptions = new List<CloudFoundryOrganization>();

            else
            {
                var orgs = await CloudFoundryService.GetOrgsForCfInstanceAsync(SelectedCf);
                CfOrgOptions = orgs;
            }
        }

        public async Task UpdateCfSpaceOptions()
        {
            if (SelectedOrg == null) CfSpaceOptions = new List<CloudFoundrySpace>();

            else
            {
                var spaces = await CloudFoundryService.GetSpacesForOrgAsync(SelectedOrg);
                CfSpaceOptions = spaces;
            }
        }

    }
}
