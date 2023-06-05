namespace GooglePlayGamesLibrary.Models.Shim
{
    public class UserInitiatedInstall
    {
        public Types.Status Status { get; set; }
        public bool NeedsRetry { get; set; }
        public AndroidInstallSession AndroidInstallSession { get; set; }
        public PcGameInstallSession PcInstallSession { get; set; }
        public string AttemptId { get; set; }
        public InstallSessionOneofCase InstallSessionCase { get; }
        public InstallGameRequest Request { get; set; }


        public enum InstallSessionOneofCase
        {
            None = 0,
            AndroidInstallSession = 3,
            PcInstallSession = 6
        }

        public static class Types
        {
            public enum Status
            {
                Unknown = 0,
                InstallRequested = 1,
                Installing = 2,
                Installed = 3,
                Failed = 4,
                Canceled = 5,
                Canceling = 6
            }
        }
    }
}