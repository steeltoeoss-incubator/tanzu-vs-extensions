using System.Threading.Tasks;

namespace TanzuForVS.ViewModels
{
    public interface IDeploymentDialogViewModel
    {
        bool CanDeployApp(object arg);
        Task DeployApp(object arg);
    }
}
