/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Threading.Tasks;

using Brite.Utility.IO;

namespace Brite.Utility
{
    public interface ISerializer
    {
        Task SerializeAsync<T>(IStream stream, T obj);
        Task<T> DeserializeAsync<T>(IStream stream);
    }
}
