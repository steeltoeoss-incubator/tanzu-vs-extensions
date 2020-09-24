﻿using System.Threading.Tasks;
using System;
using System.Net;

namespace TanzuForVS.CloudFoundryApiClient
{
    public interface IUaaClient
    {
        Token Token { get; }

        Task<HttpStatusCode> RequestAccessTokenAsync(Uri uaaUri, string uaaClientId, string uaaClientSecret, string cfUsername, string cfPassword);
    }
}