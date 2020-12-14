using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using TanzuForVS.Models;
using TanzuForVS.Services.CloudFoundry;
using TanzuForVS.Services.Dialog;
using TanzuForVS.Services.Locator;

namespace TanzuForVS.ViewModels
{
    public abstract class ViewModelTestSupport
    {
        protected IServiceProvider services;

        protected Mock<ICloudFoundryService> mockCloudFoundryService;
        protected Mock<IDialogService> mockDialogService;
        protected Mock<IViewLocatorService> mockViewLocatorService;

        protected ViewModelTestSupport()
        {
            var services = new ServiceCollection();
            mockCloudFoundryService = new Mock<ICloudFoundryService>();
            mockDialogService = new Mock<IDialogService>();
            mockViewLocatorService = new Mock<IViewLocatorService>();

            services.AddSingleton<ICloudFoundryService>(mockCloudFoundryService.Object);
            services.AddSingleton<IDialogService>(mockDialogService.Object);
            services.AddSingleton<IViewLocatorService>(mockViewLocatorService.Object);
            this.services = services.BuildServiceProvider();
        }

        protected static readonly List<CloudFoundryInstance> _fakeCloudFoundryInstances = new List<CloudFoundryInstance>
        {
            new CloudFoundryInstance("fakeCf0", "fakeApiAddress0", "fakeToken0"),
            new CloudFoundryInstance("fakeCf1", "fakeApiAddress1", "fakeToken1")
        };
        protected static readonly List<CloudFoundryOrganization> _fakeCloudFoundryOrganizations = new List<CloudFoundryOrganization>
        {
            new CloudFoundryOrganization("fakeOrg0.0", "fakeOrgGuid0.0", _fakeCloudFoundryInstances[0]),
            new CloudFoundryOrganization("fakeOrg0.1", "fakeOrgGuid0.1", _fakeCloudFoundryInstances[0]),
            new CloudFoundryOrganization("fakeOrg0.2", "fakeOrgGuid0.2", _fakeCloudFoundryInstances[0]),

            new CloudFoundryOrganization("fakeOrg1.0", "fakeOrgGuid1.0", _fakeCloudFoundryInstances[1]),
            new CloudFoundryOrganization("fakeOrg1.1", "fakeOrgGuid1.1", _fakeCloudFoundryInstances[1])
        };
        protected static readonly List<CloudFoundrySpace> _fakeCloudFoundrySpaces = new List<CloudFoundrySpace>
        {
            // only the first org has spaces (for now)
            new CloudFoundrySpace("fakeSpace0.0.0", "fakeSpaceGuid0.0.0", _fakeCloudFoundryOrganizations[0]),
            new CloudFoundrySpace("fakeSpace0.0.1", "fakeSpaceGuid0.0.1", _fakeCloudFoundryOrganizations[0]),
            new CloudFoundrySpace("fakeSpace0.0.2", "fakeSpaceGuid0.0.2", _fakeCloudFoundryOrganizations[0])
        };

        protected readonly Dictionary<string, CloudFoundryInstance> _fakeCfDict = new Dictionary<string, CloudFoundryInstance>
        {
            {"fakeCf1",  _fakeCloudFoundryInstances[0]},
            {"fakeCf2",  _fakeCloudFoundryInstances[1]}
        };
    }
}
