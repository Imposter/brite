using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Brite.UWP.Core.Network;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Brite.UWP.App.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private TcpServer server;
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (server != null)
                await server.StopAsync();

            server = new TcpServer(new IPEndPoint(IPAddress.Loopback, 2200));
            server.OnDataReceived += async (o, args) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var dataDialog = new ContentDialog
                    {
                        Title = $"Data from {args.Source}",
                        Content = Encoding.ASCII.GetString(args.Buffer, 0, args.Length),
                        CloseButtonText = "Ok"
                    };

                    await dataDialog.ShowAsync();
                });
            };
            await server.StartAsync();

            var client = new TcpClient(server.ListenEndPoint);
            await client.ConnectAsync();
            await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("Suck"), 0, 4);
            await client.DisconnectAsync();
        }
    }
}
