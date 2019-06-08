using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Brite.UWP.App.Core.Scripting
{
    // TODO: Move ScriptEngine here
    class Script : IDisposable
    {
        private readonly ScriptScope _scope;
        private readonly CompiledCode _code;
        private readonly TaskManager _taskManager;
        private bool _running;
        private CancellationTokenSource _tokenSource;

        public bool Running => _running;

        public Script(ScriptEngine engine, string code)
        {
            _scope = engine.CreateScope();

            var source = engine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect);
            _code = source.Compile();

            _taskManager = new TaskManager();
            _scope.SetVariable("Tasks", _taskManager); // TODO: Globals seem to not work -- except if its a function? -- TODO: Register a global TaskManager
            _scope.SetVariable("SomeNumber", 12);

            _running = false;
        }

        public void SetVariable(string name, object value)
        {
            _scope.SetVariable(name, value);
        }

        public T GetVariable<T>(string name)
        {
            return _scope.GetVariable<T>(name);
        }

        public async Task<dynamic> ExecuteAsync()
        {
            if (_running)
                throw new InvalidOperationException("Already executing");

            try
            {
                _running = true;
                _tokenSource = new CancellationTokenSource();
                return await Task.Run(() => _code.Execute(_scope), _tokenSource.Token);
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
                _running = true;
                _tokenSource = new CancellationTokenSource();
                return await Task.Run(() => _code.Execute<T>(_scope), _tokenSource.Token);
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
            _taskManager.Dispose(); // TODO: Clear tasks for script
        }
    }
}
