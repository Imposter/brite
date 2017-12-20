/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

namespace Brite.Utility.IO
{
    public interface ITimedStream : IStream
    {
        int Timeout { get; set; }
    }
}
