using System;
using System.Collections.Generic;
using System.Text;

namespace REP2P
{
    public static class P2PLogger
    {
        public const string LOG = "[P2P]";
        private static event LogMethod log;
        private static event LogMethod warning;
        private static event LogMethod error;
        private static event ExceptionLogMethod exception;

        public delegate void LogMethod(string log);
        public delegate void ExceptionLogMethod(Exception exception);
        public static void Initialize(LogMethod _log, LogMethod _warning, LogMethod _error, ExceptionLogMethod _exception)
        {
            log = _log;
            warning = _warning;
            error = _error;
            exception = _exception;
        }

        public static void Log(string _log) => log?.Invoke(LOG + _log);
        public static void Warning(string _log) => warning?.Invoke(LOG + _log);
        public static void Error(string _log) => error?.Invoke(LOG + _log);
        public static void Exception(Exception _exception) => exception?.Invoke(_exception);
    }
}
