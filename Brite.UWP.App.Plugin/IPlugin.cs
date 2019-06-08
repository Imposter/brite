using System;
using Windows.UI.Xaml.Controls;

namespace Brite.UWP.App.Plugin
{
    public interface IPlugin : IDisposable
    {
        void Start();
        void Stop();

        Page GetConfigPage();
    }
}