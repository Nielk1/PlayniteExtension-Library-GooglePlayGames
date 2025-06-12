using System;
using System.Collections.Generic;

namespace GooglePlayGamesLibrary.Models.Shim
{
    public class Shim
    {
        public bool success { get; set; }
        public string error { get; set; }
        public AppLibraryModuleState data { get; set; }
    }
    public class AppLibraryModuleState
    {
        //AndroidGamesDeprecated
        //AppsDeprecated
        public long LastModificationTimestampMs { get; set; }
        public Dictionary<string, PcApp> PcGames { get; set; }
        //PcAppsDeprecated
        public UserActivity UserActivity { get; set; }
        public Dictionary<string, AndroidGameLibrary> AndroidGameLibraries { get; set; }

    }
    public class AndroidGameLibrary
    {
        public LibraryId LibraryId { get; set; }
        public Dictionary<string, SingleApp> AndroidGames { get; set; }
        public UserActivity UserActivity { get; set; }
        public long LastModificationTimestampMs { get; set; }
    }
    public class LibraryId
    {
        public string Token { get; set; }
    }
    public class PcApp
    {
        public string PackageName { get; set; }
        public UserInitiatedUninstall UserInitiatedUninstall { get; set; }
        public PcLibraryGame InstalledGame { get; set; }
        public UserInitiatedInstall UserInitiatedInstall { get; set; }
        public StateOneofCase StateCase { get; set; }
        public enum StateOneofCase
        {
            None = 0,
            UserInitiatedInstall = 2,
            InstalledGame = 3,
            UserInitiatedUninstall = 4
        }
    }
    public class PcLibraryGame
    {
        public DynastyGame GameData { get; set; }
        public string Title { get; set; }
        public long InstalledTimestampMs { get; set; }
        public long LastLaunchedTimestampMs { get; set; }
        public string PlayExeLocation { get; set; }
    }
    public class DynastyGame
    {
        public string PackageName { get; set; }
        public bool HasPackageName { get; set; }
        public DynastyMetadata DynastyMetadata { get; set; }
    }
    public class DynastyMetadata
    {
        public InstallationDetails InstallationDetails { get; set; }
        public UninstallationDetails UninstallationDetails { get; set; }
        public LaunchingDetails LaunchingDetails { get; set; }
        public WpkDetails WpkDetails { get; set; }
    }
    public class InstallationDetails
    {
        public string DownloadUrl { get; set; }
        public bool HasDownloadUrl { get; set; }
        public bool RequiresElevation { get; set; }
        public bool HasRequiresElevation { get; set; }
        public InstallationSize InstallationSize { get; set; }
        public RegistryLocation InstallRegistryLocation { get; set; }
    }
    public class InstallationSize
    {
        public ulong SizeInBytes { get; set; }
        public bool HasSizeInBytes { get; set; }
    }
    public class RegistryLocation
    {
        public string Path { get; set; }
        public bool HasPath { get; set; }
        public string ValueName { get; set; }
        public bool HasValueName { get; set; }
    }
    public class UninstallationDetails
    {
        public RegistryLocation UninstallRegistryLocation { get; set; }
        public bool RequiresElevation { get; set; }
        public bool HasRequiresElevation { get; set; }
    }
    public class LaunchingDetails
    {
        public RegistryLocation LaunchRegistryLocation { get; set; }
        public string ExecutableFilename { get; set; }
        public bool HasExecutableFilename { get; set; }
        public bool RequiresElevation { get; set; }
        public bool HasRequiresElevation { get; set; }
        public string Arguments { get; set; }
        public bool HasArguments { get; set; }
    }
    public class WpkDetails
    {
        public string WpkDownloadUrl { get; set; }
        public bool HasWpkDownloadUrl { get; set; }
        //public WpkInstallerDownloadConfig WpkInstallerDownloadConfig { get; set; }
    }
    public class UserActivity
    {
        public long LastGameLaunchTimestampMs { get; set; }
    }
    public class SingleApp
    {
        public string PackageName { get; set; }
        public UserInitiatedInstall UserInitiatedInstall { get; set; }
        public UserInitiatedUninstall UserInitiatedUninstall { get; set; }
        public LibraryApp InstalledApp { get; set; }
        public GameMetadata GameMetadata { get; set; }
        public StateOneofCase StateCase { get; set; }

