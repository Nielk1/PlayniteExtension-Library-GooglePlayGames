﻿using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GooglePlayGamesLibrary
{
    public class GooglePlayGamesLibraryClient : LibraryClient
    {
        public override bool IsInstalled => GooglePlayGames.IsInstalled;

        public override string Icon => GooglePlayGames.Icon;

        public override void Open()
        {
            throw new NotImplementedException();
        }
    }
}