using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp.Common;

namespace najsvan
{
    public class Logger
    {
        private const String LOG_PATH_POSTFIX = "_runtime.log";
        private static readonly Dictionary<String, Logger> LOGGER_CACHE = new Dictionary<String, Logger>();
        private static readonly String LOG_PATH_PREFIX = Config.LeagueSharpDirectory + "/Logs/";
        public bool debugEnabled = false;
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
            if (!LOGGER_CACHE.TryGetValue(loggerName, out logger))
            {
                logger = new Logger(loggerName);
                LOGGER_CACHE.Add(loggerName, logger);
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

        public void Delete()
        {
            File.Delete(logPath);
        }

        private void Log(String severity, String message)
        {
            File.AppendAllText(logPath,
                DateTime.Now.ToLongTimeString() + " : " + severity + " : " + message + "\n");
        }
    }
}