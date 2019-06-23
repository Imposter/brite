using System;

namespace Brite.App.Plugin
{
    public interface IPlugin : IDisposable
    {
        void Start();
        void Stop();

        string GetConfigPageXaml();
    }
}