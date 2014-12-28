using System;
using System.IO;

namespace najsvan
{
    public class Logger
    {
        private static String LOG_PATH_PREFIX = LeagueSharp.Common.Config.LeagueSharpDirectory + "/Logs/";
        private static String LOG_PATH_POSTFIX = "_runtime.log";

        private String logPath;

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

        private void Log(String severity, String message)
        {
            File.AppendAllText(logPath,
                System.DateTime.Now.ToShortTimeString() + " : " + severity + " : " + message + "\n");
        }
    }
}
