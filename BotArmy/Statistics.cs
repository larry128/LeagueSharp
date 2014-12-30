using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace najsvan
{
    public class Statistics
    {
        private static readonly String STAT_PATH_PREFIX = LeagueSharp.Common.Config.LeagueSharpDirectory + "/Logs/";
        private const String STAT_PATH_POSTFIX = "_stats.log";
        private readonly String statsPath;
        private readonly Dictionary<String, int> stats = new Dictionary<String, int>();
        private int incrementCounter;

        private Statistics(String statName)
        {
            this.statsPath = STAT_PATH_PREFIX + statName + STAT_PATH_POSTFIX;
        }

        public static Statistics GetStatistics(String statName)
        {
            return new Statistics(statName);
        }

        public void Increment(String stat)
        {
            
            int value;
            if (stats.TryGetValue(stat, out value))
            {
                stats.Remove(stat);
                stats.Add(stat, value + 1);
            }
            else
            {
                stats.Add(stat, 1);
            }
            incrementCounter++;
            if (incrementCounter == 100)
            {
                Write();
                incrementCounter = 0;
            }
            
        }

        private void Write()
        {
            if (Logger.debugEnabled)
            {
                List<String> lines = new List<String>();
                foreach (String key in stats.Keys)
                {
                    int value;
                    if (stats.TryGetValue(key, out value))
                    {
                        lines.Add(key + " : " + value);
                    }
                }

                File.WriteAllLines(statsPath, lines);
            }
        }
    }
}
