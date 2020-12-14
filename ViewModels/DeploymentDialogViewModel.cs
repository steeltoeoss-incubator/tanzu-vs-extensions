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

        public DeploymentDialogViewModel(IServiceProvider services)
            : base(services)
        {
            DeploymentStatus = "Deployment hasn't started yet.";
            SelectedCf = null;
            updateCfInstances();
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
                if (value != selectedCf) // TODO: test that nothing happens (i.e. no network calls) if we try to re-assign the same value to this variable
                {
                    selectedCf = value;

                    if (value != null)
                    {
                        CfOrgs = new List<CloudFoundryOrganization>(); // TODO: test that org list is cleared while new orgs are loading
                        updateCfOrgs(value); // TODO: test that orgs are updated eventually (likely *after* this method completes)
                    }
                    else if (CfOrgs.Count > 0)
                    {
                        CfOrgs = new List<CloudFoundryOrganization>(); // TODO: test that CfOrgs is replaced with an empty list if selectedCf becomes null
                        CfSpaces = new List<CloudFoundrySpace>(); // TODO: test that CfSpaces is replaced with an empty list if selectedCf becomes null
                    }

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

                    if (value != null)
                    {
                        CfSpaces = new List<CloudFoundrySpace>(); // TODO: test that spaces list is cleared while new spaces are loading
                        updateCfSpaces(value); // TODO: test that spaces are updated eventually (likely *after* this method completes)
                    }
                    else if (CfSpaces.Count > 0)
                    {
                        CfSpaces = new List<CloudFoundrySpace>(); // TODO: test that CfSpaces is replaced with an empty list if selectedOrg becomes null
                    }
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
                var space = arg as CloudFoundrySpace;

                bool appWasDeployed = await CloudFoundryService.DeployAppAsync(space.ParentOrg.ParentCf, space.ParentOrg, space);

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
            updateCfInstances();
        }


        private void updateCfInstances()
        {
            CfInstances = new List<CloudFoundryInstance>(CloudFoundryService.CloudFoundryInstances.Values);
        }

        private async Task updateCfOrgs(CloudFoundryInstance cf)
        {
            var orgs = await CloudFoundryService.GetOrgsForCfInstanceAsync(cf);
            CfOrgs = orgs;
        }

        private async Task updateCfSpaces(CloudFoundryOrganization org)
        {
            var spaces = await CloudFoundryService.GetSpacesForOrgAsync(org);
            CfSpaces = spaces;
        }

    }
}
