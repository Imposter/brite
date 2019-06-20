using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Brite.UWP.App.ViewModels;

using Windows.UI.Xaml.Controls;
using Brite.RPC;

namespace Brite.UWP.App.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel { get; } = new HomeViewModel();

        public HomePage()
        {
            InitializeComponent();

            // Test
            Task.Run(async () =>
            {
                var connection = new AppServiceConnection
                {
                    AppServiceName = "Brite.CommunicationService",
                    PackageFamilyName = "C34EC576-6ABD-46B4-8973-4DE88153FA72_hjy10qrht1atg"
                };

                var status = await connection.OpenAsync();
                Debug.WriteLine(status.ToString());
                if (status == AppServiceConnectionStatus.Success)
                {
                    var service = new RpcService(connection, new Guid("71d2fe17-fa91-4b41-b9f8-fb892ddd694f"));
                    service.Start();

                    while (true)
                    {
                        try
                        {
                            var target = await service.GetTargetAsync(new Guid("99a90afe-af50-462e-85fa-8275b0832cfb"));
                            dynamic update =
                                await target.GetObjectAsync("Update001"); // TODO: Put interfaces in a shared project
                            var name = await update.GetNameAsync();
                        }
                        catch (RpcException ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        await Task.Delay(5000);
                    }
                }
            });
        }
    }
}
