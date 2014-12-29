using System;
using System.IO;

namespace najsvan
{
    public class Logger
    {
        public static readonly bool DEBUG_ENABLED = true;
        private static readonly String LOG_PATH_PREFIX = LeagueSharp.Common.Config.LeagueSharpDirectory + "/Logs/";
        private const String LOG_PATH_POSTFIX = "_runtime.log";

        private readonly String logPath;

        private Logger(String logPath)
        {
            this.logPath = logPath;
        }

        public static Logger GetLogger(String loggerName)
        {
            return new Logger(LOG_PATH_PREFIX + loggerName + LOG_PATH_POSTFIX);
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
            if (DEBUG_ENABLED)
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
