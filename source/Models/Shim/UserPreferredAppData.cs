namespace GooglePlayGamesLibrary.Models.Shim
{
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
}