        public enum StateOneofCase
        {
            None = 0,
            UserInitiatedInstall = 2,
            UserInitiatedUninstall = 3,
            InstalledApp = 4
        }
    }
    public class LibraryApp
    {
        public string PackageName { get; set; }
        public long VersionCode { get; set; }
        public string VersionName { get; set; }
        public string Title { get; set; }
        public long InstalledTimestampMs { get; set; }
        public long LastLaunchedTimestampMs { get; set; }
        public string ApplicationId { get; set; }
        public int Category { get; set; }
        public UpdateState UpdateState { get; set; }
        public UserPreferredAppData UserPreferredAppData { get; set; }
        public Types.ScreenOrientation DefaultScreenOrientation { get; set; }
        public Types.AspectRatioLimit AspectRatioLimit { get; set; }
        public bool IsSystemApp { get; set; }
        public Types.MouseInputMode MouseInputMode { get; set; }
        public string PrimaryCpuAbi { get; set; }
        public ulong InstallationSizeBytes { get; set; }
        public static class Types
        {
            public class AspectRatioLimit
            {
                public float Min { get; set; }
                public float Max { get; set; }
            }
            public enum ScreenOrientation
            {
                Unspecified = 0,
                Landscape = 1,
                Portrait = 2,
                User = 3,
                Behind = 4,
                Sensor = 5,
                Nosensor = 6,
                SensorLandscape = 7,
                SensorPortrait = 8,
                ReverseLandscape = 9,
                ReversePortrait = 10,
                FullSensor = 11,
                UserLandscape = 12,
                UserPortrait = 13,
                FullUser = 14,
                Locked = 15
            }
            public enum MouseInputMode
            {
                ModeUnknown = 0,
                ModeTouchscreen = 1,
                ModeRelative = 2,
                ModeKiwi = 3
            }
        }
    }
    public class GameMetadata
    {
        public string Title { get; set; }
        public ImageData BackgroundImage { get; set; }
        public ImageData AppIcon { get; set; }
        public ImageData Logo { get; set; }
    }
    public class ImageData
    {
        public string Url { get; set; }
    }
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
    public class UserPreferredAppData
    {
        public Types.Mode WindowMode { get; set; }
        public static class Types
        {
            public enum Mode
            {
                UnknownMode = 0,
                Fullscreen = 1,
                Windowed = 2
            }
        }
    }
    public class UninstallGameRequest
    {
        public string PackageName { get; set; }
    }
    public class UserInitiatedInstall
    {
        public Types.Status Status { get; set; }
        public bool NeedsRetry { get; set; }
        public AndroidInstallSession AndroidInstallSession { get; set; }
        public PcGameInstallSession PcInstallSession { get; set; }
        public string AttemptId { get; set; }
        public InstallSessionOneofCase InstallSessionCase { get; set; }
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
    public class PcGameInstallSession
    {
        public long StartedTimestampMs { get; set; }
        public long DownloadSizeBytes { get; set; }
        public Types.Installing Installing { get; set; }
        public Types.Downloading Downloading { get; set; }
        public Types.Finished Finished { get; set; }
        public Types.Canceling Canceling { get; set; }
        public StatusOneofCase StatusCase { get; set; }
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
    public class InstallGameRequest
    {
        public bool AttemptFreePurchase { get; set; }
        public string AcquisitionToken { get; set; }
        public string AccountEmail { get; set; }
        public string PackageName { get; set; }
        public string DownloadUrl { get; set; }
        public bool DebugRequest { get; set; }
    }
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
        public StatusOneofCase StatusCase { get; set; }
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
    public class AndroidUninstallSession
    {
        public string PackageName { get; set; }
        public StatusOneofCase StatusCase { get; set; }
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
