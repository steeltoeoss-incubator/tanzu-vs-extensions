using System.Threading.Tasks;
using TanzuForVS.ViewModels;

namespace TanzuForWpf
{
    public interface IMainWindowViewModel : IViewModel
    {
        bool CanOpenCloudExplorer(object arg);
        bool CanOpenDeploymentDialog(object arg);
        void OpenCloudExplorer(object arg);
        void OpenDeploymentDialog(object arg);
    }
}
