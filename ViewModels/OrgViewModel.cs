﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TanzuForVS.Models;

namespace TanzuForVS.ViewModels
{
    public class OrgViewModel : TreeViewItemViewModel
    {
        readonly CloudFoundryOrganization _org;
        readonly string _target;
        readonly string _token;

        public OrgViewModel(CloudFoundryOrganization org, string apiAddress, string accessToken, IServiceProvider services)
            : base(null, services)
        {
            _org = org;
            _target = apiAddress;
            _token = accessToken;

            this.DisplayText = _org.OrgName;
        }

        protected override async Task LoadChildren()
        {
            var spaceNames = await CloudFoundryService.GetSpaceNamesAsync(_target, _token, _org.OrgId);

            var updatedSpacesList = new ObservableCollection<TreeViewItemViewModel>();
            foreach (string spaceName in spaceNames)
            {
                updatedSpacesList.Add(new SpaceViewModel(new CloudFoundrySpace(spaceName), Services));
            }

            Children = updatedSpacesList;
        }
    }

}
