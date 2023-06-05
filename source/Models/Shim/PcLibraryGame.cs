namespace GooglePlayGamesLibrary.Models.Shim
{
    public class PcLibraryGame
    {
        public long InstalledTimestampMs { get; set; }
        public string Title { get; set; }
        public DynastyGame GameData { get; set; }
        public string PlayExeLocation { get; set; }
        public long LastLaunchedTimestampMs { get; set; }
    }
}