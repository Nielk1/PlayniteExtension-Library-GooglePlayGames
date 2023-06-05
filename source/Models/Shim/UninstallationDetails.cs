namespace GooglePlayGamesLibrary.Models.Shim
{
    public class UninstallationDetails
    {
        public RegistryLocation UninstallRegistryLocation { get; set; }
        public bool RequiresElevation { get; set; }
        public bool HasRequiresElevation { get; }
    }
}