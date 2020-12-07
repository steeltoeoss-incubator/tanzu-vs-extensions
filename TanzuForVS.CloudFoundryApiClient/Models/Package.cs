using System;

namespace TanzuForVS.CloudFoundryApiClient.Models.Package
{

    public class Package
    {
        public string guid { get; set; }
        public string type { get; set; }
        public Data data { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Relationships relationships { get; set; }
        public Links links { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Data
    {
        public Checksum checksum { get; set; }
        public object error { get; set; }
    }

    public class Checksum
    {
        public string type { get; set; }
        public object value { get; set; }
    }

    public class Relationships
    {
        public AppData app { get; set; }
    }

    public class AppData
    {
        public Guid data { get; set; }
    }

    public class Links
    {
        public Href self { get; set; }
        public Upload upload { get; set; }
        public Download download { get; set; }
        public Href app { get; set; }
    }

    public class Upload
    {
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Download
    {
        public string href { get; set; }
        public string method { get; set; }
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

}
