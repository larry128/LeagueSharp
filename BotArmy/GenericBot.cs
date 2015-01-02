﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public abstract class GenericBot
    {
        protected GenericContext context;
        protected ProducedContext producedContext;
        protected TargetFinder targetFinder;
        private readonly JSONBTree bTree;
        private readonly Menu config;
        private readonly List<ServerInteraction> serverInteractions = new List<ServerInteraction>();

        protected GenericBot(GenericContext context)
        {
            try
            {
                GetLogger().Info("Constructor");
                var botName = GetType().Name;
                Game.PrintChat(botName + " - Loading");
                bTree = new JSONBTree(this, "GenericBot");

                config = new Menu(botName, botName, true);
                SetupMenu();

                this.context = context;
                SetupContext();

                producedContext = new ProducedContext();
                SetupProducedContextCallbacks();
                targetFinder = new TargetFinder(context, producedContext, serverInteractions);
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
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void StopProcessing()
        {
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            CustomEvents.Game.OnGameEnd -= Game_OnGameEnd;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe && args.Target.IsValid)
            {
                Game.PrintChat("hitbox: " + args.Target.BoundingRadius + " distance: " + unit.Distance(args.Target.Position));
            }
        }

        private void SetupProducedContextCallbacks()
        {
            producedContext.Set(ProducedContextKey.Wards, Producer_Wards);
        }

        private void SetupContext()
        {
            Assert.True(context.levelSpellsOrder.Length > 0, "GenericContext.levelSpellsOrder is not setup");

            foreach (var spawn in ObjectManager.Get<Obj_SpawnPoint>())
            {
                Assert.True(spawn.IsValid<Obj_SpawnPoint>(), "invalid Obj_SpawnPoint");
                if (spawn.IsAlly)
                {
                    context.allySpawn = spawn;
                }
                else
                {
                    context.enemySpawn = spawn;
                }
            }

            context.summonerHeal = context.myHero.GetSpellSlot("summonerheal", true);
            context.summonerFlash = context.myHero.GetSpellSlot("summonerflash", true);
            context.summonerIgnite = context.myHero.GetSpellSlot("summonerdot", true);

            context.enemies = ProcessEachGameObject<Obj_AI_Hero>(hero => !hero.IsAlly);
            context.enemies.ForEach(hero => { context.heroesInfo.Add(hero.NetworkId, new TrackedHeroInfo(hero)); });
            context.allies = ProcessEachGameObject<Obj_AI_Hero>(hero => hero.IsAlly);
            context.allies.ForEach(hero => { context.heroesInfo.Add(hero.NetworkId, new TrackedHeroInfo(hero)); });
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

        private void Game_OnGameEnd(EventArgs args)
        {
            GetLogger().Info("Game_OnGameEnd");
            Thread oThread = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(20000);
                Environment.Exit(0);
            }));
            oThread.Start();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            context.currentTick = Environment.TickCount;
            if (context.currentTick - context.lastTickProcessed > context.tickDelay + Game.Ping)
            {
                if (serverInteractions.Count > 0)
                {
                    GetLogger()
                        .Error(
                            "Not all serverInteractions processed, pushing tick 50 * serverInteractions.Count millis.");
                    context.lastTickProcessed += 50 * serverInteractions.Count;
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

            producedContext.Clear();
            context.lastTickProcessed = context.currentTick;
        }

        private void ProcessServerInteractions()
        {
            if (serverInteractions.Count > 0)
            {
                GetLogger().Debug("serverInteractions.Count: " + serverInteractions.Count);
                var timePerAction = context.tickDelay / (serverInteractions.Count + 1);
                var delay = 0;
                foreach (var interaction in serverInteractions)
                {
                    delay += timePerAction;
                    var interactionLocal = interaction;
                    Utility.DelayAction.Add(delay, () =>
                    {
                        GetLogger().Debug(interactionLocal.change + " at tick: " + context.currentTick);
                        interactionLocal.serverAction();
                        var movingTo = interactionLocal.change as MovingTo;
                        if (movingTo != null)
                        {
                            context.lastDestination = movingTo.destination;
                        }
                        var holdingPosition = interactionLocal.change as HoldingPosition;
                        if (holdingPosition != null)
                        {
                            context.lastDestination = Vector3.Zero;
                        }

                        serverInteractions.Remove(interactionLocal);
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

            var abilityLevel = context.myHero.Spellbook.GetSpell(SpellSlot.Q).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.W).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.E).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.R).Level;
            if (context.myHero.Level > abilityLevel && abilityLevel < context.levelSpellsOrder.Length)
            {
                serverInteractions.Add(new ServerInteraction(new SpellLeveledUp(),
                    () => { context.myHero.Spellbook.LevelSpell(context.levelSpellsOrder[abilityLevel]); }));
            }
        }

        public void Action_Buy(Node node, String stack)
        {
            bTree.OnlyOncePer(500);

            // if you fail to buy at any point you have a 10 seconds timeout
            if ((context.myHero.InShop() || context.myHero.IsDead) && GetSecondsSince(context.lastFailedBuy) > 10)
            {
                var nextToBuy = GetNextBuyItemId();
                var elixir = ItemMapper.GetItem(context.shoppingListElixir);
                // handle initial consumables first
                if (context.shoppingListConsumables.Length > 0 && context.myHero.Level == 1)
                {
                    foreach (var consumable in context.shoppingListConsumables)
                    {
                        var consumableLocal = consumable;
                        var item = ItemMapper.GetItem(consumableLocal);
                        if (item.HasValue && context.myHero.GoldCurrent >= item.Value.Price)
                        serverInteractions.Add(new ServerInteraction(new BuyItem(),
                            () =>
                            {
                                if (!context.myHero.BuyItem(consumableLocal)) {
                                    context.lastFailedBuy = context.currentTick;
                                    }
                            }));
                    }
                    context.shoppingListConsumables = new ItemId[] { };
                }
                else if (nextToBuy.HasValue && context.myHero.GoldCurrent >= nextToBuy.Value.Price)
                {
                    if (GetOccuppiedInventorySlots().Count == 7)
                    {
                        var wardSlot = GetItemSlot(ItemId.Stealth_Ward);
                        var manaPotSlot = GetItemSlot(ItemId.Mana_Potion);
                        var healthPotSlot = GetItemSlot(ItemId.Health_Potion);
                        if (wardSlot != null)
                        {
                            serverInteractions.Add(new ServerInteraction(new SellItem(),
                                () => { context.myHero.SellItem(wardSlot.Slot); }));
                        }
                        else if (manaPotSlot != null)
                        {
                            serverInteractions.Add(new ServerInteraction(new SellItem(),
                                () => { context.myHero.SellItem(manaPotSlot.Slot); }));
                        }
                        else if (healthPotSlot != null)
                        {
                            serverInteractions.Add(new ServerInteraction(new SellItem(),
                                () => { context.myHero.SellItem(healthPotSlot.Slot); }));
                        }
                    }
                    else
                    {
                        serverInteractions.Add(new ServerInteraction(new BuyItem(),
                            () =>
                            {
                                if (!context.myHero.BuyItem((ItemId)nextToBuy.Value.Id))
                                {
                                    context.lastFailedBuy = context.currentTick;
                                }
                            }));
                    }
                }
                else if (GetMinutesSince(context.lastElixirBought) > 3 && elixir.HasValue && context.myHero.GoldCurrent >= elixir.Value.Price)
                {
                    serverInteractions.Add(new ServerInteraction(new BuyItem(),
                        () =>
                        {
                            if (!context.myHero.BuyItem(context.shoppingListElixir))
                                context.lastFailedBuy = context.currentTick;
                        }));
                    context.lastElixirBought = context.currentTick;
                }
            }
        }

        private int GetSecondsSince(int actionTookPlaceAt)
        {
            return (context.currentTick - actionTookPlaceAt) / 1000;
        }

        private int GetMinutesSince(int actionTookPlaceAt)
        {
            return (context.currentTick - actionTookPlaceAt) / 1000 / 60;
        }

        private InventorySlot GetItemSlot(ItemId itemId)
        {
            foreach (var inventorySlot in context.myHero.InventoryItems)
            {
                if (inventorySlot.Id == itemId)
                {
                    return inventorySlot;
                }
            }
            return null;
        }

        private ItemData.Item? GetNextBuyItemId()
        {
            if (context.shoppingList.Length > 0)
            {
                // expand inventory list
                var expandedInventory = new List<int>();
                foreach (var inventorySlot in GetOccuppiedInventorySlots())
                {
                    ExpandRecipe(inventorySlot.Id, expandedInventory);
                }

                // reduce expandedInventoryList
                foreach (var itemId in context.shoppingList)
                {
                    if (!expandedInventory.Remove((int)itemId))
                    {
                        ItemMapper.GetItem(itemId);
                    }
                }
            }
            return null;
        }

        private void ExpandRecipe(ItemId itemId, List<int> into)
        {
            into.Add((int)itemId);
            var item = ItemMapper.GetItem(itemId);
            if (item != null && item.Value.RecipeItems != null && item.Value.RecipeItems.Length > 0)
            {
                var recipe = item.Value.RecipeItems;
                into.AddRange(recipe);
                foreach (var id in recipe)
                {
                    ExpandRecipe((ItemId)id, into);
                }
            }
        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return context.myHero.IsZombie;
        }

        public abstract void Action_ZombieCast(Node node, String stack);

        public bool Condition_IsDead(Node node, String stack)
        {
            return context.myHero.IsDead;
        }

        public void Action_DropWard(Node node, String stack)
        {
            if (context.myHero.Level > 1)
            {
                // use wardSpell rather than wardSlot
                var wardSpell = GetWardSpell();
                InventorySlot wardSlot = null;
                if (wardSpell == null)
                {
                    wardSlot = GetWardSlot();
                }

                if ((wardSpell != null || wardSlot != null) && GetSecondsSince(context.lastWardDropped) > 4)
                {
                    var keys = context.wardSpots.Keys;
                    foreach (var key in keys)
                    {
                        if (key == GameObjectTeam.Neutral || key == context.myHero.Team)
                        {
                            List<WardSpot> values;
                            if (context.wardSpots.TryGetValue(key, out values))
                            {
                                foreach (var wardSpot in values)
                                {
                                    var position = wardSpot.GetPosition();
                                    var isAWardNear = IsAWardNear(position);
                                    if (!isAWardNear)
                                    {
                                        // will probably be more complicated than InRange...
                                        if (wardSpell != null && wardSpell.InRange(position.To3D()))
                                        {
                                            serverInteractions.Add(new ServerInteraction(new WardCast(),
                                                () => wardSpell.Cast(position)));
                                            return;
                                        }

                                        if (wardSlot != null)
                                        {
                                            var wardSlotSpell = new Spell(wardSlot.SpellSlot, TargetFinder.GetHitboxDistance(context.wardPlaceDistance, context.myHero));
                                            if (wardSlotSpell.InRange(position.To3D()))
                                            {
                                                serverInteractions.Add(new ServerInteraction(new WardUsed(wardSlot),
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

        private bool IsAWardNear(Vector2 position)
        {
            var wardList = producedContext.Get(ProducedContextKey.Wards) as List<GameObject>;
            foreach (var ward in wardList)
            {
                if (position.Distance(ward.Position) < context.wardSightRadius)
                {
                    return true;
                }
            }
            return false;
        }

        private InventorySlot GetWardSlot()
        {
            InventorySlot ward;
            var wardsUsed = new List<InventorySlot>();

            serverInteractions.ForEach(interaction =>
            {
                var wardUsed = interaction.change as WardUsed;
                if (wardUsed != null)
                {
                    wardsUsed.Add(wardUsed.wardSlot);
                }
            });

            if ((ward = GetItemSlot(ItemId.Warding_Totem_Trinket)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Greater_Stealth_Totem_Trinket)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Stealth_Ward)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Sightstone)) != null && ward.SpellSlot.IsReady() && !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Ruby_Sightstone)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Vision_Ward)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Greater_Vision_Totem_Trinket)) != null && ward.SpellSlot.IsReady() &&
                !wardsUsed.Contains(ward))
            {
                return ward;
            }

            return null;
        }

        protected abstract Spell GetWardSpell();
        public abstract bool Condition_WillInterruptSelf(Node node, String stack);

        public bool Condition_IsRecalling(Node node, String stack)
        {
            return context.myHero.IsRecalling();
        }

        public abstract void Action_CastSafeSpells(Node node, String stack);

        public void Action_RecklessCastSummoners(Node node, String stack)
        {
            var lowestHpAllyHealRange = targetFinder.GetLowestHpAlly(context.summonerHealRange);
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
            if (context.myHero.InFountain() &&
                (context.myHero.Health != context.myHero.MaxHealth || context.myHero.Mana != context.myHero.MaxMana))
            {
                return true;
            }
            return false;
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (context.myHero.IsMoving)
            {
                serverInteractions.Add(new ServerInteraction(new HoldingPosition(),
                    () => { context.myHero.IssueOrder(GameObjectOrder.HoldPosition, context.myHero); }));
            }
        }

        public bool Action_GoHome(Node node, String stack)
        {
            return false;
        }

        public bool Action_MoveToWard(Node node, String stack)
        {
            SafeMoveToDestination(context.enemySpawn.Position);
            return true;
        }

        public abstract void Action_Move(Node node, String stack);

        protected void SafeMoveToDestination(Vector3 destination)
        {
            if (destination.IsValid() && (!context.myHero.IsMoving || destination.Distance(context.lastDestination) > context.myHero.BoundingRadius))
            {
                serverInteractions.Add(new ServerInteraction(new MovingTo(destination),
                    () => { context.myHero.IssueOrder(GameObjectOrder.MoveTo, destination); }));
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

        private List<InventorySlot> GetOccuppiedInventorySlots()
        {
            var result = new List<InventorySlot>();
            foreach (var inventoryItem in context.myHero.InventoryItems)
            {
                if (!"".Equals(inventoryItem.DisplayName))
                {
                    result.Add(inventoryItem);
                }
            }
            return result;
        }

        protected delegate bool Condition<in T>(T hero);

        protected delegate void Action<in T>(T change);
    }
}