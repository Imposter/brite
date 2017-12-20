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
    public class ManualReset
    {
        private readonly SemaphoreSlim _semaphore;

        public ManualReset()
        {
            _semaphore = new SemaphoreSlim(0, 1);
        }

        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();
        }

        public void Set()
        {
            _semaphore.Release();
        }
    }
}
