namespace GooglePlayGamesLibrary.Models.Shim
{
    public class PcGameInstallSession
    {
        public long StartedTimestampMs { get; set; }
        public long DownloadSizeBytes { get; set; }
        public Types.Installing Installing { get; set; }
        public Types.Downloading Downloading { get; set; }
        public Types.Finished Finished { get; set; }
        public Types.Canceling Canceling { get; set; }
        public StatusOneofCase StatusCase { get; }
        public Types.Pending Pending { get; set; }

        public enum StatusOneofCase
        {
            None = 0,
            Pending = 3,
            Downloading = 4,
            Installing = 5,
            Finished = 6,
            Canceling = 7
        }

        public static class Types
        {
            public class Pending
            {
            }
            public class Downloading
            {
                public int ProgressPercentage { get; set; }
                public long FinishedBytes { get; set; }
                public long TotalBytes { get; set; }
            }
            public class Installing
            {
            }
            public class Canceling
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
                        Failed = 1,
                        Completed = 2,
                        DownloadCanceled = 3
                    }
                }
            }
        }
    }
}