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
