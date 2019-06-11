
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace Brite.Win.App.Daemon
{
    public class App : System.Windows.Application
    {
        [STAThread]
        public static void Main()
        {
            Task.Run(async () =>
            {
                var appNamespace = typeof(App).Namespace;
                using (var md5 = MD5.Create())
                {
                    var guid = new Guid(md5.ComputeHash(Encoding.ASCII.GetBytes(appNamespace)));

                    var connection = new AppServiceConnection();
                    connection.AppServiceName = appNamespace;// TODO: These are supposed to be the app infos
                    connection.PackageFamilyName = guid.ToString();

                    var status = await connection.OpenAsync();
                    if (status == AppServiceConnectionStatus.Success)
                    {
                        // Hello!
                        Console.WriteLine("We did it bois");
                    }
                }
            });

            var app = new App();
            app.StartupUri = new Uri("AboutWindow.xaml", System.UriKind.Relative);
            app.Run();
        }
    }
}
