using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp.Common;

namespace najsvan
{
    public class Statistics
    {
        private const String STAT_PATH_POSTFIX = "_stats.log";
        private static readonly Dictionary<String, Statistics> STAT_CACHE = new Dictionary<String, Statistics>();
        private static readonly String STAT_PATH_PREFIX = Config.AppDataDirectory + "/Logs/";
        private int incrementCounter;
        public bool writingEnabled = false;
        private readonly Dictionary<String, int> stats = new Dictionary<String, int>();
        private readonly String statsPath;

        private Statistics(String statName)
        {
            statsPath = STAT_PATH_PREFIX + statName + STAT_PATH_POSTFIX;
        }

        public static Statistics GetStatistics(String statName)
        {
            Statistics statistics;
            if (!STAT_CACHE.TryGetValue(statName, out statistics))
            {
                statistics = new Statistics(statName);
                STAT_CACHE.Add(statName, statistics);
            }
            return statistics;
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

        public void Delete()
        {
            try
            {
                File.Delete(statsPath);
            }
            catch
            {
                //ignore
            }
        }

        private void Write()
        {
            if (writingEnabled)
            {
                var lines = new List<String>();
                foreach (var key in stats.Keys)
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