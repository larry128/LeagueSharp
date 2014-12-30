using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public abstract class GenericBot
    {
        private static readonly Logger LOG = Logger.GetLogger("GenericBot");
        private JSONBTree bTree;
        private Context context;
        private ProducedContext producedContext;

        protected GenericBot()
        {
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Game.PrintChat(e.GetType().Name + " : " + e.Message);
                LOG.Error(e.ToString());
                context.disableTickProcessing = true;
            }
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (ulong)WindowsMessages.WM_KEYDOWN)
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

        public void Init()
        {
            LOG.Info("Init()");
            Game.PrintChat(this.GetType().Name + " - Loading");
            bTree = new JSONBTree(this, "GenericBot");
            context = new Context();
            SetSpawns();

            producedContext = new ProducedContext();
            SetProducedContextCallbacks();

            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;

            Game.PrintChat(this.GetType().Name + " - Loaded");
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

        public void Action_LevelSpells(Node node, String stack)
        {

        }

        public void Action_Buy(Node node, String stack)
        {

        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return context.myHero.IsZombie;
        }

        public virtual void Action_ZombieCast(Node node, String stack)
        {
            throw new NotImplementedException();
        }

        public bool Condition_IsDead(Node node, String stack)
        {
            return context.myHero.IsDead;
        }

        public void Action_DropWard(Node node, String stack)
        {

        }

        public bool Condition_WillInterruptSelf(Node node, String stack)
        {
            return false;
        }

        public void Action_CastAnythingSafe(Node node, String stack)
        {

        }

        public bool Condition_BeReckless(Node node, String stack)
        {
            return false;
        }

        public void Action_RecklessCast(Node node, String stack)
        {

        }

        public void Action_RecklessAutoAttack(Node node, String stack)
        {

        }

        public void Action_RecklessMove(Node node, String stack)
        {

        }

        public bool Condition_IsInPanic(Node node, String stack)
        {
            return false;
        }

        public void Action_PanicCounterMeasures(Node node, String stack)
        {

        }

        public bool Condition_IsInDanger(Node node, String stack)
        {
            return false;
        }

        public void Action_DangerCounterMeasures(Node node, String stack)
        {

        }

        public void Action_AutoAttack(Node node, String stack)
        {

        }

        public void Action_CastAnything(Node node, String stack)
        {

        }

        public bool Condition_IsRegenerating(Node node, String stack)
        {
            if (IsInSpawn(context.myHero) && (context.myHero.Health != context.myHero.MaxHealth || context.myHero.Mana != context.myHero.MaxMana))
            {
                return true;
            }
            return false;
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (context.myHero.IsMoving)
            {
                context.myHero.IssueOrder(GameObjectOrder.HoldPosition, context.myHero);
            }
        }

        public bool Action_MoveToWard(Node node, String stack)
        {
            return false;
        }

        public void Action_Move(Node node, String stack)
        {
            // figure out where to move
        }

        private void MoveToDestination(Vector3 destination)
        {
            if (destination.IsValid() && (!context.myHero.IsMoving || destination.Distance(context.myHero.Path.Last(), true) < context.myHero.BoundingRadius))
            {
                context.myHero.IssueOrder(GameObjectOrder.MoveTo, destination);
            }
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