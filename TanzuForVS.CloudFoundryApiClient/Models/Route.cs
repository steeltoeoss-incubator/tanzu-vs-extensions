using System;

namespace TanzuForVS.CloudFoundryApiClient.Models.Route
{

    public class Route
    {
        public string guid { get; set; }
        public string protocol { get; set; }
        public int port { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string host { get; set; }
        public string path { get; set; }
        public string url { get; set; }
        public Destination[] destinations { get; set; }
        public Metadata metadata { get; set; }
        public Relationships relationships { get; set; }
        public Links links { get; set; }
    }

    public class Metadata
    {
        public Labels labels { get; set; }
        public Annotations annotations { get; set; }
    }

    public class Labels
    {
    }

    public class Annotations
    {
    }

    public class Relationships
    {
        public SpaceData space { get; set; }
        public Domain domain { get; set; }
    }

    public class SpaceData
    {
        public Guid data { get; set; }
    }

    public class Domain
    {
        public Guid data { get; set; }
    }

    public class Links
    {
        public Href self { get; set; }
        public Href space { get; set; }
        public Href domain { get; set; }
        public Href destinations { get; set; }
    }

    public class Destination
    {
        public string guid { get; set; }
        public AppData app { get; set; }
        public object weight { get; set; }
        public int port { get; set; }
    }

    public class AppData
    {
        public string guid { get; set; }
        public Process process { get; set; }
    }

    public class Process
    {
        public string type { get; set; }
    }

}
