using System;
using System.Threading.Tasks;
using TanzuForVS.ViewModels;

namespace TanzuForWpf
{
    public class MainWindowViewModel : AbstractViewModel, IMainWindowViewModel
    {
        public MainWindowViewModel(IServiceProvider services)
            : base(services)
        {
            DeploymentStatus = "Deployment hasn't started yet.";
        }

        private string _status;

        public string DeploymentStatus {

            get => this._status;

            set
            {
                this._status = value;
                this.RaisePropertyChangedEvent("DeploymentStatus");
            }
        }

        public bool CanOpenCloudExplorer(object arg)
        {
            return true;
        }

        public void OpenCloudExplorer(object arg)
        {
            ActiveView = ViewLocatorService.NavigateTo(typeof(CloudExplorerViewModel).Name);
        }

        public bool CanDeployApp(object arg)
        {
            return true;
        }

        public async Task DeployApp(object arg)
        {
            try
            {
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
