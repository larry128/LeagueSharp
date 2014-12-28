using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class KarthusSupport
    {
        private static Logger LOG = Logger.GetLogger("KarthusSupport");
        private const int TICK_DELAY = 100;
        private int lastTickProcessed;
        private BTForrest forrest;

        public KarthusSupport()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            forrest = new BTForrest("First", this);
            Game.PrintChat("KarthusSupport - Loaded");
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            int currentTick = Environment.TickCount;
            if (currentTick - lastTickProcessed > TICK_DELAY)
            {
                try
                {
                    forrest.Tick();
                }
                catch (Exception e)
                {
                    Game.PrintChat(e.GetType().Name + " : " + e.Message);
                    LOG.Error(e.ToString());
                }
                lastTickProcessed = currentTick;
            }
        }

        public bool Action_Action_1(Node node, String func, String stack)
        {
            return true;
        }

        public bool Condition_Condition_1(Node node, String func, String stack)
        {
            return true;
        }
    }
}