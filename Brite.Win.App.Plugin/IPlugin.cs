using System;

namespace Brite.Win.App.Plugin
{
    public interface IPlugin : IDisposable
    {
        void Start();
        void Stop();

        string GetConfigPageXaml();
    }
}