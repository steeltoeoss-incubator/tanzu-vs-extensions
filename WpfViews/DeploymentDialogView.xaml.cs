using EnvDTE;
using EnvDTE80;
using System;
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
        private IDeploymentDialogViewModel _viewModel;
        private readonly DTE2 _dte;

        public ICommand UploadAppCommand { get; }
        public ICommand OpenLoginDialogCommand { get; }

        public DeploymentDialogView(IServiceProvider services)
        {
            InitializeComponent();
        }

        public DeploymentDialogView(IDeploymentDialogViewModel viewModel, DTE2 dte)
        {
            _viewModel = viewModel;
            _dte = dte;
            UploadAppCommand = new DelegatingCommand(viewModel.DeployApp, viewModel.CanDeployApp);
            OpenLoginDialogCommand = new DelegatingCommand(viewModel.OpenLoginView, viewModel.CanOpenLoginView);

            DataContext = viewModel;
            InitializeComponent();
        }

        private void CfInstanceOptions_ComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            _viewModel.UpdateCfOrgOptions();
        }

        private void CfOrgOptions_ComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            _viewModel.UpdateCfSpaceOptions();
        }

        private void DeploymentStatus_TextChanged(object sender, TextChangedEventArgs e)
        {
            var title = "My Cool Pane";

            OutputWindowPanes panes = _dte.ToolWindows.OutputWindow.OutputWindowPanes;
            OutputWindowPane deploymentPane;

            try
            {
                // If the pane exists already, write to it.
                deploymentPane = panes.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane and write to it.
                deploymentPane = panes.Add(title);
            }

            if (deploymentPane != null && sender is TextBox textBox)
            {
                deploymentPane.Clear();
                deploymentPane.Activate();

                deploymentPane.OutputString(textBox.Text);
            }

        }
    }
}