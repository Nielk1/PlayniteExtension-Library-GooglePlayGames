namespace GooglePlayGamesLibrary.Models.Shim
{
    public class LibraryApp
    {
        public UserPreferredAppData UserPreferredAppData { get; set; }
        public UpdateState UpdateState { get; set; }
        public Types.AspectRatioLimit AspectRatioLimit { get; set; }
        public int Category { get; set; }
        public string ApplicationId { get; set; }
        public long LastLaunchedTimestampMs { get; set; }
        public long InstalledTimestampMs { get; set; }
        public string Title { get; set; }
        public string VersionName { get; set; }
        public long VersionCode { get; set; }
        public string PackageName { get; set; }
        public Types.ScreenOrientation DefaultScreenOrientation { get; set; }
        public bool IsSystemApp { get; set; }
        public string PrimaryCpuAbi { get; set; }
        public Types.MouseInputMode MouseInputMode { get; set; }

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
}