using System;
using System.Windows.Controls;

namespace Brite.Win.App.Plugin
{
    public interface IPlugin : IDisposable
    {
        void Start();
        void Stop();

        Page GetConfigPage();
    }
}