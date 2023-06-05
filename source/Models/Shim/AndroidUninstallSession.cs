namespace GooglePlayGamesLibrary.Models.Shim
{
    public class AndroidUninstallSession
    {
        public string PackageName { get; set; }
        public StatusOneofCase StatusCase { get; }
        public Types.Finished Finished { get; set; }
        public Types.Uninstalling Uninstalling { get; set; }

        public enum StatusOneofCase
        {
            None = 0,
            Uninstalling = 2,
            Finished = 3
        }

        public static class Types
        {
            public class Uninstalling
            {
            }
            public class Finished
            {
                public Types.Result Result { get; set; }
                public long TimestampMs { get; set; }
                public static class Types
                {
                    public enum Result
                    {
                        Unknown = 0,
                        Completed = 1,
                        Failed = 2,
                        FailedUnavailable = 3,
                        FailedRefused = 4
                    }
                }
            }
        }
    }
}