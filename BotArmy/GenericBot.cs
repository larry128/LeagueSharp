using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public abstract class GenericBot
    {
        private readonly JSONBTree bTree;
        private readonly Menu config;

        protected GenericBot()
        {
            try
            {
                GetLogger().Info("Constructor");
                var botName = GetType().Name;
                Game.PrintChat(botName + " - Loading");
                bTree = new JSONBTree(this, "GenericBot");
                config = new Menu(botName, botName, true);
                SetupMenu();
                SetupContext();
                SetupProducedContextCallbacks();
                StartProcessing();
            }
            catch (Exception e)
            {
                Game.PrintChat(e.GetType().Name + " : " + e.Message);
                GetLogger().Error(e.ToString());

                StopProcessing();
            }
        }

        private void StartProcessing()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void StopProcessing()
        {
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe && args.Target.IsValid)
            {
                Game.PrintChat("hitbox: " + args.Target.BoundingRadius + " distance: " +
                               unit.Distance(args.Target.Position));
            }
        }

        private void SetupProducedContextCallbacks()
        {
            ProducedContext.Set(ProducedContextKey.Wards, Producer_Wards);
        }

        private void SetupContext()
        {
            Assert.True(GenericContext.levelSpellsOrder.Length > 0, "GenericContext.levelSpellsOrder is not setup");

            foreach (var spawn in ObjectManager.Get<Obj_SpawnPoint>())
            {
                Assert.True(spawn.IsValid<Obj_SpawnPoint>(), "invalid Obj_SpawnPoint");
                if (spawn.IsAlly)
                {
                    GenericContext.allySpawn = spawn;
                }
                else
                {
                    GenericContext.enemySpawn = spawn;
                }
            }

            GenericContext.summonerHeal = GenericContext.MY_HERO.GetSpellSlot("summonerheal", true);
            GenericContext.summonerFlash = GenericContext.MY_HERO.GetSpellSlot("summonerflash", true);
            GenericContext.summonerIgnite = GenericContext.MY_HERO.GetSpellSlot("summonerdot", true);

            GenericContext.enemies = ProcessEachGameObject<Obj_AI_Hero>(hero => !hero.IsAlly);
            GenericContext.allies = ProcessEachGameObject<Obj_AI_Hero>(hero => hero.IsAlly);
        }

        private void SetupMenu()
        {
            var configBotDebug = new MenuItem("botDebug", GetType().Name + " Debug");
            // default value
            configBotDebug.SetValue(false);
            GetLogger().debugEnabled = configBotDebug.GetValue<bool>();
            configBotDebug.ValueChanged += ConfigBotDebug_ValueChanged;
            config.AddItem(configBotDebug);

            var configJsonBTreeDebug = new MenuItem("jsonBTreeDebug", "JSONBTree Debug");
            // default value
            configJsonBTreeDebug.SetValue(false);
            Logger.GetLogger(bTree.GetType().Name).debugEnabled = configJsonBTreeDebug.GetValue<bool>();
            configJsonBTreeDebug.ValueChanged += ConfigJsonBTreeDebug_ValueChanged;
            config.AddItem(configJsonBTreeDebug);

            var configJsonBTreeStats = new MenuItem("jsonBTreeStats", "JSONBTree Stats");
            // default value
            configJsonBTreeStats.SetValue(false);
            Statistics.GetStatistics(bTree.GetType().Name).writingEnabled = configJsonBTreeStats.GetValue<bool>();
            configJsonBTreeStats.ValueChanged += ConfigJsonBTreeStats_ValueChanged;
            config.AddItem(configJsonBTreeStats);

            config.AddToMainMenu();
        }

        private void ConfigBotDebug_ValueChanged(Object obj, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<bool>();
            GetLogger().debugEnabled = newValue;
            args.Process = true;
        }

        private void ConfigJsonBTreeDebug_ValueChanged(Object obj, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<bool>();
            Logger.GetLogger(bTree.GetType().Name).debugEnabled = newValue;
            args.Process = true;
        }

        private void ConfigJsonBTreeStats_ValueChanged(Object obj, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<bool>();
            Statistics.GetStatistics(bTree.GetType().Name).writingEnabled = newValue;
            args.Process = true;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (ulong)WindowsMessages.WM_KEYDOWN)
            {
                if (args.WParam == 0x75) // F6 - test shit
                {
                    Game.PrintChat("...");
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            GenericContext.currentTick = Environment.TickCount;
            if (GenericContext.currentTick - GenericContext.lastTickProcessed > GenericContext.TICK_DELAY + Game.Ping)
            {
                if (GenericContext.SERVER_INTERACTIONS.Count > 0)
                {
                    GetLogger()
                        .Error(
                            "Not all serverInteractions processed, pushing tick 50 * GenericContext.serverInteractions.Count millis.");
                    GenericContext.lastTickProcessed += 50 * GenericContext.SERVER_INTERACTIONS.Count;
                    return;
                }

                ProcessTick();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ProcessTick()
        {
            try
            {
                bTree.Tick();

                ProcessServerInteractions();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                Game.PrintChat(e.GetType().Name + " : " + e.Message);
                GetLogger().Error(e.ToString());
                StopProcessing();
            }

            ProducedContext.Clear();
            GenericContext.lastTickProcessed = GenericContext.currentTick;
        }

        private void ProcessServerInteractions()
        {
            if (GenericContext.SERVER_INTERACTIONS.Count > 0)
            {
                GetLogger()
                    .Debug("GenericContext.serverInteractions.Count: " + GenericContext.SERVER_INTERACTIONS.Count);
                var timePerAction = GenericContext.TICK_DELAY / (GenericContext.SERVER_INTERACTIONS.Count + 1);
                var delay = 0;
                foreach (var interaction in GenericContext.SERVER_INTERACTIONS)
                {
                    delay += timePerAction;
                    var interactionLocal = interaction;
                    Utility.DelayAction.Add(delay, () =>
                    {
                        GetLogger().Debug(interactionLocal.change + " at tick: " + GenericContext.currentTick);
                        interactionLocal.serverAction();
                        var movingTo = interactionLocal.change as MovingTo;
                        if (movingTo != null)
                        {
                            GenericContext.lastDestination = movingTo.destination;
                        }
                        var holdingPosition = interactionLocal.change as HoldingPosition;
                        if (holdingPosition != null)
                        {
                            GenericContext.lastDestination = Vector3.Zero;
                        }

                        GenericContext.SERVER_INTERACTIONS.Remove(interactionLocal);
                    });
                }
            }
        }

        private Logger GetLogger()
        {
            return Logger.GetLogger(GetType().Name);
        }

        public void Action_Track(Node node, String stack)
        {
        }

        public void Action_LevelSpells(Node node, String stack)
        {
            bTree.OnlyOncePer(1000);

            var abilityLevel = GenericContext.MY_HERO.Spellbook.GetSpell(SpellSlot.Q).Level +
                               GenericContext.MY_HERO.Spellbook.GetSpell(SpellSlot.W).Level +
                               GenericContext.MY_HERO.Spellbook.GetSpell(SpellSlot.E).Level +
                               GenericContext.MY_HERO.Spellbook.GetSpell(SpellSlot.R).Level;
            if (GenericContext.MY_HERO.Level > abilityLevel && abilityLevel < GenericContext.levelSpellsOrder.Length)
            {
                GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellLeveledUp(),
                    () =>
                    {
                        GenericContext.MY_HERO.Spellbook.LevelSpell(GenericContext.levelSpellsOrder[abilityLevel]);
                    }));
            }
        }

        public void Action_Buy(Node node, String stack)
        {
            bTree.OnlyOncePer(500);

            // if you fail to buy at any point you have a 10 seconds timeout
            if ((GenericContext.MY_HERO.InShop() || GenericContext.MY_HERO.IsDead) &&
                GetSecondsSince(GenericContext.lastFailedBuy) > 10)
            {
                var nextToBuy = BotUtils.GetNextBuyItemId();
                var elixir = ItemMapper.GetItem(GenericContext.shoppingListElixir);
                // handle initial consumables first
                if (GenericContext.shoppingListConsumables.Length > 0 && GenericContext.MY_HERO.Level == 1)
                {
                    foreach (var consumable in GenericContext.shoppingListConsumables)
                    {
                        var consumableLocal = consumable;
                        var item = ItemMapper.GetItem(consumableLocal);
                        if (item.HasValue && GenericContext.MY_HERO.GoldCurrent >= item.Value.Price)
                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                                () =>
                                {
                                    if (!GenericContext.MY_HERO.BuyItem(consumableLocal))
                                    {
                                        GenericContext.lastFailedBuy = GenericContext.currentTick;
                                    }
                                }));
                    }
                    GenericContext.shoppingListConsumables = new ItemId[] { };
                }
                else if (nextToBuy.HasValue && GenericContext.MY_HERO.GoldCurrent >= nextToBuy.Value.Price)
                {
                    if (BotUtils.GetOccuppiedInventorySlots().Count == 7)
                    {
                        var wardSlot = BotUtils.GetItemSlot(ItemId.Stealth_Ward);
                        var manaPotSlot = BotUtils.GetItemSlot(ItemId.Mana_Potion);
                        var healthPotSlot = BotUtils.GetItemSlot(ItemId.Health_Potion);
                        if (wardSlot != null)
                        {
                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { GenericContext.MY_HERO.SellItem(wardSlot.Slot); }));
                        }
                        else if (manaPotSlot != null)
                        {
                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { GenericContext.MY_HERO.SellItem(manaPotSlot.Slot); }));
                        }
                        else if (healthPotSlot != null)
                        {
                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { GenericContext.MY_HERO.SellItem(healthPotSlot.Slot); }));
                        }
                    }
                    else
                    {
                        GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                            () =>
                            {
                                if (!GenericContext.MY_HERO.BuyItem((ItemId)nextToBuy.Value.Id))
                                {
                                    GenericContext.lastFailedBuy = GenericContext.currentTick;
                                }
                            }));
                    }
                }
                else if (GetMinutesSince(GenericContext.lastElixirBought) > 3 && elixir.HasValue &&
                         GenericContext.MY_HERO.GoldCurrent >= elixir.Value.Price)
                {
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                        () =>
                        {
                            if (!GenericContext.MY_HERO.BuyItem(GenericContext.shoppingListElixir))
                                GenericContext.lastFailedBuy = GenericContext.currentTick;
                        }));
                    GenericContext.lastElixirBought = GenericContext.currentTick;
                }
            }
        }

        private int GetSecondsSince(int actionTookPlaceAt)
        {
            return (GenericContext.currentTick - actionTookPlaceAt) / 1000;
        }

        private int GetMinutesSince(int actionTookPlaceAt)
        {
            return (GenericContext.currentTick - actionTookPlaceAt) / 1000 / 60;
        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return GenericContext.MY_HERO.IsZombie;
        }

        public abstract void Action_ZombieCast(Node node, String stack);

        public bool Condition_IsDead(Node node, String stack)
        {
            return GenericContext.MY_HERO.IsDead;
        }

        public void Action_DropWard(Node node, String stack)
        {
            if (GenericContext.MY_HERO.Level > 1)
            {
                // use wardSpell rather than wardSlot
                var wardSpell = GetWardSpell();
                InventorySlot wardSlot = null;
                if (wardSpell == null)
                {
                    wardSlot = BotUtils.GetWardSlot();
                }

                if ((wardSpell != null || wardSlot != null) && GetSecondsSince(GenericContext.lastWardDropped) > 4)
                {
                    var keys = GenericContext.WARD_SPOTS.Keys;
                    foreach (var key in keys)
                    {
                        if (key == GameObjectTeam.Neutral || key == GenericContext.MY_HERO.Team)
                        {
                            List<WardSpot> values;
                            if (GenericContext.WARD_SPOTS.TryGetValue(key, out values))
                            {
                                foreach (var wardSpot in values)
                                {
                                    var position = wardSpot.GetPosition();
                                    var isAWardNear = BotUtils.IsAWardNear(position);
                                    if (!isAWardNear)
                                    {
                                        // will probably be more complicated than InRange...
                                        if (wardSpell != null && wardSpell.InRange(position.To3D()))
                                        {
                                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new WardCast(),
                                                () => wardSpell.Cast(position)));
                                            return;
                                        }

                                        if (wardSlot != null)
                                        {
                                            var wardSlotSpell = new Spell(wardSlot.SpellSlot,
                                                BotUtils.GetHitboxDistance(GenericContext.WARD_PLACE_DISTANCE,
                                                    GenericContext.MY_HERO));
                                            if (wardSlotSpell.InRange(position.To3D()))
                                            {
                                                GenericContext.SERVER_INTERACTIONS.Add(
                                                    new ServerInteraction(new WardUsed(wardSlot),
                                                        () => wardSlotSpell.Cast(position)));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected abstract Spell GetWardSpell();
        public abstract bool Condition_WillInterruptSelf(Node node, String stack);

        public bool Condition_IsRecalling(Node node, String stack)
        {
            return GenericContext.MY_HERO.IsRecalling();
        }

        public abstract void Action_CastSafeSpells(Node node, String stack);

        public void Action_RecklessCastSummoners(Node node, String stack)
        {
            if (GenericContext.summonerHeal.IsReady())
            {
                var healTarget = TargetFinder.FindRecklessHelpAlly(GenericContext.SUMMONER_HEAL_RANGE);
                if (healTarget != null)
                {
                    Obj_AI_Hero[] healedTargets;
                    if (healTarget.NetworkId == GenericContext.MY_HERO.NetworkId)
                    {
                        healedTargets = new[] { healTarget };
                    }
                    else
                    {
                        healedTargets = new[] { healTarget, GenericContext.MY_HERO };
                    }
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new AllyHealed(healedTargets, BotUtils.GetSummonerHealAmount()),
                        () => { GenericContext.MY_HERO.Spellbook.CastSpell(GenericContext.summonerHeal, healTarget); }));
                }
            }
        }

        public void Action_RecklessCastItems(Node node, String stack)
        {
        }

        public abstract void Action_RecklessCastSpells(Node node, String stack);
        public abstract void Action_RecklessAutoAttack(Node node, String stack);
        public abstract bool Action_RecklessMove(Node node, String stack);

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

        public void Action_CastAnySpells(Node node, String stack)
        {
        }

        public bool Condition_IsRegenerating(Node node, String stack)
        {
            if (GenericContext.MY_HERO.InFountain() &&
                (GenericContext.MY_HERO.Health != GenericContext.MY_HERO.MaxHealth ||
                 GenericContext.MY_HERO.Mana != GenericContext.MY_HERO.MaxMana))
            {
                return true;
            }
            return false;
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (GenericContext.MY_HERO.IsMoving)
            {
                GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new HoldingPosition(),
                    () => { GenericContext.MY_HERO.IssueOrder(GameObjectOrder.HoldPosition, GenericContext.MY_HERO); }));
            }
        }

        public bool Action_GoHome(Node node, String stack)
        {
            return false;
        }

        public bool Action_MoveToWard(Node node, String stack)
        {
            SafeMoveToDestination(GenericContext.enemySpawn.Position);
            return true;
        }

        public abstract void Action_Move(Node node, String stack);

        protected void SafeMoveToDestination(Vector3 destination)
        {
            if (destination.IsValid() &&
                (!GenericContext.MY_HERO.IsMoving ||
                 destination.Distance(GenericContext.lastDestination) > GenericContext.MY_HERO.BoundingRadius))
            {
                GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new MovingTo(destination),
                    () => { GenericContext.MY_HERO.IssueOrder(GameObjectOrder.MoveTo, destination); }));
            }
        }

        protected List<T> ProcessEachGameObject<T>(Condition<T> cond) where T : GameObject, new()
        {
            var result = new List<T>();
            foreach (var obj in ObjectManager.Get<T>())
            {
                if (obj != null && obj.IsValid && cond(obj))
                {
                    result.Add(obj);
                }
            }
            return result;
        }

        private List<GameObject> Producer_Wards()
        {
            return
                ProcessEachGameObject<GameObject>(
                    obj => obj.IsValid && obj.IsVisible && obj.IsAlly && obj.Name.ToLower().Contains("ward"));
        }

        protected delegate bool Condition<in T>(T hero);

        protected delegate void Action<in T>(T change);
    }
}