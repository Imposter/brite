using System;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public class Log
    {
        private readonly Logger _logger;
        private readonly string _name;

        public Log(ref Logger logger, string name)
        {
            _logger = logger;
            _name = name;
        }

        public async Task InfoAsync(string format, params object[] args)
        {
            if (_logger.Level >= LoggerLevel.Info)
                await _logger.WriteLineAsync("[INFO] {0} - {1}: {2}", DateTime.Now, _name, string.Format(format, args));
        }

        public async Task ErrorAsync(string format, params object[] args)
        {
            if (_logger.Level >= LoggerLevel.Error)
                await _logger.WriteLineAsync("[ERROR] {0} - {1}: {2}", DateTime.Now, _name, string.Format(format, args));
        }

        public async Task WarnAsync(string format, params object[] args)
        {
            if (_logger.Level >= LoggerLevel.Warn)
                await _logger.WriteLineAsync("[WARN] {0} - {1}: {2}", DateTime.Now, _name, string.Format(format, args));
        }

        public async Task DebugAsync(string format, params object[] args)
        {
            if (_logger.Level >= LoggerLevel.Debug)
                await _logger.WriteLineAsync("[DEBUG] {0} - {1}: {2}", DateTime.Now, _name, string.Format(format, args));
        }

        public async Task TraceAsync(string format, params object[] args)
        {
            if (_logger.Level >= LoggerLevel.Trace)
                await _logger.WriteLineAsync("[TRACE] {0} - {1}: {2}", DateTime.Now, _name, string.Format(format, args));
        }
    }
}
