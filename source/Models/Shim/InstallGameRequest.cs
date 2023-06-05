namespace GooglePlayGamesLibrary.Models.Shim
{
    public class InstallGameRequest
    {
        public bool AttemptFreePurchase { get; set; }
        public string AcquisitionToken { get; set; }
        public string AccountEmail { get; set; }
        public string PackageName { get; set; }
        public string DownloadUrl { get; set; }
        public bool DebugRequest { get; set; }
    }
}
