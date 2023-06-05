namespace GooglePlayGamesLibrary.Models.Shim
{
    public class AndroidInstallSession
    {
        public Types.Finished Finished { get; set; }
        public Types.Installing Installing { get; set; }
        public Types.Downloading Downloading { get; set; }
        public Types.Pending Pending { get; set; }
        public long DownloadSizeBytes { get; set; }
        public long StartedTimestampMs { get; set; }
        public string PackageName { get; set; }
        public Types.InstallQueueStatus InstallQueueStatus { get; set; }
        public StatusOneofCase StatusCase { get; }
        public Types.Canceling Canceling { get; set; }

        public enum StatusOneofCase
        {
            None = 0,
            Pending = 4,
            Downloading = 5,
            Installing = 6,
            Finished = 7,
            Canceling = 8
        }
        public static class Types
        {
            public enum InstallQueueStatus
            {
                Unknown = 0,
                DownloadPending = 1,
                Downloading = 2,
                DownloadCancelled = 3,
                DownloadError = 4,
                Installing = 5,
                InstallError = 6,
                Installed = 7,
                Uninstalling = 8,
                Uninstalled = 9,
                UninstallError = 10,
                InstallSkipped = 11,
                Scheduled = 12,
                AwaitingInstallation = 13,
                PostDownloading = 14,
                Cancelling = 15
            }

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
                        Completed = 3,
                        FailedTryAgainLater = 4,
                        FailedPrecondition = 5,
                        FailedApiDisabled = 6,
                        FailedUnauthenticated = 7,
                        FailedInvalidAccountState = 8,
                        FailedAccountMismatch = 9,
                        FailedCallerVerification = 10,
                        FailedDocumentUnavailable = 11,
                        FailedUnableToPurchase = 12,
                        FailedAccountError = 13,
                        FailedCallingInstallService = 14,
                        FailedNullResultFromInstallService = 15,
                        FailedRequestFailed = 16,
                        FailedScheduling = 17,
                        FailedFutureException = 18,
                        FailedCatchAllException = 19,
                        FailedInvalidAndroidId = 20,
                        FailedInstallServiceNetworkingIssue = 21,
                        FailedDocumentRestricted = 22,
                        FailedAcquisitionDenied = 23,
                        FailedAcquisitionTimeout = 24,
                        FailedInstallServiceAuthIssue = 25
                    }
                }
            }
        }
    }
}