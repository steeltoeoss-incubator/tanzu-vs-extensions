﻿using System;
using TanzuForVS.ViewModels;

namespace TanzuForWpf
{
    public class MainWindowViewModel : AbstractViewModel, IMainWindowViewModel
    {
        public MainWindowViewModel(IServiceProvider services)
            : base(services)
        {
        }

        public bool CanOpenCloudExplorer(object arg)
        {
            return true;
        }

        public void OpenCloudExplorer(object arg)
        {
            ActiveView = ViewLocatorService.NavigateTo(typeof(CloudExplorerViewModel).Name);
        }

        public bool CanOpenDeploymentDialog(object arg)
        {
            return true;
        }

        public void OpenDeploymentDialog(object arg)
        {
            ActiveView = ViewLocatorService.NavigateTo(typeof(DeploymentDialogViewModel).Name);
        }

    }
}
