using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Linq;

namespace GooglePlayGamesLibrary
{
    public class GooglePlayGamesLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private GooglePlayGamesLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("fcd1bbc9-c3a3-499f-9a4c-8b7c9c8b9de8");

        // Addition of "on PC" for now because only games playable on PC and part of the emulator will be fetched.
        public override string Name => "Google Play Games on PC Library";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new GooglePlayGamesLibraryClient(logger);

        public GooglePlayGamesLibrary(IPlayniteAPI api) : base(api)
        {
            settings = new GooglePlayGamesLibrarySettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                CanShutdownClient = true,
                HasSettings = true
            };
        }

        private string ShimExe => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"PlayServiceShim.exe");
        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (System.IO.File.Exists(ShimExe))
                {
                    StringBuilder output = new StringBuilder();
                    Process proc = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = ShimExe,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                        }
                    };
                    
                    proc.Start();
                    string data = proc.StandardOutput.ReadToEnd();
                    ////proc.WaitForExit(); // I've had nothing but issues with this
                    //while (!proc.HasExited)
                    //    System.Threading.Thread.Sleep(100);
                    proc.Close();

                    string[] lines = data.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    Models.Shim.AppLibraryModuleState ShimLibrary = Playnite.SDK.Data.Serialization.FromJson<Models.Shim.AppLibraryModuleState>(lines[0]);
                    return ShimLibrary.AndroidGames.Where(dr => dr.Value.InstalledApp != null && !dr.Value.InstalledApp.IsSystemApp)
                                            .Select(dr => {
                                                GameMetadata game = new GameMetadata()
                                                {
                                                    Source = new MetadataNameProperty("Google Play Games on PC"),
                                                    Name = dr.Value.InstalledApp.Title,
                                                    GameId = dr.Value.InstalledApp.PackageName,
                                                    IsInstalled = dr.Value.StateCase == Models.Shim.SingleApp.StateOneofCase.InstalledApp,
                                                };
                                                // maybe also check InstalledTimestampMs?
                                                if (dr.Value.InstalledApp.LastLaunchedTimestampMs > 0)
                                                    game.LastActivity = DateTimeOffset.FromUnixTimeMilliseconds(dr.Value.InstalledApp.LastLaunchedTimestampMs).UtcDateTime;
                                                if (dr.Value.GameMetadata?.BackgroundImage?.Url != null)
                                                    game.BackgroundImage = new MetadataFile(dr.Value.GameMetadata?.BackgroundImage?.Url);
                                                if (dr.Value.GameMetadata?.AppIcon?.Url != null)
                                                    game.Icon = new MetadataFile(dr.Value.GameMetadata?.AppIcon?.Url);
                                                //if (dr.Value.GameMetadata?.Logo?.Url != null)
                                                //    game.Logo = new MetadataFile(dr.Value.GameMetadata?.Logo?.Url);
                                                return game;
                                            }).ToList();

                }
            }

            // Return list of user's games.
            return new List<GameMetadata>()
            {
                new GameMetadata()
                {
                    Name = "Notepad",
                    GameId = "notepad",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = "notepad.exe",
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(@"c:\Windows\notepad.exe")
                },
                new GameMetadata()
                {
                    Name = "Calculator",
                    GameId = "calc",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = "calc.exe",
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = true,
                    Icon = new MetadataFile(@"https://playnite.link/applogo.png"),
                    BackgroundImage = new MetadataFile(@"https://playnite.link/applogo.png")
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GooglePlayGamesLibrarySettingsView();
        }
    }
}