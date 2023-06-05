namespace GooglePlayGamesLibrary.Models.Shim
{
    public class DynastyGame
    {
        public string PackageName { get; set; }
        public bool HasPackageName { get; }
        public DynastyMetadata DynastyMetadata { get; set; }
    }
}