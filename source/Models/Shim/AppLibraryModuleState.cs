using System.Collections.Generic;

namespace GooglePlayGamesLibrary.Models.Shim
{
    public class AppLibraryModuleState
    {
        public Dictionary<string, PcApp> PcGames { get; set; }
        public long LastModificationTimestampMs { get; set; }
        public Dictionary<string, SingleApp> AndroidGames { get; set; }
        public UserActivity UserActivity { get; set; }

    }
}
