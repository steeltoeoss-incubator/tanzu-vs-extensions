using System.Collections.Generic;
using System.Threading.Tasks;
using TanzuForVS.CloudFoundryApiClient.Models.AppsResponse;
using TanzuForVS.CloudFoundryApiClient.Models.Build;
using TanzuForVS.CloudFoundryApiClient.Models.OrgsResponse;
using TanzuForVS.CloudFoundryApiClient.Models.Package;
using TanzuForVS.CloudFoundryApiClient.Models.Route;
using TanzuForVS.CloudFoundryApiClient.Models.SpacesResponse;

namespace TanzuForVS.CloudFoundryApiClient
{
    public interface ICfApiClient
    {
        string AccessToken { get; }

        Task<string> LoginAsync(string cfTarget, string cfUsername, string cfPassword);
        Task<List<Org>> ListOrgs(string cfTarget, string accessToken);
        Task<List<Space>> ListSpaces(string cfTarget, string accessToken);
        Task<List<Space>> ListSpacesForOrg(string cfTarget, string accessToken, string orgGuid);
        Task<List<App>> ListAppsForSpace(string cfTarget, string accessToken, string spaceGuid);
        Task<bool> StopAppWithGuid(string cfTarget, string accessToken, string appGuid);
        Task<bool> StartAppWithGuid(string cfTarget, string accessToken, string appGuid);
        Task<bool> DeleteAppWithGuid(string cfTarget, string accessToken, string appGuid);
        Task<App> CreateApp(string cfTarget, string accessToken, string appName, string spaceGuid);
        Task<Package> CreatePackage(string cfTarget, string accessToken, string appGuid);
        Task<Package> GetPackage(string cfTarget, string accessToken, string packageGuid);
        Task<bool> DeletePackage(string cfTarget, string accessToken, string packageGuid);
        Task<bool> SetDropletForApp(string cfTarget, string accessToken, string appGuid, string dropletGuid);
        Task<Build> CreateBuild(string cfTarget, string accessToken, string pckgGuid);
        Task<Build> GetBuild(string cfTarget, string accessToken, string buildGuid);
        Task<Route> CreateRoute(string cfTarget, string accessToken, string spaceGuid, string domainGuid, string host, string path, int port);
        Task<bool> DeleteRoute(string cfTarget, string accessToken, string routeGuid);
        Task<bool> AddDestinationToRoute(string cfTarget, string accessToken, string routeGuid, string appGuid, string processType, int port);
        Task<bool> UploadBits(string cfTarget, string accessToken, string packageGuid, string fileName, byte[] fileBytes);
    }
}
