using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace KarthusSupport
{
    public class KarthusSupport
    {
        static void Main(string[] args)
        {
            new KarthusSupport();
        }       

        public KarthusSupport()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("KarthusSupport - Loaded");
        }
    }
}
