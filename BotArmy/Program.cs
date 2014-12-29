using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            String champName = ObjectManager.Player.ChampionName;
            switch (champName)
            {
                case "Karthus":
                    GenericBot bot = new KarthusBot();
                    break;
            }
        }
    }
}
