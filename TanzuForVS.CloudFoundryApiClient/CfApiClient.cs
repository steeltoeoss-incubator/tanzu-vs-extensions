using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TanzuForVS.CloudFoundryApiClient.Models;
using TanzuForVS.CloudFoundryApiClient.Models.AppsResponse;
using TanzuForVS.CloudFoundryApiClient.Models.BasicInfoResponse;
using TanzuForVS.CloudFoundryApiClient.Models.Build;
using TanzuForVS.CloudFoundryApiClient.Models.OrgsResponse;
using TanzuForVS.CloudFoundryApiClient.Models.Package;
using TanzuForVS.CloudFoundryApiClient.Models.Route;
using TanzuForVS.CloudFoundryApiClient.Models.SpacesResponse;

namespace TanzuForVS.CloudFoundryApiClient
{
    public class CfApiClient : ICfApiClient
    {
        public string AccessToken { get; private set; }

        internal static readonly string listOrgsPath = "/v3/organizations";
        internal static readonly string listSpacesPath = "/v3/spaces";
        internal static readonly string listAppsPath = "/v3/apps";
        internal static readonly string deleteAppsPath = "/v3/apps";
        internal static readonly string createAppsPath = "/v3/apps";
        internal static readonly string createPackagesPath = "/v3/packages";
        internal static readonly string getPackagePath = "/v3/packages/:guid";
        internal static readonly string uploadBitsPath = "v3/packages/:guid/upload";
        internal static readonly string createBuildsPath = "/v3/builds";
        internal static readonly string createRoutesPath = "/v3/routes";
        internal static readonly string getBuildPath = "/v3/builds";
        internal static readonly string setDropletForAppPath = "/v3/apps/:guid/relationships/current_droplet";
        internal static readonly string addDestinationToRoutePath = "/v3/routes/:guid/destinations";

        public static readonly string defaultAuthClientId = "cf";
        public static readonly string defaultAuthClientSecret = "";
        public static readonly string AuthServerLookupFailureMessage = "Unable to locate authentication server";
        public static readonly string InvalidTargetUriMessage = "Invalid target URI";

        private static IUaaClient _uaaClient;
        private static HttpClient _httpClient;

        public CfApiClient(IUaaClient uaaClient, HttpClient httpClient)
        {
            _uaaClient = uaaClient;
            _httpClient = httpClient;
            AccessToken = null;
        }

        /// <summary>
        /// LoginAsync contacts the auth server for the specified cfTarget
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="cfUsername"></param>
        /// <param name="cfPassword"></param>
        /// <returns>
        /// Access Token from the auth server as a string, 
        /// or null if auth request responded with a status code other than 200
        /// </returns>
        public async Task<string> LoginAsync(string cfTarget, string cfUsername, string cfPassword)
        {
            validateUriStringOrThrow(cfTarget, InvalidTargetUriMessage);
            var authServerUri = await GetAuthServerUriFromCfTarget(cfTarget);

            var result = await _uaaClient.RequestAccessTokenAsync(authServerUri,
                                                                  defaultAuthClientId,
                                                                  defaultAuthClientSecret,
                                                                  cfUsername,
                                                                  cfPassword);

            if (result == HttpStatusCode.OK)
            {
                AccessToken = _uaaClient.Token.access_token;
                return AccessToken;
            }
            else
            {
                return null;
            }
        }

        private async Task<Uri> GetAuthServerUriFromCfTarget(string cfTargetString)
        {
            try
            {
                Uri authServerUri = null;

                var uri = new UriBuilder(cfTargetString)
                {
                    Path = "/",
                    Port = -1
                };

                var request = new HttpRequestMessage(HttpMethod.Get, uri.ToString());

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var basicInfo = JsonConvert.DeserializeObject<BasicInfoResponse>(content);
                    authServerUri = new Uri(basicInfo.links.login.href);
                }

                return authServerUri;
            }
            catch (Exception e)
            {
                e.Data.Add("MessageToDisplay", AuthServerLookupFailureMessage);
                throw;
            }
        }

