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
        protected Context context;
        protected ProducedContext producedContext;
        private readonly JSONBTree bTree;
        private readonly Menu config; 

        protected GenericBot(Context context)
        {
            try
            {
                GetLogger().Info("Constructor");
                String botName = GetType().Name;
                Game.PrintChat(botName + " - Loading");
                config = new Menu(botName, botName, true);
                SetupMenu();

                bTree = new JSONBTree(this, "GenericBot");
                this.context = context;
                SetupContext();

                producedContext = new ProducedContext();
                SetupProducedContextCallbacks();

                CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
                Game.OnGameUpdate += Game_OnGameUpdate;
                Game.OnWndProc += Game_OnWndProc;
            }
            catch (Exception e)
            {
                Game.PrintChat(e.GetType().Name + " : " + e.Message);
                GetLogger().Error(e.ToString());

                CustomEvents.Game.OnGameEnd -= Game_OnGameEnd;
                Game.OnGameUpdate -= Game_OnGameUpdate;
                Game.OnWndProc -= Game_OnWndProc;
            }
        }

        private void SetupProducedContextCallbacks()
        {
            producedContext.Set(ProducedContextKey.EnemyHeroes, Producer_EnemyHeroes);
            producedContext.Set(ProducedContextKey.AllyHeroes, Producer_AllyHeroes);
        }

        private void SetupContext()
        {
            Assert.True(context.levelSpellsOrder.Count() > 0, "context.levelSpellsOrder is not setup");
            autoLevel = new AutoLevel(context.levelSpellsOrder);

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

        private void SetupMenu()
        {
            MenuItem configDebugMode = new MenuItem("debugMode", "Debug Mode");
            configDebugMode.SetValue(false);
            configDebugMode.ValueChanged += ConfigDebugMode_ValueChanged;
            config.AddItem(configDebugMode);
            
            config.AddToMainMenu();
        }

        private void ConfigDebugMode_ValueChanged(Object obj, OnValueChangeEventArgs args)
        {
            bool newValue = args.GetNewValue<bool>();
            Logger.debugEnabled = newValue;
            args.Process = true;
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
            GetLogger().Info("Game_OnGameEnd");
            // quit game somehow
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
                    GetLogger().Error(e.ToString());
                    context.disableTickProcessing = true;
                }
                producedContext.Clear();
                context.lastTickProcessed = currentTick;
            }
        }

        private Logger GetLogger()
        {
            return Logger.GetLogger(GetType().Name);
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