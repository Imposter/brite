using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brite.UWP.App.Core.Plugin;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Brite.UWP.App.Core.Scripting
{
    // TODO: Create one engine per script and remove this class
    class ScriptManager // TODO: Rename this to module?
    {
        private readonly ScriptEngine _engine;
        private readonly List<string> _searchPaths;

        public ScriptManager()
        {
            _engine = Python.CreateEngine(); // TODO: One engine per script...?
            _searchPaths = new List<string> { Path.Combine(Environment.CurrentDirectory, "Lib") };

            // Redirect IO
            var outputStream = new DebugStream();
            var outputStreamWriter = new StreamWriter(outputStream);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);

            // Set library path
            _engine.SetSearchPaths(_searchPaths);
        }

        public void AddSearchPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!_searchPaths.Contains(fullPath))
            {
                _searchPaths.Add(fullPath);
                _engine.SetSearchPaths(_searchPaths);
            }
        }

        public void RemoveSearchPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (_searchPaths.Contains(fullPath))
            {
                _searchPaths.Remove(path);
                _engine.SetSearchPaths(_searchPaths);
            }
        }

        public void LoadAssembly(Assembly assembly)
        {
            _engine.Runtime.LoadAssembly(assembly);
        }

        public Script LoadScript(string code)
        {
            return new Script(_engine, code);
        }

        public dynamic CreateInstance(object obj, params object[] args)
        {
            return _engine.Operations.CreateInstance(obj, args);
        }

        public T CreateInstance<T>(object obj, params object[] args)
        {
            return _engine.Operations.CreateInstance(obj, args);
        }

        public void SetVariable(string name, object value)
        {
            _engine.GetBuiltinModule().SetVariable(name, value);
        }

        public T GetVariable<T>(string name)
        {
            return _engine.GetBuiltinModule().GetVariable<T>(name);
        }
    }
}
