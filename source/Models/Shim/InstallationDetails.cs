namespace GooglePlayGamesLibrary.Models.Shim
{
    public class InstallationDetails
    {
        public bool HasRequiresElevation { get; }
        public bool RequiresElevation { get; set; }
        public bool HasDownloadUrl { get; }
        public string DownloadUrl { get; set; }
        public RegistryLocation InstallRegistryLocation { get; set; }
        public InstallationSize InstallationSize { get; set; }
    }
}