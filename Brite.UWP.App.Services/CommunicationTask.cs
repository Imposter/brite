using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Brite.Utility.IO;
using Newtonsoft.Json;

namespace Brite.UWP.App.Services
{
    public sealed class CommunicationTask : IBackgroundTask
    {
        private static readonly Log Log = Logger.GetLog<CommunicationTask>();

        private BackgroundTaskDeferral _backgroundTaskDeferral;
        private AppServiceConnection _appServiceConnection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a deferral so that the service isn't terminated
            _backgroundTaskDeferral = taskInstance.GetDeferral();

            // Associate a cancellation handler with the background task
            taskInstance.Canceled += OnTaskCanceled;

            // Retrieve the app service connection and set up a listener for incoming app service requests
            if (!(taskInstance.TriggerDetails is AppServiceTriggerDetails details)) return;

            _appServiceConnection = details.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnRequestReceived;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get canceled while we are waiting
            var messageDeferral = args.GetDeferral();

            var request = args.Request.Message;

            // Log
            await Log.TraceAsync($"Received request: {JsonConvert.SerializeObject(request)}");

            // Complete the deferral so that the platform knows that we're done responding to the app service call.
            // Note for error handling: this must be called even if SendResponseAsync() throws an exception
            messageDeferral.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // Complete the service deferral
            _backgroundTaskDeferral?.Complete();
        }
    }
}
