﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Tanzu.Toolkit.CloudFoundryApiClient;
using Tanzu.Toolkit.VisualStudio.Commands;
using Tanzu.Toolkit.VisualStudio.Services.CfCli;
using Tanzu.Toolkit.VisualStudio.Services.CloudFoundry;
using Tanzu.Toolkit.VisualStudio.Services.CmdProcess;
using Tanzu.Toolkit.VisualStudio.Services.Dialog;
using Tanzu.Toolkit.VisualStudio.Services.FileLocator;
using Tanzu.Toolkit.VisualStudio.Services.ViewLocator;
using Tanzu.Toolkit.VisualStudio.ViewModels;
using Tanzu.Toolkit.VisualStudio.WpfViews;
using Tanzu.Toolkit.VisualStudio.WpfViews.Services;
using Task = System.Threading.Tasks.Task;

namespace Tanzu.Toolkit.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(TanzuCloudExplorerToolWindow))]
    [ProvideToolWindow(typeof(OutputToolWindow))]
    public sealed class TanzuForVSPackage : AsyncPackage
    {
        /// <summary>
        /// TanzuForVSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9419e55b-9e82-4d87-8ee5-70871b01b7cc";

        private IServiceProvider serviceProvider;

        public TanzuForVSPackage()
        {
        }



        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await TanzuCloudExplorerCommand.InitializeAsync(this);
            await PushToCloudFoundryCommand.InitializeAsync(this, serviceProvider);
            await Tanzu.Toolkit.VisualStudio.Commands.OutputWindowCommand.InitializeAsync(this);
        }

        protected override object GetService(Type serviceType)
        {
            if (serviceProvider == null)
            {
                var collection = new ServiceCollection();
                ConfigureServices(collection);
                serviceProvider = collection.BuildServiceProvider();
            }

            var result = serviceProvider.GetService(serviceType);
            if (result != null)
            {
                return result;
            }

            return base.GetService(serviceType);
        }

        protected override WindowPane InstantiateToolWindow(Type toolWindowType)
        {
            return GetService(toolWindowType) as WindowPane;
        }

        #endregion

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICloudFoundryService, CloudFoundryService>();
            services.AddSingleton<IViewLocatorService, WpfViewLocatorService>();
            services.AddSingleton<IDialogService, WpfDialogService>();
            services.AddSingleton<ICfCliService, CfCliService>();
            services.AddSingleton<ICmdProcessService, CmdProcessService>();
            services.AddSingleton<IFileLocatorService, FileLocatorService>();

            services.AddTransient<TanzuCloudExplorerToolWindow>();
            services.AddTransient<OutputToolWindow>();

            services.AddTransient<ICloudExplorerViewModel, CloudExplorerViewModel>();
            services.AddTransient<ICloudExplorerView, CloudExplorerView>();

            services.AddTransient<IDeploymentDialogViewModel, DeploymentDialogViewModel>();
            services.AddTransient<IDeploymentDialogView, DeploymentDialogView>();

            services.AddTransient<IAddCloudDialogViewModel, AddCloudDialogViewModel>();
            services.AddTransient<IAddCloudDialogView, AddCloudDialogView>();

            services.AddSingleton<IOutputViewModel, OutputViewModel>();
            services.AddSingleton<IOutputView, OutputView>();

            HttpClient concreteHttpClient = new HttpClient();
            IUaaClient concreteUaaClient = new UaaClient(concreteHttpClient);
            services.AddSingleton(_ => concreteUaaClient);
            services.AddSingleton<ICfApiClient>(_ => new CfApiClient(concreteUaaClient, concreteHttpClient));
        }
    }
}