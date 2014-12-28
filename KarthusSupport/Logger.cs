using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace najsvan
{
    public class Logger
    {
        private static String LOG_PATH = LeagueSharp.Common.Config.LeagueSharpDirectory + "/Logs/KarthusSupport_runtime.log";

        public static void Log(String message)
        {
            File.WriteAllText(LOG_PATH, message);
        }
    }
}
