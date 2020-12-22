namespace TanzuForVS.Services.CloudFoundry
{
    public class FileEntry
    {

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the file text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the name to be renamed.
        /// </summary>
        public string Rename { get; set; }

        /// <summary>
        /// Gets or sets the associated dependencies.
        /// </summary>
        public string Dependencies { get; set; }
    }
}
