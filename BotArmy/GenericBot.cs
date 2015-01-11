using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace najsvan
{
    public abstract class GenericBot
    {
        public delegate void Action<in T>(T change);

        private readonly JSONBTree bTree;
        private readonly Menu config;

        public GenericBot()
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
                Utility.DelayAction.Add(2000, StartProcessing);
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
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnGameEnd += Game_OnGameEnd;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private void StopProcessing()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
            Game.OnGameEnd -= Game_OnGameEnd;
            GameObject.OnCreate -= GameObject_OnCreate;
            GameObject.OnDelete -= GameObject_OnDelete;
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (args.ToString().ToLower().Contains(GenericContext.TARGETED_BY_TOWER_OBJ_NAME))
            {
                GenericContext.GetHeroInfo(GenericContext.MY_HERO)
                    .SetFocusedByTower(LibraryOfAlexandria.GetNearestTower(GenericContext.MY_HERO, false));
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (args.ToString().ToLower().Contains(GenericContext.TARGETED_BY_TOWER_OBJ_NAME))
            {
                GenericContext.GetHeroInfo(GenericContext.MY_HERO).SetFocusedByTower(null);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (GenericContext.GetHeroInfo(GenericContext.MY_HERO).GetDirection().IsValid())
            {
                const int textOffsetX = 50;
                const int textOffsetY = 70;

                var worldSketchyDirection = GenericContext.GetHeroInfo(GenericContext.MY_HERO).GetDirection();
                var worldDirection = worldSketchyDirection;
                var sdLength = Math.Sqrt(worldSketchyDirection.X * worldSketchyDirection.X + worldSketchyDirection.Y * worldSketchyDirection.Y);
                const int length = 600;
                if (sdLength != 0) { 
                    var coefficient = length/(float)sdLength;
                    worldDirection *= coefficient;
                    var start = Drawing.WorldToScreen(GenericContext.MY_HERO.Position);
                    var end = Drawing.WorldToScreen(GenericContext.MY_HERO.Position + worldDirection.To3D());
                    Drawing.DrawLine(start, end, 2, Color.Blue);
                    Drawing.DrawText(textOffsetX, textOffsetY, Color.LawnGreen,
                        ((int)sdLength).ToString());
                }



            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsValid<Obj_AI_Hero>() && args.Target.IsValid && args.Target.Position.IsValid())
            {
                var hero = unit as Obj_AI_Hero;
                if (hero != null) {
                    GenericContext.GetHeroInfo(hero).SetDirection(hero.Position.To2D(), args.Target.Position.To2D());
                }
            }

            if (unit.IsValid<Obj_AI_Turret>() && args.Target.IsValid<Obj_AI_Hero>())
            {
                GenericContext.GetHeroInfo((Obj_AI_Hero)args.Target).SetFocusedByTower((Obj_AI_Turret)unit);
            }
        }

        private void SetupContext()
        {
            GenericContext.summonerHeal = GenericContext.MY_HERO.GetSpellSlot("summonerheal");
            GenericContext.summonerFlash = GenericContext.MY_HERO.GetSpellSlot("summonerflash");
            GenericContext.summonerIgnite = GenericContext.MY_HERO.GetSpellSlot("summonerdot");
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
            var jsonBTreeStats = Statistics.GetStatistics(bTree.GetType().Name);
            jsonBTreeStats.writingEnabled = newValue;
            args.Process = true;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (ulong)WindowsMessages.WM_KEYDOWN)
            {
                if (args.WParam == 0x75) // F6 - test shit
                {
                    var nextToBuy = LibraryOfAlexandria.GetNextBuyItemId();
                    Game.PrintChat("nextToBuy: " + ((ItemId)nextToBuy.Value.Id));
                    Game.PrintChat("price: " + nextToBuy.Value.GoldBase);
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

        private void Game_OnGameEnd(EventArgs args)
        {
            GetLogger().Info("OnGameEnd");
            var oThread = new Thread(() =>
            {
                Thread.Sleep(30000);
                Environment.Exit(0);
            });
            oThread.Start();
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

        public void Action_DoFirst(Node node, String stack)
        {
            // update hero info
            foreach (var hero in ProducedContext.ALL_HEROES.Get())
            {
                var heroInfo = GenericContext.GetHeroInfo(hero);
                heroInfo.UpdateDirection();
                heroInfo.UpdateHpHistory();
                if (heroInfo.IsFocusedByTower())
                {
                    var turret = heroInfo.GetFocusedByTower();
                    if (hero.Distance(turret) > GenericContext.TURRET_RANGE ||
                        turret.IsDead)
                    {
                        heroInfo.SetFocusedByTower(null);
                    }
                }
            }

            bTree.OnlyOncePer(500);

            // level up
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

            // buy
            if ((GenericContext.MY_HERO.InShop() || GenericContext.MY_HERO.IsDead))
            {
                var nextToBuy = LibraryOfAlexandria.GetNextBuyItemId();
                var elixir = ItemMapper.GetItem((int)GenericContext.shoppingListElixir);
                // handle initial consumables first
                if (GenericContext.shoppingListConsumables.Length > 0 && GenericContext.MY_HERO.Level == 1)
                {
                    foreach (var consumable in GenericContext.shoppingListConsumables)
                    {
                        var consumableLocal = consumable;
                        var item = ItemMapper.GetItem((int)consumableLocal);
                        if (item.HasValue && GenericContext.MY_HERO.GoldCurrent >= item.Value.GoldBase)
                            GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(), () => GenericContext.MY_HERO.BuyItem(consumableLocal)));
                    }
                    GenericContext.shoppingListConsumables = new ItemId[] { };
                }
                else if (nextToBuy.HasValue && GenericContext.MY_HERO.GoldCurrent >= nextToBuy.Value.GoldBase)
                {
                    if (LibraryOfAlexandria.GetOccuppiedInventorySlots().Count == 7 && nextToBuy.Value.GoldBase == nextToBuy.Value.GoldPrice)
                    {
                        var wardSlot = LibraryOfAlexandria.GetItemSlot(ItemId.Stealth_Ward);
                        var manaPotSlot = LibraryOfAlexandria.GetItemSlot(ItemId.Mana_Potion);
                        var healthPotSlot = LibraryOfAlexandria.GetItemSlot(ItemId.Health_Potion);
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
                            () => GenericContext.MY_HERO.BuyItem((ItemId)nextToBuy.Value.Id)));
                    }
                }
                else if (!nextToBuy.HasValue && LibraryOfAlexandria.GetMinutesSince(GenericContext.lastElixirBought) > 3 && elixir.HasValue &&
                         GenericContext.MY_HERO.GoldCurrent >= elixir.Value.GoldBase)
                {
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(), () => GenericContext.MY_HERO.BuyItem(GenericContext.shoppingListElixir)));
                    GenericContext.lastElixirBought = GenericContext.currentTick;
                }
            }
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
                var wardSlot = LibraryOfAlexandria.GetWardSlot();

                if ((IsWardSpellReady() || wardSlot != null) &&
                    LibraryOfAlexandria.GetSecondsSince(GenericContext.lastWardDropped) > 4)
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
                                    var isAWardNear = LibraryOfAlexandria.IsAWardNear(position);
                                    if (!isAWardNear)
                                    {
                                        if (IsWardSpellReady() && WardSpellIsInRange(position))
                                        {
                                            WardSpellCast(position);
                                        }
                                        else if (wardSlot != null)
                                        {
                                            var wardSlotSpell = new Spell(wardSlot.SpellSlot,
                                                LibraryOfAlexandria.GetHitboxDistance(
                                                    GenericContext.WARD_PLACE_DISTANCE,
                                                    GenericContext.MY_HERO));
                                            if (wardSlotSpell.IsInRange(position.To3D()))
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

        public abstract bool IsWardSpellReady();
        public abstract bool WardSpellIsInRange(Vector2 position);
        public abstract void WardSpellCast(Vector2 position);
        public abstract bool Condition_WillInterruptSelf(Node node, String stack);

        public bool Condition_IsRecalling(Node node, String stack)
        {
            return GenericContext.MY_HERO.IsRecalling();
        }

        public abstract void Action_DoRecklesslyButDontInterruptSelf(Node node, String stack);

        public void Action_RecklessSSAndItems(Node node, String stack)
        {
            if (GenericContext.summonerHeal.IsReady())
            {
                var healTarget = LibraryOfAlexandria.FindAllyInDanger(GenericContext.SUMMONER_HEAL_RANGE);
                if (healTarget != null)
                {
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast(),
                        () => { GenericContext.MY_HERO.Spellbook.CastSpell(GenericContext.summonerHeal, healTarget); }));
                    return;
                }
            }

            var mikaelsSlot = LibraryOfAlexandria.GetItemSlot(ItemId.Mikaels_Crucible);
            if (mikaelsSlot != null && mikaelsSlot.SpellSlot.IsReady())
            {
                var mikaelsTarget = LibraryOfAlexandria.FindAllyInDanger(GenericContext.MIKAELS_RANGE);
                if (mikaelsTarget != null)
                {
                    var mikaelsSpell = new Spell(mikaelsSlot.SpellSlot);
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast(),
                        () => { mikaelsSpell.CastOnUnit(mikaelsTarget); }));
                    return;
                }
            }
        }

        public abstract void Action_DoRecklessly(Node node, String stack);
        public abstract bool Action_RecklessMove(Node node, String stack);

        public bool Condition_DangerCooldown(Node node, String stack)
        {
            return LibraryOfAlexandria.GetSecondsSince(GenericContext.lastDanger) < 3;
        }

        public bool Condition_IsInDanger(Node node, String stack)
        {
            return LibraryOfAlexandria.IsAllyInDanger(GenericContext.MY_HERO);
        }

        public void Action_DoIfInDanger(Node node, String stack)
        {
            if (GenericContext.summonerFlash.IsReady())
            {
                var safeFlashPosition = LibraryOfAlexandria.GetNearestSafeFlashPosition();
                if (safeFlashPosition.HasValue)
                {
                    GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast(),
                        () =>
                        {
                            GenericContext.MY_HERO.Spellbook.CastSpell(GenericContext.summonerFlash,
                                safeFlashPosition.Value);
                        }));
                    return;
                }
            }
        }

        public abstract void Action_DoIfNotInDanger(Node node, String stack);

        public bool Condition_IsUnsafe(Node node, String stack)
        {
            return !ProducedContext.IS_MY_HERO_SAFE.Get();
        }

        public void Action_MoveToSafety(Node node, String stack)
        {
            LibraryOfAlexandria.SafeMoveToDestination(ProducedContext.ALLY_SPAWN.Get().Position);
        }

        public void Action_AutoAttack(Node node, String stack)
        {
        }

        public abstract void Action_DoIfSafe(Node node, String stack);

        public bool Condition_IsRegenerating(Node node, String stack)
        {
            return GenericContext.MY_HERO.InFountain() &&
                   (GenericContext.MY_HERO.Health < GenericContext.MY_HERO.MaxHealth ||
                    GenericContext.MY_HERO.Mana < GenericContext.MY_HERO.MaxMana);
        }

        public bool Condition_IsBuying(Node node, String stack)
        {
            var nextToBuy = LibraryOfAlexandria.GetNextBuyItemId();
            return GenericContext.MY_HERO.InShop() && nextToBuy.HasValue &&
                   GenericContext.MY_HERO.GoldCurrent >= nextToBuy.Value.GoldBase &&
                   !GenericContext.MY_HERO.IsDead;
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (GenericContext.MY_HERO.IsMoving)
            {
                GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new HoldingPosition(),
                    () => { GenericContext.MY_HERO.IssueOrder(GameObjectOrder.HoldPosition, GenericContext.MY_HERO); }));
            }
        }

        public bool Action_GoBuy(Node node, String stack)
        {
            return false;
        }

        public bool Action_MoveToWard(Node node, String stack)
        {
            LibraryOfAlexandria.SafeMoveToDestination(ProducedContext.ENEMY_SPAWN.Get().Position);
            return true;
        }

        public void Action_MoveToSpawn(Node node, String stack)
        {
            LibraryOfAlexandria.SafeMoveToDestination(ProducedContext.ALLY_SPAWN.Get().Position);
        }

        public bool Condition_IsOnSpawn(Node node, String stack)
        {
            return GenericContext.MY_HERO.Distance(ProducedContext.ALLY_SPAWN.Get()) <
                   GenericContext.MY_HERO.BoundingRadius;
        }

        public abstract void Action_Move(Node node, String stack);
    }
}