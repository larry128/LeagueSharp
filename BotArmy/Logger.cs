using System;
using System.Collections.Generic;
using System.IO;

namespace najsvan
{
    public class Logger
    {
        public static bool debugEnabled = false;
        public readonly String loggerName;
        private static Dictionary<String, Logger> loggerCache = new Dictionary<String, Logger>();
        private static readonly String LOG_PATH_PREFIX = LeagueSharp.Common.Config.LeagueSharpDirectory + "/Logs/";
        private const String LOG_PATH_POSTFIX = "_runtime.log";
        private readonly String logPath;

        private Logger(String loggerName)
        {
            this.loggerName = loggerName;
            this.logPath = LOG_PATH_PREFIX + loggerName + LOG_PATH_POSTFIX;
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
                System.DateTime.Now.ToLongTimeString() + " : " + severity + " : " + message + "\n");
        }
    }
}
