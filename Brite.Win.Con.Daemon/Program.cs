using Brite.Utility.IO;
using Brite.Win.Con.Daemon.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Brite.Win.Con.Daemon
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Brite Firmware Updater v{0}", Assembly.GetExecutingAssembly().GetName().Version);

            // Read instance
            var instancePath = "./instance/";
            if (args.Length > 1)
                instancePath = args[1];

            // Check if instance path exists
            if (!Directory.Exists(instancePath))
            {
                Console.WriteLine("Invalid instance path!");
                MessageBox.Show("Invalid instance path!", "Brite", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if config file exists
            if (!File.Exists(Path.Combine(instancePath, "config.json")))
            {
                Console.WriteLine("Config file not found!");
                MessageBox.Show("Config file not found!", "Brite", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Initialize logger
            var logger = new FileLogger(Path.Combine(instancePath, "brite-daemon.log"));
            Logger.SetInstance(logger);

            // Create notification icon
            var notifyIcon = new NotifyIcon
            {
                Icon = Resources.BriteIcon,
                Visible = true
            };
            notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Edit Configuration", (sender, eventArgs) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = Path.GetFullPath(Path.Combine(instancePath, ".\\config.json"))
                    });
                }),
                new MenuItem("Exit", (sender, eventArgs) =>
                {
                    notifyIcon.Visible = false;
                    Environment.Exit(0);
                })
            });

            // Create service
            var service = new Service(instancePath);
            service.StartAsync().Wait();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            service.StopAsync().Wait();
        }
    }
}
