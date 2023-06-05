namespace GooglePlayGamesLibrary.Models.Shim
{
    public class PcApp
    {
        public string PackageName { get; set; }
        public UserInitiatedUninstall UserInitiatedUninstall { get; set; }
        public StateOneofCase StateCase { get; }
        public PcLibraryGame InstalledGame { get; set; }
        public UserInitiatedInstall UserInitiatedInstall { get; set; }

        public enum StateOneofCase
        {
            None = 0,
            UserInitiatedInstall = 2,
            InstalledGame = 3,
            UserInitiatedUninstall = 4
        }
    }
}
