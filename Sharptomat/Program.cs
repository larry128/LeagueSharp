using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            var champName = ObjectManager.Player.ChampionName;
            Game.PrintChat("Current champ: " + champName);
            switch (champName)
            {
                case "Karthus":
                    GenericAI bot = new KarthusAI();
                    break;
            }
        }
    }
}