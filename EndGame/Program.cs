using System;
using System.Threading;
using LeagueSharp.Common;

namespace EndGame
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
        }

        private static void Game_OnGameEnd(EventArgs args)
        {
            var oThread = new Thread(() =>
            {
                Thread.Sleep(30000);
                Environment.Exit(0);
            });
            oThread.Start();
        }
    }
}