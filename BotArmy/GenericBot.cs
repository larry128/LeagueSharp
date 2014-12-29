using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class GenericBot
    {
        enum TreeKeys
        {
            NotInDanger
        }
        private static readonly Logger LOG = Logger.GetLogger("GenericBot");
        private readonly Dictionary<TreeKeys, JSONBTree> treeCache = new Dictionary<TreeKeys, JSONBTree>();
        private JSONBTree bTree;
        private Context context;
        private ProducedContext producedContext;

        public GenericBot()
        {
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (ulong) WindowsMessages.WM_KEYDOWN)
            {
                if (args.WParam == 0x75) // F6 - test shit
                {
                    Game.PrintChat("hitbox size: " + context.myHero.BoundingRadius);
                }
            }
        }

        private void Game_OnGameEnd(EventArgs args)
        {
            LOG.Info("Game_OnGameEnd");
            // quit game somehow
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            LOG.Info("Game_OnGameLoad");
            bTree = new JSONBTree(this);
            context = new Context();
            SetSpawns();

            producedContext = new ProducedContext();
            SetProducedContextCallbacks();

            LOG.Info("Game_OnGameLoad - GenericBot - Loaded");
            Game.PrintChat("GenericBot - Loaded");
        }

        private void SetProducedContextCallbacks()
        {
            producedContext.Set(ProducedContextKey.EnemyHeroes, Producer_EnemyHeroes);
            producedContext.Set(ProducedContextKey.AllyHeroes, Producer_AllyHeroes);
        }

        private void SetSpawns()
        {
            foreach (Obj_SpawnPoint spawn in ObjectManager.Get<Obj_SpawnPoint>())
            {
                if (spawn.IsAlly)
                {
                    context.allySpawn = spawn;
                }
                else
                {
                    context.enemySpawn = spawn;
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (context.disableTickProcessing)
            {
                return;
            }

            int currentTick = Environment.TickCount;
            if (currentTick - context.lastTickProcessed > context.tickDelay)
            {
                try
                {
                    bTree.Tick();
                }
                catch (Exception e)
                {
                    Game.PrintChat(e.GetType().Name + " : " + e.Message);
                    LOG.Error(e.ToString());
                    context.disableTickProcessing = true;
                }
                producedContext.Clear();
                context.lastTickProcessed = currentTick;
            }
        }

        public void Action_ZombieCast(Node node, String stack)
        {
            
        }

        public void Action_CheckEnvironment(Node node, String stack)
        {
        }

        public void Action_OnlySafeStuff(Node node, String stack)
        {
        }

        public void Action_OnlySafeCast(Node node, String stack)
        {
        }

        public void Action_RecklessCast(Node node, String stack)
        {
        }

        public void Action_RecklessMove(Node node, String stack)
        {
        }

        public void Action_RecklessAutoAttack(Node node, String stack)
        {
        }

        public void Action_AutoAttack(Node node, String stack)
        {
        }

        public void Action_CastAnything(Node node, String stack)
        {
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (context.myHero.IsMoving)
            {
                context.myHero.IssueOrder(GameObjectOrder.HoldPosition, context.myHero);
            }
        }

        public void Action_Move(Node node, String stack)
        {
            if (context.moveTo.IsValid() && (!context.myHero.IsMoving || context.moveTo.Distance(context.myHero.Path.Last(), true) < context.myHero.BoundingRadius))
            {
                context.myHero.IssueOrder(GameObjectOrder.MoveTo, context.moveTo);
            }
        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return context.myHero.IsZombie;
        }

        public bool Condition_WillInterruptSelf(Node node, String stack)
        {
            return false;
        }

        public bool Condition_BeReckless(Node node, String stack)
        {
            return false;
        }

        public bool Condition_InDanger(Node node, String stack)
        {
            JSONBTree tree;
            if (!treeCache.TryGetValue(TreeKeys.NotInDanger, out tree))
            {
                tree = new JSONBTree(new InDanger(context, producedContext));
                treeCache.Add(TreeKeys.NotInDanger, tree);
            }

            return tree.Tick();
        }

        public bool Condition_IsRegenerating(Node node, String stack)
        {
            if (IsInSpawn(context.myHero) && (context.myHero.Health != context.myHero.MaxHealth || context.myHero.Mana != context.myHero.MaxMana)) { 
                return true;
            }
            return false;
        }

        public bool IsInSpawn(Obj_AI_Hero hero)
        {
            return hero.Distance(context.allySpawn) < context.spawnBuyRange;
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