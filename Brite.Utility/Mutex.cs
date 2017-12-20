/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Threading;
using System.Threading.Tasks;

namespace Brite.Utility
{
    public class Mutex
    {
        private readonly SemaphoreSlim _semaphore;

        public Mutex()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LockAsync()
        {
            await _semaphore.WaitAsync();
        }

        public void Unlock()
        {
            _semaphore.Release();
        }
    }
}
