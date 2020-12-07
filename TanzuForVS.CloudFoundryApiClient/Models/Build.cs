using System;

namespace TanzuForVS.CloudFoundryApiClient.Models.Build
{
    public class Build
    {
        public string guid { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Created_By created_by { get; set; }
        public string state { get; set; }
        public object error { get; set; }
        public Lifecycle lifecycle { get; set; }
        public Guid package { get; set; }
        public Guid droplet { get; set; }
        public Relationships relationships { get; set; }
        public Metadata metadata { get; set; }
        public Links links { get; set; }
    }

    public class Created_By
    {
        public string guid { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class Lifecycle
    {
        public string type { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string[] buildpacks { get; set; }
        public string stack { get; set; }
    }

    public class Relationships
    {
        public AppData app { get; set; }
    }

    public class AppData
    {
        public Guid data { get; set; }
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

    public class Links
    {
        public Href self { get; set; }
        public Href app { get; set; }
    }

}

