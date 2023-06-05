namespace GooglePlayGamesLibrary.Models.Shim
{
    public class UpdateState
    {
        public AndroidInstallSession UpdateSession { get; set; }
        public Types.UpdateStatus Status { get; set; }
        public bool UserRequested { get; set; }

        public static class Types
        {
            public enum UpdateStatus
            {
                UnknownUpdateStatus = 0,
                UpdateRequested = 1,
                Updating = 2
            }
        }
    }
}