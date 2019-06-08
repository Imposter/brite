/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Net;
using System.Threading.Tasks;

using Brite.Utility.IO;

namespace Brite.Utility.Network
{
    public interface ITcpClient
    {
        int Timeout { get; set; }
        IPEndPoint RemoteEndPoint { get; }
        bool Connected { get; }

        Task ConnectAsync();
        Task DisconnectAsync();

        IStream GetStream();
    }
}
