﻿using System.Threading.Tasks;

namespace TanzuForVS.ViewModels
{
    public interface IDeploymentDialogViewModel
    {
        bool CanDeployApp(object arg);
        bool CanOpenLoginView(object arg);
        Task DeployApp(object arg);
        void OpenLoginView(object arg);
        void UpdateCfInstanceOptions();
        Task UpdateCfOrgOptions();
        Task UpdateCfSpaceOptions();
    }
}