using System.Windows.Controls;
using System.Windows.Input;
using TanzuForVS.ViewModels;
using TanzuForVS.WpfViews.Commands;

namespace TanzuForVS.WpfViews
{
    /// <summary>
    /// Interaction logic for DeploymentDialogView.xaml
    /// </summary>
    public partial class DeploymentDialogView : UserControl, IDeploymentDialogView
    {
        public ICommand UploadAppCommand { get; }

        public DeploymentDialogView()
        {
            InitializeComponent();
        }

        public DeploymentDialogView(IDeploymentDialogViewModel viewModel)
        {
            UploadAppCommand = new AsyncDelegatingCommand(viewModel.DeployApp, viewModel.CanDeployApp);

            DataContext = viewModel;
            InitializeComponent();
        }

    }
}
