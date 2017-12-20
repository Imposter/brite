/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Windows.ApplicationModel.Background;

namespace Brite.UWP.App.Notifications
{
    public sealed class ServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        // RESTART SERVICE ON SETTINGS CHANGE
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            // TODO: Run background task

            _deferral.Complete();
        }
    }
}
