namespace GooglePlayGamesLibrary.Models.Shim
{
    public class LaunchingDetails
    {
        public bool HasRequiresElevation { get; }
        public bool RequiresElevation { get; set; }
        public bool HasExecutableFilename { get; }
        public string ExecutableFilename { get; set; }
        public RegistryLocation LaunchRegistryLocation { get; set; }
        public bool HasArguments { get; }
        public string Arguments { get; set; }
    }
}