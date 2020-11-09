﻿using System.Windows.Controls;
using System.Windows.Input;
using TanzuForVS.ViewModels;
using TanzuForVS.WpfViews.Commands;

namespace TanzuForVS.WpfViews
{
    /// <summary>
    /// Interaction logic for CloudExplorerView.xaml
    /// </summary>
    public partial class CloudExplorerView : UserControl, ICloudExplorerView
    {
        public ICommand OpenLoginFormCommand { get; }
        public ICommand StopCfAppCommand { get; }
        public ICommand StartCfAppCommand { get; }
        public ICommand DeleteCfAppCommand { get; }

        public CloudExplorerView()
        {
            InitializeComponent();
        }

        public CloudExplorerView(ICloudExplorerViewModel viewModel)
        {
            OpenLoginFormCommand = new DelegatingCommand(viewModel.OpenLoginView, viewModel.CanOpenLoginView);
            StopCfAppCommand = new AsyncDelegatingCommand(viewModel.StopCfApp, viewModel.CanStopCfApp);
            StartCfAppCommand = new AsyncDelegatingCommand(viewModel.StartCfApp, viewModel.CanStartCfApp);
            DeleteCfAppCommand = new AsyncDelegatingCommand(viewModel.DeleteCfApp, viewModel.CanDeleteCfApp);

            DataContext = viewModel;
            InitializeComponent();
        }

    }
}
