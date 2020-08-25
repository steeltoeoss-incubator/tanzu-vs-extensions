﻿using CloudFoundry.CloudController.V2.Client;
using CloudFoundry.UAA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanzuForVS
{
    /// <summary>
    /// Provides a method for constructing a new Cloud Foundry client.
    /// </summary>
    public class CfApiV2ClientFactory : ICfApiClientFactory
    {
        public IUAA CreateCfApiV2Client(Uri target, Uri httpProxy, bool skipSsl)
        {
            return new CloudFoundryClient(target, new System.Threading.CancellationToken(), httpProxy, skipSsl);
        }
    }
}
