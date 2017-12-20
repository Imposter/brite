/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public interface IStream : IDisposable
    {
        Task<int> ReadAsync(byte[] buffer, int offset, int length);
        Task WriteAsync(byte[] buffer, int offset, int length);
    }
}