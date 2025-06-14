using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using File = System.IO.File;

namespace GooglePlayGamesLibrary
{
    public class GooglePlayGamesLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static IPlayniteAPI playniteAPI;

        private GooglePlayGamesLibrarySettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("fcd1bbc9-c3a3-499f-9a4c-8b7c9c8b9de8");

        // Addition of "on PC" for now because only games playable on PC and part of the emulator will be fetched.
        public override string Name => GooglePlayGames.ApplicationName + @" on PC";

        public override string LibraryIcon => GooglePlayGames.Icon;

        // Implementing Client adds ability to open it via special menu in Playnite.
        public override LibraryClient Client { get; } = new GooglePlayGamesLibraryClient(logger);

        public GooglePlayGamesLibrary(IPlayniteAPI api) : base(api)
        {
            // Use injected API instance.
            playniteAPI = api;

            Settings = new GooglePlayGamesLibrarySettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                CanShutdownClient = true,
                HasSettings = true
            };
        }

        internal readonly struct GameData
        {
            internal GameData(string shortcut, string gameStartURL, string gameName, bool useTrackingFallback)
            {
                this.shortcut = shortcut;
                this.gameStartURL = gameStartURL;
                this.gameName = gameName;
                this.useTrackingFallback = useTrackingFallback;
            }

            internal readonly string shortcut;
            internal readonly string gameStartURL;
            internal readonly string gameName;
            internal readonly bool useTrackingFallback;
        }

        private static string GetShortcutName(string shortcut)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(shortcut);

            return fileNameWithoutExtension;
        }

        private static string GetShortcutDescription(string shortcut)
        {
            var wshShell = new IWshRuntimeLibrary.WshShell();
            var wshShortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(shortcut);

            return wshShortcut.Description;
        }

        private static string[] GetShortcutContentArray(string shortcut)
        {
            var shortcutContentErrorIdentifier = "GooglePlayGamesShortcutContentError";

            Exception shortcutContentDataError = null;
            var shortcutContentDataErrorMessage = "Failed to read shortcut data. Faulting step: ReadAllText.";

            Exception shortcutContentMandatoryDataError = null;
            var shortcutContentMandatoryDataErrorMessage = "Failed to read mandatory shortcut data. Faulting step: GameStartURL.";

            // Exception shortcutContentOptionalDataError = null;
            var shortcutContentOptionalDataErrorMessage = "Failed to read optional shortcut data. Faulting step: GameName.";

            var shortcutContent = string.Empty;
            /**
             *  Resulting array for exemplary game "Eversoul" (at the time of writing this) consists of:
             *  ["googleplaygames://launch/?id=", = "googleplaygames://launch/?id="
             *   "<gameID>", = "com.kakaogames.eversoul"
             *   "&lid=<someNumber>&pid=<someAdditionalNumber>", = "&lid=1&pid=1" (it is unknown if this id combination applies to all possible usage scenarios)
             *   "<gameName>", = "Eversoul"
             *   "<shortcutNameWithoutExtension>"] = string.Empty if parsing finished without issues
             */
            var shortcutContentArray = new string[5];

            try
            {
                shortcutContent = File.ReadAllText(shortcut);
            }
            catch (Exception e)
            {
                logger.Error(e, shortcutContentDataErrorMessage);
                shortcutContentDataError = e;
            }

            if (shortcutContentDataError == null)
            {
                try
                {
                    var shortcutContentWithoutNullCharacters = Regex.Replace(shortcutContent, GooglePlayGames.shortcutRemoveNullCharactersRegex, string.Empty);
                    var shortcutContentWithoutSpecialCharacters = Regex.Replace(shortcutContentWithoutNullCharacters, GooglePlayGames.shortcutRemoveControlCharactersAndUnicodeRegex, string.Empty);
                    var shortcutContentGameStartURLArrayUnclean = Regex.Split(shortcutContentWithoutSpecialCharacters, GooglePlayGames.shortcutMatchGameStartURLRegex);

                    // googleplaygames://launch/?id=
                    shortcutContentArray[0] = shortcutContentGameStartURLArrayUnclean[1];
                    // <gameID>
                    shortcutContentArray[1] = shortcutContentGameStartURLArrayUnclean[2];
                    // &lid=<someNumber>&pid=<someAdditionalNumber>
                    shortcutContentArray[2] = shortcutContentGameStartURLArrayUnclean[3];
                }
                catch (Exception e)
                {
                    logger.Error(e, shortcutContentMandatoryDataErrorMessage);
                    shortcutContentMandatoryDataError = e;
                }

                if (shortcutContentMandatoryDataError == null)
                {
                    try
                    {
                        // „<gameName>“, <GooglePlayGames.ApplicationName>
                        var gameNameUnclean = GetShortcutDescription(shortcut);
                        var gameName = Regex.Split(gameNameUnclean, GooglePlayGames.shortcutMatchGameNameRegex);

                        // <gameName>
                        shortcutContentArray[3] = gameName[1];
                    }
                    catch (Exception e)
                    {
                        shortcutContentArray[4] = GetShortcutName(shortcut);

                        logger.Warn(e, shortcutContentOptionalDataErrorMessage);
                        // shortcutContentOptionalDataError = e;
                    }
                }
            }

            if (shortcutContentDataError != null || shortcutContentMandatoryDataError != null)
            {
                var shortcutContentErrorMessage = "Failed to read required shortcut data. Additional details are depicted in 'extensions.log'.";
                playniteAPI.Notifications.Add(shortcutContentErrorIdentifier, shortcutContentErrorMessage, NotificationType.Error);
            }
            else
            {
                playniteAPI.Notifications.Remove(shortcutContentErrorIdentifier);
            }

            return shortcutContentArray;
        }

        internal static Dictionary<string, GameData> GetInstalledGamesShortcutData()
        {
            var shortcutData = new Dictionary<string, GameData>();

            var shortcutsPath = GooglePlayGames.ShortcutsPath;
            var shortcuts = Directory.GetFiles(shortcutsPath);

            foreach (var shortcut in shortcuts)
            {
                var shortcutContentArray = GetShortcutContentArray(shortcut);

                string gameIdentifier;

                if (!string.IsNullOrEmpty(shortcutContentArray[1]))
                {
                    gameIdentifier = shortcutContentArray[1];
                }
                else
                {
                    // Should normally never happen unless additional shortcuts not created by Google Play Games were found.
                    continue;
                }

                var gameStartURL = string.Join(string.Empty, shortcutContentArray[0], shortcutContentArray[1], shortcutContentArray[2]);
                var gameName = shortcutContentArray[3];
                var useTrackingFallback = !string.IsNullOrEmpty(shortcutContentArray[4]);

                // Disallow game start URL retrieval to fail.
                if (!string.IsNullOrEmpty(gameStartURL))
                {
                    GameData gameData;
                    if (useTrackingFallback)
                    {
                        // Parsing of optional shortcut data failed = use shortcut name without extension as game name for game import
                        gameName = shortcutContentArray[4];

                        gameData = new GameData(shortcut, gameStartURL, gameName, true);
                    }
                    else
                    {
                        // Parsing finished without issues = use retrieved game name
                        gameData = new GameData(shortcut, gameStartURL, gameName, false);
                    }

                    shortcutData.Add(gameIdentifier, gameData);
                }
            }

            return shortcutData;
        }

        private static List<string> GetInstalledGamesIdentifiers()
        {
            var installedGamesIdentifierList = new List<string>();

            var installedGamesImageCachePath = GooglePlayGames.ImageCachePath;

            if (!string.IsNullOrEmpty(installedGamesImageCachePath))
            {
                var gameBackgroundIdentifier = GooglePlayGames.GameBackgroundIdentifierTypePNG;
                var searchPattern = @"*" + gameBackgroundIdentifier;

                var installedGamesBackgroundList = Directory.GetFiles(installedGamesImageCachePath, searchPattern);

                var installedGamesBackgroundListFileNames =
                    installedGamesBackgroundList.Select(Path.GetFileName);

                installedGamesIdentifierList = installedGamesBackgroundListFileNames
                                              .Select(x => x.TrimEndString(gameBackgroundIdentifier,
                                                                           StringComparison.OrdinalIgnoreCase))
                                              .ToList();
            }

            return installedGamesIdentifierList;
        }

        internal static List<GameMetadata> GetInstalledGames()
        {
            var installedGames = new List<GameMetadata>();

            var applicationName = GooglePlayGames.ApplicationName;

            var noGamesInstalledIdentifier = "GooglePlayGamesNoGamesInstalled";

            var installedGamesIdentifiers = GetInstalledGamesIdentifiers();

            if (!installedGamesIdentifiers.Any())
            {
                var noGamesInstalled = "No installed games found for " + applicationName + ".";
                playniteAPI.Notifications.Add(noGamesInstalledIdentifier, noGamesInstalled, NotificationType.Info);
                logger.Info(noGamesInstalled);

                return installedGames;
            }

            var installedGamesShortcutData = new Dictionary<string, GameData>();

            try
            {
                installedGamesShortcutData = GetInstalledGamesShortcutData();
            }
            catch (Exception)
            {
                // Use game ID as fallback for game name if shortcut data parsing is defective.
            }

            var libraryMetadataProvider = new GooglePlayGamesLibraryMetadataProvider();

            foreach (var gameIdentifier in installedGamesIdentifiers)
            {
                var gameName = string.Empty;

                if (installedGamesShortcutData.ContainsKey(gameIdentifier))
                {
                    var newGameShortcutData = installedGamesShortcutData[gameIdentifier];

                    if (newGameShortcutData.useTrackingFallback)
                    {
                        var shortcutDataGameNameFallbackError = @"Failed to retrieve game name from shortcut data of " + applicationName + @" for '" + gameIdentifier + @"'. Shortcut name without extension will be used as fallback for game name.";
                        logger.Info(shortcutDataGameNameFallbackError);
                    }

                    gameName = newGameShortcutData.gameName;
                }
                if (string.IsNullOrEmpty(gameName))
                {
                    // Command line exit used by auto close client results in an empty shortcut folder = Info severity.

                    var shortcutDataGameNameError = @"Failed to read shortcut data of " + applicationName + @" for '" + gameIdentifier + @"'. Game ID will be used as fallback for game name.";
                    logger.Info(shortcutDataGameNameError);
                    gameName = gameIdentifier;
                }

                var newGameMedia = libraryMetadataProvider.GetMetadata(gameIdentifier);

                var newGame = new GameMetadata
                {
                    GameId = gameIdentifier,
                    Name = gameName,
                    Icon = newGameMedia.Icon,
                    BackgroundImage = newGameMedia.BackgroundImage,
                    IsInstalled = true,
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    Source = new MetadataNameProperty(applicationName),
                    InstallDirectory = GooglePlayGames.UserDataImageFolderPath
                };

                installedGames.Add(newGame);
            }

            playniteAPI.Notifications.Remove(noGamesInstalledIdentifier);

            return installedGames;
        }

        private string ShimExe => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"PlayServiceShim.exe");

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                if (System.IO.File.Exists(ShimExe))
                {
                    Dictionary<string, Game> KnownGames = new Dictionary<string, Game>();
                    foreach (var game in playniteAPI.Database.Games)
                    {
                        if (game.PluginId == Id)
                        {
                            KnownGames[game.GameId] = game;
                        }
                    }

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

                    Models.Shim.Shim ShimLibrary = Playnite.SDK.Data.Serialization.FromJson<Models.Shim.Shim>(data);
                    if (ShimLibrary.success)
                    {
                        var AndroidGames = ShimLibrary.data.AndroidGameLibraries.SelectMany(dx => dx.Value.AndroidGames.Where(dr => dr.Value.InstalledApp != null && !dr.Value.InstalledApp.IsSystemApp)
                            .Select(dr =>
                            {
                                ulong? size = dr.Value.InstalledApp?.InstallationSizeBytes;
                                if (size == 0)
                                    size = null;
                                GameMetadata game = new GameMetadata()
                                {
                                    //Source = new MetadataNameProperty("Google Play Games on PC"),
                                    Source = new MetadataNameProperty(GooglePlayGames.ApplicationName),
                                    Name = dr.Value.InstalledApp.Title,
                                    GameId = dr.Value.InstalledApp.PackageName,
                                    IsInstalled = dr.Value.StateCase == Models.Shim.SingleApp.StateOneofCase.InstalledApp,
                                    InstallSize = size,
                                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                                    InstallDirectory = GooglePlayGames.UserDataImageFolderPath
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
                            })
                        ).ToList();
                        var PCGames = ShimLibrary.data.PcGames.Where(dr => dr.Value.InstalledGame != null)
                            .Select(dr =>
                            {
                                ulong? size = dr.Value.InstalledGame?.GameData?.DynastyMetadata?.InstallationDetails?.InstallationSize?.SizeInBytes;
                                if (size == 0)
                                    size = null;
                                GameMetadata game = new GameMetadata()
                                {
                                    //Source = new MetadataNameProperty("Google Play Games on PC"),
                                    Source = new MetadataNameProperty(GooglePlayGames.ApplicationName),
                                    Name = dr.Value.InstalledGame.Title,
                                    GameId = dr.Value.InstalledGame.GameData.PackageName,
                                    IsInstalled = dr.Value.StateCase == Models.Shim.PcApp.StateOneofCase.InstalledGame,
                                    InstallSize = size,
                                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                                };
                                // maybe also check InstalledTimestampMs?
                                if (dr.Value.InstalledGame.LastLaunchedTimestampMs > 0)
                                    game.LastActivity = DateTimeOffset.FromUnixTimeMilliseconds(dr.Value.InstalledGame.LastLaunchedTimestampMs).UtcDateTime;
                                //if (dr.Value.GameMetadata?.BackgroundImage?.Url != null)
                                //    game.BackgroundImage = new MetadataFile(dr.Value.GameMetadata?.BackgroundImage?.Url);
                                //if (dr.Value.GameMetadata?.AppIcon?.Url != null)
                                //    game.Icon = new MetadataFile(dr.Value.GameMetadata?.AppIcon?.Url);
                                //if (dr.Value.GameMetadata?.Logo?.Url != null)
                                //    game.Logo = new MetadataFile(dr.Value.GameMetadata?.Logo?.Url);
                                return game;
                            }).ToList();
                        List<GameMetadata> retVal = new List<GameMetadata>();
                        var AllNewMeta = AndroidGames.Union(PCGames);
                        foreach (GameMetadata meta in AllNewMeta)
                        {
                            if (KnownGames.ContainsKey(meta.GameId))
                                KnownGames.Remove(meta.GameId);
                            retVal.Add(meta);
                        }
                        foreach (var known in KnownGames)
                        {
                            retVal.Add(new GameMetadata()
                            {
                                GameId = known.Value.GameId,
                                IsInstalled = false,
                                InstallSize = null,
                            });
                        }
                        return retVal;
                    }
                }
            }


            var games = new List<GameMetadata>();

            var applicationName = GooglePlayGames.ApplicationName;

            var notInstalledIdentifier = "GooglePlayGamesNotInstalled";
            var importErrorIdentifier = "GooglePlayGamesImportError";

            if (!GooglePlayGames.IsInstalled)
            {
                var installationNotFound = applicationName + " installation not found.";
                playniteAPI.Notifications.Add(notInstalledIdentifier, installationNotFound, NotificationType.Error);
                logger.Error(installationNotFound);

                return games;
            }

            playniteAPI.Notifications.Remove(notInstalledIdentifier);

            Exception importError = null;

            try
            {
                var installedGames = GetInstalledGames();
                if (installedGames.Any())
                {
                    games.AddRange(installedGames);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to import games from " + applicationName + ".");
                importError = e;
            }

            if (importError != null)
            {
                playniteAPI.Notifications.Add(importErrorIdentifier,
                                              string.Format(playniteAPI.Resources.GetString("LOCLibraryImportError"), applicationName) +
                                              Environment.NewLine +
                                              importError.Message,
                                              NotificationType.Error);
            }
            else
            {
                playniteAPI.Notifications.Remove(importErrorIdentifier);
            }

            return games;
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new GooglePlayGamesLibraryPlayController(logger, playniteAPI, args.Game);
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new GooglePlayGamesLibraryMetadataProvider();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new GooglePlayGamesLibrarySettingsView();
        }
    }
}
