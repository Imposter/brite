using System;

namespace Brite.UWP.App.Core.Scripting
{
    class ScriptException : Exception
    {
        public ScriptException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
