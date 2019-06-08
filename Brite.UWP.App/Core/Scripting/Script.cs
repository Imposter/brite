using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brite.UWP.App.Core.Plugin;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Brite.UWP.App.Core.Scripting
{
    class Script : IDisposable
    {
        private readonly string _sourceCode;
        private readonly ScriptEngine _engine;
        private readonly ScriptScope _scope;
        private readonly TaskManager _taskManager;
        private readonly List<string> _searchPaths;
        private bool _running;
        private CancellationTokenSource _tokenSource;

        public bool Running => _running;

        public Script(string code)
        {
            _sourceCode = code;

            // Create engine
            _engine = Python.CreateEngine();

            // Redirect IO
            var outputStream = new TraceStream();
            var outputStreamWriter = new StreamWriter(outputStream);
            _engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);

            // Set library path
            _searchPaths = new List<string> { Path.Combine(Environment.CurrentDirectory, "Lib") };
            _engine.SetSearchPaths(_searchPaths);

            // Set global vars
            _taskManager = new TaskManager();
            _engine.GetBuiltinModule().SetVariable("Tasks", _taskManager);

            // Create local script scope
            _scope = _engine.CreateScope();

            _running = false;
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

        public void SetGlobalVariable(string name, object value)
        {
            _engine.GetBuiltinModule().SetVariable(name, value);
        }

        public T GetGlobalVariable<T>(string name)
        {
            return _engine.GetBuiltinModule().GetVariable<T>(name);
        }

        public void SetVariable(string name, object value)
        {
            _scope.SetVariable(name, value);
        }

        public T GetVariable<T>(string name)
        {
            return _scope.GetVariable<T>(name);
        }

        public dynamic CreateInstance(object obj, params object[] args)
        {
            return _engine.Operations.CreateInstance(obj, args);
        }

        public T CreateInstance<T>(object obj, params object[] args)
        {
            return _engine.Operations.CreateInstance(obj, args);
        }

        public async Task<dynamic> ExecuteAsync()
        {
            if (_running)
                throw new InvalidOperationException("Already executing");

            try
            {
                // Compile code
                var source = _engine.CreateScriptSourceFromString(_sourceCode, SourceCodeKind.AutoDetect);
                var code = source.Compile();

                _running = true;
                _tokenSource = new CancellationTokenSource();
                return await Task.Run(() => code.Execute(_scope), _tokenSource.Token);
            }
            catch (Exception ex)
            {
                throw new ScriptException("An exception was raised while executing the script", ex);
            }
            finally
            {
                _running = false;
                _taskManager.EndAll();
            }
        }

        public async Task<T> ExecuteAsync<T>()
        {
            if (_running)
                throw new InvalidOperationException("Already executing");

            try
            {
                // Compile code
                var source = _engine.CreateScriptSourceFromString(_sourceCode, SourceCodeKind.AutoDetect);
                var code = source.Compile();

                _running = true;
                _tokenSource = new CancellationTokenSource();
                return await Task.Run(() => code.Execute<T>(_scope), _tokenSource.Token);
            }
            catch (Exception ex)
            {
                throw new ScriptException("An exception was raised while executing the script", ex);
            }
            finally
            {
                _running = false;
                _taskManager.EndAll();
            }
        }

        public void Halt()
        {
            if (_running)
                _tokenSource.Cancel();
        }

        public void Dispose()
        {
            if (_running)
                _tokenSource.Cancel();
            _taskManager.Dispose();
        }
    }
}
