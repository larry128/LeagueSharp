using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp.Common;

namespace najsvan
{
    public class Logger
    {
        private const String LOG_PATH_POSTFIX = "_runtime.log";
        public static bool debugEnabled = false;
        private static readonly Dictionary<String, Logger> loggerCache = new Dictionary<String, Logger>();
        private static readonly String LOG_PATH_PREFIX = Config.LeagueSharpDirectory + "/Logs/";
        public readonly String loggerName;
        private readonly String logPath;

        private Logger(String loggerName)
        {
            this.loggerName = loggerName;
            logPath = LOG_PATH_PREFIX + loggerName + LOG_PATH_POSTFIX;
        }

        public static Logger GetLogger(String loggerName)
        {
            Logger logger;
            if (!loggerCache.TryGetValue(loggerName, out logger))
            {
                logger = new Logger(loggerName);
                loggerCache.Add(loggerName, logger);
            }
            return logger;
        }

        public void Error(String message)
        {
            Log("!__ERROR__!", message);
        }

        public void Info(String message)
        {
            Log("INFO", message);
        }

        public void Debug(String message)
        {
            if (debugEnabled)
            {
                Log("DEBUG", message);
            }
        }

        private void Log(String severity, String message)
        {
            File.AppendAllText(logPath,
                DateTime.Now.ToLongTimeString() + " : " + severity + " : " + message + "\n");
        }
    }
}