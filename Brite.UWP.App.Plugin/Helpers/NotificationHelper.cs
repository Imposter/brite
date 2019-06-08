using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications.Management;

namespace Brite.UWP.App.Plugin.Helpers
{
    public sealed class NotificationHelper
    {
        public static async Task<UserNotificationListener> GetListenerAsync()
        {
            if (!ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
                throw new InvalidOperationException("Notification listening is not supported by the system");

            // Get the listener
            var listener = UserNotificationListener.Current;

            // And request access to the user's notifications (must be called from UI thread)
            var accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
                    return listener;

                // This means the user has denied access.
                // Any further calls to RequestAccessAsync will instantly
                // return Denied. The user must go to the Windows settings
                // and manually allow access.
                default:
                    throw new UnauthorizedAccessException("Permission to access notifications was not provided");
            }
        }
    }
}
