using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class KarthusSupport
    {
        private static readonly Logger LOG = Logger.GetLogger("KarthusSupport");
        private BTForrest forrest;
        private ImmutableContext immutableContext;
        private MutableContext mutableContext;
        private ProducedContext producedContext;

        public KarthusSupport()
        {
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameEnd(EventArgs args)
        {
            LOG.Info("Game_OnGameEnd");
            // quit game somehow
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            LOG.Info("Game_OnGameLoad");
            forrest = new BTForrest("First", this);
            producedContext = new ProducedContext();
            mutableContext = new MutableContext();
            immutableContext = new ImmutableContext();

            producedContext.Set(ProducedContextKey.EnemyHeroes, Producer_EnemyHeroes);
            producedContext.Set(ProducedContextKey.AllyHeroes, Producer_AllyHeroes);

            LOG.Info("Game_OnGameLoad - KarthusSupport - Loaded");
            Game.PrintChat("KarthusSupport - Loaded");
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (mutableContext.disableTickProcessing)
            {
                return;
            }

            int currentTick = Environment.TickCount;
            if (currentTick - mutableContext.lastTickProcessed > immutableContext.tickDelay)
            {
                try
                {
                    LOG.Debug("forrest.Tick()");
                    forrest.Tick();
                }
                catch (Exception e)
                {
                    Game.PrintChat(e.GetType().Name + " : " + e.Message);
                    LOG.Error(e.ToString());
                    mutableContext.disableTickProcessing = true;
                }
                producedContext.Clear();
                mutableContext.lastTickProcessed = currentTick;
            }
        }

        public delegate bool Handler();

        public bool LoggingAspect(Node node, String stack, Handler handler)
        {
            bool reasult = handler();
            LOG.Debug(stack + " : " + reasult);
            return reasult;
        }

        public bool Action_FollowLeader(Node node, String stack)
        {
            return LoggingAspect(node, stack, () =>
            {
                immutableContext.myHero.IssueOrder(GameObjectOrder.MoveTo, mutableContext.leader.ServerPosition);
                return true;
            });
        }

        public bool Action_PickLeader(Node node, String stack)
        {
            return LoggingAspect(node, stack, () =>
            {
                List<Obj_AI_Hero> allyHeroes = (List<Obj_AI_Hero>)producedContext.Get(ProducedContextKey.AllyHeroes);
                if (allyHeroes.Count > 0)
                {
                    mutableContext.leader = allyHeroes[0];
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public bool Condition_IsLeaderKnown(Node node, String stack)
        {
            return LoggingAspect(node, stack, () => mutableContext.leader != null);
        }

        public delegate bool HeroCondition(Obj_AI_Hero hero);

        private List<Obj_AI_Hero> ForeachHeroes(HeroCondition cond)
        {
            List<Obj_AI_Hero> result = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (cond(hero))
                {
                    result.Add(hero);
                }
            }
            return result;
        }

        public List<Obj_AI_Hero> Producer_EnemyHeroes()
        {
            return ForeachHeroes((hero) => !hero.IsAlly && !hero.IsDead);
        }

        public List<Obj_AI_Hero> Producer_AllyHeroes()
        {
            return ForeachHeroes((hero) => hero.IsAlly && !hero.IsMe && !hero.IsDead);
        }
    }
}