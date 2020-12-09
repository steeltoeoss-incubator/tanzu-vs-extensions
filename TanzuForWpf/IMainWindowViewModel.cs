using System.Threading.Tasks;
using TanzuForVS.ViewModels;

namespace TanzuForWpf
{
    public interface IMainWindowViewModel : IViewModel
    {
        bool CanOpenCloudExplorer(object arg);
        void OpenCloudExplorer(object arg);
        bool CanDeployApp(object arg);
        Task DeployApp(object arg);
    }
}
