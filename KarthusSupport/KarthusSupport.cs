using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class KarthusSupport
    {
        private const int TICK_DELAY = 100;
        private int lastTickProcessed;

        public KarthusSupport()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("KarthusSupport - Loaded");
            BTForrest forrest = new BTForrest("FirstTree", this);

            try
            {
                forrest.Tick();
            }
            catch (Exception e)
            {
               Game.PrintChat( e.GetType().Name + " : " + e.Message);
               Logger.Log(e.ToString());
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            int currentTick = Environment.TickCount;
            if (currentTick - lastTickProcessed > TICK_DELAY)
            {

                lastTickProcessed = currentTick;
            }
        }

        public bool Action_Action_1(Node node, String func, String stack)
        {
            Game.PrintChat(stack + node.ToString());
            return true;
        }

        public bool Condition_Condition_1(Node node, String func, String stack)
        {
            Game.PrintChat(stack + node.ToString());
            return true;
        }
    }
}