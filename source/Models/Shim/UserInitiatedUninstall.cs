namespace GooglePlayGamesLibrary.Models.Shim
{
    public class UserInitiatedUninstall
    {
        public LibraryApp LibraryApp { get; set; }
        public UninstallGameRequest Request { get; set; }
        public PcLibraryGame PcLibraryGame { get; set; }
        public Types.Status Status { get; set; }
        public AndroidUninstallSession AndroidUninstallSession { get; set; }

        public static class Types
        {
            public enum Status
            {
                Unknown = 0,
                UninstallRequested = 1,
                Uninstalling = 2,
                Uninstalled = 3
            }
        }
    }
}