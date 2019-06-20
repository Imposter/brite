
using Brite.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Brite.Win.App.Daemon.Interface.Implementation.Update;

namespace Brite.Win.App.Daemon // TODO: Convert to .NET Core 3.0
{
    public class App
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var connection = new AppServiceConnection
                        {
                            AppServiceName = "Brite.CommunicationService",
                            PackageFamilyName = "C34EC576-6ABD-46B4-8973-4DE88153FA72_hjy10qrht1atg"
                        };

                        // TODO: These are supposed to be the app infos
                        var status = await connection.OpenAsync();
                        Console.WriteLine(status.ToString());
                        MessageBox.Show(status.ToString());
                        if (status == AppServiceConnectionStatus.Success)
                        {
                            /*
                            // Hello!
                            Console.WriteLine("We did it bois");

                            while (true)
                            {
                                var message = new ValueSet
                                {
                                    ["Class"] = "TestClass",
                                    ["Method"] = "TestMethod",
                                    ["NumArgs"] = 0
                                };
                                var response = await connection.SendMessageAsync(message);

                                await Task.Delay(2500);
                            }
                            */

                            // Create service
                            var service = new RpcService(connection, new Guid("99a90afe-af50-462e-85fa-8275b0832cfb"));
                            service.AddObject("Update001", new Update001());
                            service.Start();

                            while (true)
                            {
                                await Task.Delay(100);
                            }

                            // TODO: We need CommunicationTask otherwise we'll always fail to connect to the App's network
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    //var notifyIcon = new NotifyIcon
                    //{
                    //    Icon = Properties.Resources.BriteIcon,
                    //    Visible = true
                    //};
                    //notifyIcon.ContextMenu = new ContextMenu(new[]
                    //{
                    //    new MenuItem("About", (sender, eventArgs) =>
                    //    {
                    //        // TODO: Show AboutForm
                    //    }),
                    //    // TODO: Check for updates
                    //    new MenuItem("Exit", (sender, eventArgs) =>
                    //    {
                    //        notifyIcon.Visible = false;
                    //        Environment.Exit(0);
                    //    })
                    //});
                }).Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error More!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}