/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Brite.API;
using Brite.API.Client;
using Brite.Utility.Network;

namespace Brite.UWP.App
{
    internal static class Global
    {
        public static bool Running;

        public static RootFrame RootFrame;

        public static ITcpClient TcpClient;
        public static BriteClient BriteClient;

        public static Config Config;
    }
}