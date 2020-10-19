﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TanzuForVS.CloudFoundryApiClient.Models.OrgsResponse;
using TanzuForVS.CloudFoundryApiClient.Models.SpacesResponse;

namespace TanzuForVS.CloudFoundryApiClient
{
    public interface ICfApiClient
    {
        string AccessToken { get; }

        Task<string> LoginAsync(string cfTarget, string cfUsername, string cfPassword);

        Task<List<Org>> ListOrgs(string cfTarget, string accessToken);

        Task<List<Space>> ListSpaces(string cfTarget, string accessToken);
    }
}
