namespace GooglePlayGamesLibrary.Models.Shim
{
    public class SingleApp
    {
        public UserInitiatedUninstall UserInitiatedUninstall { get; set; }
        public GameMetadata GameMetadata { get; set; }
        public StateOneofCase StateCase { get; }
        public UserInitiatedInstall UserInitiatedInstall { get; set; }
        public string PackageName { get; set; }
        public LibraryApp InstalledApp { get; set; }

        public enum StateOneofCase
        {
            None = 0,
            UserInitiatedInstall = 2,
            UserInitiatedUninstall = 3,
            InstalledApp = 4
        }
    }
}