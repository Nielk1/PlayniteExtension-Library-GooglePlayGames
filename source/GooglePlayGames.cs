﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GooglePlayGamesLibrary
{
    internal class GooglePlayGames
    {
        public static string DataPath
        {
            get
            {
                string dataPath;

                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Google\Play Games"))
                {
                    if (key?.GetValueNames().Contains("UserLocalAppDataRoot") == true)
                    {
                        var rootPath = key.GetValue("UserLocalAppDataRoot")?.ToString();
                        dataPath = Path.Combine(rootPath, "Google", "Play Games");
                        if (Directory.Exists(dataPath))
                        {
                            return dataPath;
                        }
                    }
                    }

                // Fallback to default location if registry key is missing.
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                dataPath = Path.Combine(localAppData, "Google", "Play Games");
                if (Directory.Exists(dataPath))
                {
                    return dataPath;
                }

                return string.Empty;
            }
        }

        public static string InstallationPath
        {
            get
            {
                string installationPath;

                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Google\Play Games"))
                {
                    if (key?.GetValueNames().Contains("InstallFolder") == true)
                    {
                        installationPath = key.GetValue("InstallFolder")?.ToString();
                        if (Directory.Exists(installationPath))
                        {
                            return installationPath;
                        }
                    }
                    }

                // Fallback to default location if registry key is missing.
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                installationPath = Path.Combine(programFiles, "Google", "Play Games");
                if (Directory.Exists(installationPath))
                {
                    return installationPath;
                }

                return string.Empty;
            }
        }

        public static string MainExecutablePath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Bootstrapper.exe");
            }
        }

        public static bool IsInstalled
        {
            get
            {
                var path = MainExecutablePath;
                return !string.IsNullOrEmpty(path) && File.Exists(path);
            }
        }
    }
}
