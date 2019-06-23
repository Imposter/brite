using System;
using System.Diagnostics;
using System.Windows;

namespace Brite.App.Win.Services
{
    public sealed class ApplicationService : IApplicationService
    {
        private string _logFolder;

        public string LogFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(_logFolder))
                    return _logFolder;

                _logFolder = GetLogFolder();
                return _logFolder;
            }
        }

        public void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        public void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public void OpenFolder(string folder)
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }

        private static string GetLogFolder()
        {
            // TODO: Use registry or a config file
            throw new NotImplementedException();
        }
    }
}