        private Uri validateUriStringOrThrow(string uriString, string errorMessage)
        {
            Uri uriResult;
            Uri.TryCreate(uriString, UriKind.Absolute, out uriResult);

            if (uriResult == null) throw new Exception(errorMessage);

            return uriResult;
        }

        public async Task<List<Org>> ListOrgs(string cfTarget, string accessToken)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = listOrgsPath
                };

                Href firstPageHref = new Href() { href = uri.ToString() };

                List<Org> visibleOrgs = await GetRemainingPagesForType<Org>(firstPageHref, accessToken, new List<Org>());

                return visibleOrgs;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }

        }

        public async Task<List<Space>> ListSpaces(string cfTarget, string accessToken)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = listSpacesPath
                };

                Href firstPageHref = new Href() { href = uri.ToString() };

                return await GetRemainingPagesForType<Space>(firstPageHref, accessToken, new List<Space>());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public async Task<List<Space>> ListSpacesForOrg(string cfTarget, string accessToken, string orgGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = listSpacesPath,
                    Query = $"organization_guids={orgGuid}"
                };

                Href firstPageHref = new Href() { href = uri.ToString() };

                return await GetRemainingPagesForType<Space>(firstPageHref, accessToken, new List<Space>());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        public async Task<List<App>> ListAppsForSpace(string cfTarget, string accessToken, string spaceGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = listAppsPath,
                    Query = $"space_guids={spaceGuid}"
                };

                Href firstPageHref = new Href() { href = uri.ToString() };

                return await GetRemainingPagesForType<App>(firstPageHref, accessToken, new List<App>());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        private async Task<List<ResourceType>> GetRemainingPagesForType<ResourceType>(Href pageAddress, string accessToken, List<ResourceType> resultsSoFar)
        {
            if (pageAddress == null) return resultsSoFar;

            var request = new HttpRequestMessage(HttpMethod.Get, pageAddress.href);
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) throw new Exception($"Response from GET `{pageAddress}` was {response.StatusCode}");

            string resultContent = await response.Content.ReadAsStringAsync();

            Href nextPageHref;

            if (typeof(ResourceType) == typeof(Org))
            {
                var results = JsonConvert.DeserializeObject<OrgsResponse>(resultContent);
                resultsSoFar.AddRange((IEnumerable<ResourceType>)results.Orgs.ToList());

                nextPageHref = results.pagination.next;
            }
            else if (typeof(ResourceType) == typeof(Space))
            {
                var results = JsonConvert.DeserializeObject<SpacesResponse>(resultContent);
                resultsSoFar.AddRange((IEnumerable<ResourceType>)results.Spaces.ToList());

                nextPageHref = results.pagination.next;
            }
            else if (typeof(ResourceType) == typeof(App))
            {
                var results = JsonConvert.DeserializeObject<AppsResponse>(resultContent);
                resultsSoFar.AddRange((IEnumerable<ResourceType>)results.Apps.ToList());

                nextPageHref = results.pagination.next;
            }
            else
            {
                throw new Exception($"ResourceType unknown: {typeof(ResourceType).Name}");
            }

            return await GetRemainingPagesForType<ResourceType>(nextPageHref, accessToken, resultsSoFar);
        }

        public async Task<bool> StopAppWithGuid(string cfTarget, string accessToken, string appGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var stopAppPath = listAppsPath + $"/{appGuid}/actions/stop";

                var uri = new UriBuilder(cfTarget)
                {
                    Path = stopAppPath
                };

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) throw new Exception($"Response from POST `{stopAppPath}` was {response.StatusCode}");

                string resultContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<App>(resultContent);

                if (result.state == "STOPPED") return true;
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> StartAppWithGuid(string cfTarget, string accessToken, string appGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var startAppPath = listAppsPath + $"/{appGuid}/actions/start";

                var uri = new UriBuilder(cfTarget)
                {
                    Path = startAppPath
                };

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) throw new Exception($"Response from POST `{startAppPath}` was {response.StatusCode}");

                string resultContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<App>(resultContent);

                if (result.state == "STARTED") return true;
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Delete an App: DELETE /v3/apps/:guid
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="appGuid"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAppWithGuid(string cfTarget, string accessToken, string appGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var deleteAppPath = deleteAppsPath + $"/{appGuid}";

                var uri = new UriBuilder(cfTarget)
                {
                    Path = deleteAppPath
                };

                var request = new HttpRequestMessage(HttpMethod.Delete, uri.ToString());
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Accepted) throw new Exception($"Response from DELETE `{deleteAppPath}` was {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Accepted) return true;
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Create an App: POST /v3/apps
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<App> CreateApp(string cfTarget, string accessToken)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var createAppPath = createAppsPath;

                var uri = new UriBuilder(cfTarget)
                {
                    Path = createAppPath
                };

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Created) throw new Exception($"Response from POST `{createAppPath}` was {response.StatusCode}");

                string responseContent = await response.Content.ReadAsStringAsync();
                var app = JsonConvert.DeserializeObject<App>(responseContent);

                return app;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Create a Package: POST /v3/packages
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="appGuid"></param>
        /// <returns></returns>
        public async Task<Package> CreatePackage(string cfTarget, string accessToken, string appGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = createPackagesPath
                };

                string json = JsonConvert.SerializeObject(new
                {
                    type = "bits",
                    relationships = new
                    {
                        app = new
                        {
                            data = new
                            {
                                guid = appGuid
                            }
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())
                {
                    Content = new StringContent(json, Encoding.UTF8, "applicaiton/json")
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Created) throw new Exception($"Response from POST `{createPackagesPath}` was {response.StatusCode}");

                string responseContent = await response.Content.ReadAsStringAsync();
                var package = JsonConvert.DeserializeObject<Package>(responseContent);

                return package;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Get a Package by guid: GET /v3/packages/:guid
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="packageGuid"></param>
        /// <returns></returns>
        public async Task<Package> GetPackage(string cfTarget, string accessToken, string packageGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = getPackagePath.Replace(":guid", packageGuid)
                };

                var request = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"Response from POST `{getPackagePath}` was {response.StatusCode}");

                string responseContent = await response.Content.ReadAsStringAsync();
                var package = JsonConvert.DeserializeObject<Package>(responseContent);

                return package;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        // TODO: finish this method (needs tests too)
        /// <summary>
        /// Upload package bits: POST /v3/packages/:guid/upload
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="pckgBits"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        //public async Task<bool> UploadBits(string cfTarget, string accessToken, string pckgBits, string path)
        //{
        //    try
        //    {
        //        // trust any certificate
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        ServicePointManager.ServerCertificateValidationCallback +=
        //            (sender, cert, chain, sslPolicyErrors) => { return true; };

        //        var uplaodBitPath = uploadBitsPath + $"/{pckgBits}" + $"/{path}";

        //        var uri = new UriBuilder(cfTarget)
        //        {
        //            Path = uplaodBitPath
        //        };

        //        var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString());
        //        request.Headers.Add("Authorization", "Bearer " + accessToken);

        //        var response = await _httpClient.SendAsync(request);
        //        if (response.StatusCode != HttpStatusCode.Accepted) throw new Exception($"Response from POST `{uplaodBitPath}` was {response.StatusCode}");

        //        if (response.StatusCode == HttpStatusCode.Accepted) return true;
        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        System.Diagnostics.Debug.WriteLine(e);
        //        return false;
        //    }
        //}

        /// <summary>
        /// Create a new build: POST /v3/builds
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="pckgGuid"></param>
        /// <returns></returns>
        public async Task<Build> CreateBuild(string cfTarget, string accessToken, string pckgGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var createBuildPath = createBuildsPath;

                var uri = new UriBuilder(cfTarget)
                {
                    Path = createBuildPath
                };

                string json = JsonConvert.SerializeObject(new
                {
                    package = new
                    {
                        guid = pckgGuid
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())
                {
                    Content = new StringContent(json, Encoding.UTF8, "applicaiton/json")
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Created) throw new Exception($"Response from POST `{createBuildPath}` was {response.StatusCode}");

                string responseContent = await response.Content.ReadAsStringAsync();
                var build = JsonConvert.DeserializeObject<Build>(responseContent);

                return build;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Set the current droplet for an app: PATCH /v3/apps/:guid/relationships/current_droplet
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="appGuid"></param>
        /// <param name="dropletGuid"></param>
        /// <returns></returns>
        public async Task<bool> SetDropletForApp(string cfTarget, string accessToken, string appGuid, string dropletGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var assignDropletPath = setDropletForAppPath.Replace(":guid", appGuid);

                var uri = new UriBuilder(cfTarget)
                {
                    Path = assignDropletPath
                };

                string json = JsonConvert.SerializeObject(new
                {
                    data = new
                    {
                        guid = dropletGuid
                    }
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri.ToString())
                {
                    Content = new StringContent(json, Encoding.UTF8, "applicaiton/json")
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"Response from PATCH `{assignDropletPath}` was {response.StatusCode}");

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Get info about a Build: GET /v3/builds/:guid
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="buildGuid"></param>
        /// <returns></returns>
        public async Task<Build> GetBuild(string cfTarget, string accessToken, string buildGuid)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var uri = new UriBuilder(cfTarget)
                {
                    Path = getBuildPath + $"/{buildGuid}"
                };

                var request = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"Response from GET `{getBuildPath}/{buildGuid}` was {response.StatusCode}");

                string resultContent = await response.Content.ReadAsStringAsync();
                var build = JsonConvert.DeserializeObject<Build>(resultContent);

                return build;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Create a Route: POST /v3/routes
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="spaceGuid"></param>
        /// <param name="domainGuid"></param>
        /// <param name="host"></param>
        /// <param name="path"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<Route> CreateRoute(string cfTarget, string accessToken, string spaceGuid, string domainGuid, string host, string path, int port)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };


                var uri = new UriBuilder(cfTarget)
                {
                    Path = createRoutesPath
                };

                string json = JsonConvert.SerializeObject(new
                {
                    host = host,
                    path = path,
                    port = port,
                    relationships = new
                    {
                        domain = new
                        {
                            data = new { guid = domainGuid }
                        },
                        space = new
                        {
                            data = new { guid = spaceGuid }
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())
                {
                    Content = new StringContent(json, Encoding.UTF8, "applicaiton/json")
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Created) throw new Exception($"Response from POST `{createRoutesPath}` was {response.StatusCode}");

                string responseContent = await response.Content.ReadAsStringAsync();
                var route = JsonConvert.DeserializeObject<Route>(responseContent);

                return route;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Creating a Destination to link a Route to an App: POST /v3/routes/:guid/destinations
        /// This endpoint *inserts* a new Destination for a Route & cannot add weighted Destinations.
        /// </summary>
        /// <param name="cfTarget"></param>
        /// <param name="accessToken"></param>
        /// <param name="routeGuid"></param>
        /// <param name="appGuid"></param>
        /// <param name="processType"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> AddDestinationToRoute(string cfTarget, string accessToken, string routeGuid, string appGuid, string processType, int port)
        {
            try
            {
                // trust any certificate
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => { return true; };

                var addDestinationPath = addDestinationToRoutePath.Replace(":guid", routeGuid);

                var uri = new UriBuilder(cfTarget)
                {
                    Path = addDestinationPath
                };

                string json = JsonConvert.SerializeObject(new
                {
                    destinations = new[]
                    {
                        new
                        {
                            app = new
                            {
                                guid = appGuid,
                                process = new
                                {
                                    type = processType
                                }
                            },
                            port = port
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())
                {
                    Content = new StringContent(json, Encoding.UTF8, "applicaiton/json")
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK) throw new Exception($"Response from POST `{addDestinationPath}` was {response.StatusCode}");

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }
    }
}
