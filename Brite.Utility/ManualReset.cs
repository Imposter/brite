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
        private readonly ManualResetEvent _event;

        public ManualReset()
        {
            _event = new ManualResetEvent(false);
        }

        public async Task WaitAsync()
        {
            await Task.Run(() => _event.WaitOne());
        }

        public void Set()
        {
            _event.Set();
        }
    }
}
