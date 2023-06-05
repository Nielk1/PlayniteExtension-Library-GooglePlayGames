namespace GooglePlayGamesLibrary.Models.Shim
{
    public class RegistryLocation
    {
        public bool HasPath { get; }
        public string Path { get; set; }
        public string ValueName { get; set; }
        public bool HasValueName { get; }
    }
}