using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Brite.UWP.App.Common;
using Brite.UWP.Core.IO;
using Brite.UWP.Core.Network;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace Brite.UWP.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootFrame : Page
    {
        public ICommand NavigateCommand => new DelegateCommand(p => Navigate((Type)p));

        // DEBUG
        private readonly UdpServer s;

        public RootFrame()
        {
            InitializeComponent();

            s = new UdpServer(IPAddress.Loopback, 6645);
            s.StartAsync().Wait();

            Task.Factory.StartNew(new Action(async () =>
            {
                var s = new DatagramSocket(); // WORKS!
                await s.ConnectAsync(new HostName("127.0.0.1"), "6645");
                using (var st = new Stream(s.OutputStream))
                {
                    var b = Encoding.ASCII.GetBytes("hello!");
                    await st.WriteAsync(b, 0, b.Length);
                }
            }));
        }

        public void Navigate(Type targetPageType)
        {
            var frame = Content as Frame;
            frame?.Navigate(targetPageType);
        }
    }
}
