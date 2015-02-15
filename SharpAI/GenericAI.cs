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
    public abstract class GenericAI
    {
        protected int lastDanger = 0;
        protected Vector3 lastDestination = Vector3.Zero;
        protected int lastElixirBought;
        protected int lastTickProcessed;
        protected int lastWardDropped = 0;
        protected SpellSlot[] levelSpellsOrder;
        protected ItemId[] shoppingList;
        protected Stack<ItemId> shoppingListConsumables;
        protected ItemId shoppingListElixir;
        private readonly JSONBTree bTree;
        private readonly Menu config;

        protected GenericAI()
        {
            try
            {
                Constants.LOG.Info("Constructor");
                var botName = GetType().Name;
                Game.PrintChat(botName + " - Loading");
                bTree = new JSONBTree(this, "GenericBot");
                config = new Menu(botName, botName, true);
                SetupMenu();
                Utility.DelayAction.Add(5000, StartProcessing);
            }
            catch (Exception e)
            {
                Game.PrintChat(e.GetType().Name + " : " + e.Message);
                Constants.LOG.Error(e.ToString());
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
            if (sender != null && sender.IsValid && sender.Name != null &&
                sender.Name.ToLower().Contains(Constants.TARGETED_BY_TOWER_OBJ_NAME))
            {
                Constants.GetHeroInfo(Constants.MY_HERO)
                    .SetFocusedByTower(LibraryOfAIexandria.GetNearestTower(Constants.MY_HERO, false));
            }
            GameObject_OnDelete_Hook(sender, args);
        }

        protected abstract void GameObject_OnDelete_Hook(GameObject sender, EventArgs args);

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name != null &&
                sender.Name.ToLower().Contains(Constants.TARGETED_BY_TOWER_OBJ_NAME))
            {
                Constants.GetHeroInfo(Constants.MY_HERO).SetFocusedByTower(null);
            }
            GameObject_OnCreate_Hook(sender, args);
        }

        protected abstract void GameObject_OnCreate_Hook(GameObject sender, EventArgs args);

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Constants.GetHeroInfo(Constants.MY_HERO).GetDirection().IsValid())
            {
                const int textOffsetX = 60;
                const int textOffsetY = 100;

                var worldDirection = Constants.GetHeroInfo(Constants.MY_HERO).GetDirection();
                Drawing.DrawText(textOffsetX, textOffsetY, Color.Yellow, worldDirection.X + "x" + worldDirection.Y);

                var sdLength = Math.Sqrt(worldDirection.X*worldDirection.X + worldDirection.Y*worldDirection.Y);
                const int length = 600;
                if (sdLength != 0)
                {
                    var coefficient = length/(float) sdLength;
                    worldDirection *= coefficient;
                    var start = Drawing.WorldToScreen(Constants.MY_HERO.Position);
                    var end = Drawing.WorldToScreen(Constants.MY_HERO.Position + worldDirection.To3D());
                    Drawing.DrawLine(start, end, 2, Color.Blue);
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsValid<Obj_AI_Hero>() && args.Target.IsValid && args.Target.Position.IsValid())
            {
                var hero = unit as Obj_AI_Hero;
                if (hero != null)
                {
                    Constants.GetHeroInfo(hero).SetDirection(hero.Position.To2D(), args.Target.Position.To2D());

                    var spell = args.SData;
                    if (hero.IsMe && spell != null && spell.IsAutoAttack())
                    {
                    }
                }
            }

            if (unit.IsValid<Obj_AI_Turret>() && args.Target.IsValid<Obj_AI_Hero>())
            {
                Constants.GetHeroInfo((Obj_AI_Hero) args.Target).SetFocusedByTower((Obj_AI_Turret) unit);
            }

            Obj_AI_Base_OnProcessSpellCast_Hook(unit, args);
        }

        protected abstract void Obj_AI_Base_OnProcessSpellCast_Hook(Obj_AI_Base unit,
            GameObjectProcessSpellCastEventArgs args);

        private void SetupMenu()
        {
            var configBotDebug = new MenuItem("botDebug", GetType().Name + " Debug");
            // default value
            configBotDebug.SetValue(false);
            Constants.LOG.debugEnabled = configBotDebug.GetValue<bool>();
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

            Constants.LOG.debugEnabled = newValue;
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
            if (args.Msg == (ulong) WindowsMessages.WM_KEYDOWN)
            {
                if (args.WParam.Equals(0x75)) // F6 - test shit
                {
                    var nextToBuy = LibraryOfAIexandria.GetNextBuyItemId(shoppingList);
                    Game.PrintChat("nextToBuy: " + ((ItemId) nextToBuy.Value.Id));
                    Game.PrintChat("price: " + nextToBuy.Value.GoldBase);
                    Game.PrintChat("GetOccuppiedInventorySlots: " +
                                   LibraryOfAIexandria.GetOccuppiedInventorySlots().Count);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Environment.TickCount - lastTickProcessed > Constants.TICK_DELAY + Game.Ping)
            {
                if (Constants.SERVER_INTERACTIONS.Count > 0)
                {
                    Constants.LOG
                        .Error(
                            "Not all serverInteractions processed, pushing tick 50 * GenericAIContext.serverInteractions.Count millis.");
                    lastTickProcessed += 50*Constants.SERVER_INTERACTIONS.Count;
                    return;
                }

                ProcessTick();
            }
        }

        private void Game_OnGameEnd(EventArgs args)
        {
            Constants.LOG.Info("OnGameEnd");
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
                Constants.LOG.Error(e.ToString());
                StopProcessing();
            }

            ProducedContext.Clear();
            lastTickProcessed = Environment.TickCount;
        }

        private void ProcessServerInteractions()
        {
            if (Constants.SERVER_INTERACTIONS.Count > 0)
            {
                Constants.LOG
                    .Debug("SERVER_INTERACTIONS.Count: " + Constants.SERVER_INTERACTIONS.Count);
                var timePerAction = Constants.TICK_DELAY/(Constants.SERVER_INTERACTIONS.Count + 1);
                var delay = 0;
                foreach (var interaction in Constants.SERVER_INTERACTIONS)
                {
                    delay += timePerAction;
                    var interactionLocal = interaction;
                    Utility.DelayAction.Add(delay, () =>
                    {
                        try
                        {
                            Constants.LOG.Debug(interactionLocal.request + " at tick: " + Environment.TickCount);
                            interactionLocal.serverAction();
                            var movingTo = interactionLocal.request as MovingTo;
                            if (movingTo != null)
                            {
                                lastDestination = movingTo.destination;
                            }
                            var holdingPosition = interactionLocal.request as HoldingPosition;
                            if (holdingPosition != null)
                            {
                                lastDestination = Vector3.Zero;
                            }
                        }
                        finally
                        {
                            Constants.SERVER_INTERACTIONS.Remove(interactionLocal);
                        }
                    });
                }
            }
        }

        public void Action_LevelUp(Node node, String stack)
        {
            bTree.OnlyOncePer(500);

            // level up
            var abilityLevel = Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.Q).Level +
                               Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.W).Level +
                               Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.E).Level +
                               Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.R).Level;
            if (Constants.MY_HERO.Level > abilityLevel && abilityLevel < levelSpellsOrder.Length)
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellLeveledUp(),
                    () => { Constants.MY_HERO.Spellbook.LevelSpell(levelSpellsOrder[abilityLevel]); }));
            }
        }

        public void Action_Buy(Node node, String stack)
        {
            bTree.OnlyOncePer(500);

            // buy
            if ((Constants.MY_HERO.InShop() || Constants.MY_HERO.IsDead))
            {
                var nextToBuy = LibraryOfAIexandria.GetNextBuyItemId(shoppingList);
                var elixir = ItemMapper.GetItem((int) shoppingListElixir);
                // handle initial consumables first
                if (shoppingListConsumables.Count > 0 && Constants.MY_HERO.Level == 1)
                {
                    var consumable = shoppingListConsumables.Pop();
                    var item = ItemMapper.GetItem((int) consumable);
                    if (item.HasValue && Constants.MY_HERO.GoldCurrent >= item.Value.GoldBase)
                        Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                            () => Constants.MY_HERO.BuyItem(consumable)));
                    return;
                }
                if (nextToBuy.HasValue && Constants.MY_HERO.GoldCurrent >= nextToBuy.Value.GoldBase)
                {
                    if (LibraryOfAIexandria.GetOccuppiedInventorySlots().Count == 7 &&
                        nextToBuy.Value.GoldBase == nextToBuy.Value.GoldPrice)
                    {
                        var wardSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Stealth_Ward);
                        var manaPotSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Mana_Potion);
                        var healthPotSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Health_Potion);
                        if (manaPotSlot != null)
                        {
                            Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { Constants.MY_HERO.SellItem(manaPotSlot.Slot); }));
                        }
                        else if (healthPotSlot != null)
                        {
                            Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { Constants.MY_HERO.SellItem(healthPotSlot.Slot); }));
                        }
                        else if (wardSlot != null)
                        {
                            Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SellItem(),
                                () => { Constants.MY_HERO.SellItem(wardSlot.Slot); }));
                        }
                    }
                    else
                    {
                        Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                            () => Constants.MY_HERO.BuyItem((ItemId) nextToBuy.Value.Id)));
                    }
                }
                else if (!nextToBuy.HasValue && LibraryOfAIexandria.GetMinutesSince(lastElixirBought) > 3 &&
                         elixir.HasValue &&
                         Constants.MY_HERO.GoldCurrent >= elixir.Value.GoldBase)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new BuyItem(),
                        () => Constants.MY_HERO.BuyItem(shoppingListElixir)));
                    lastElixirBought = Environment.TickCount;
                }
            }
        }

        public void Action_Update(Node node, String stack)
        {
            // update hero info
            foreach (var hero in ProducedContext.ALL_HEROES.Get())
            {
                var heroInfo = Constants.GetHeroInfo(hero);
                heroInfo.UpdateDirection();
                heroInfo.UpdateHpHistory();
                if (heroInfo.IsFocusedByTower())
                {
                    var turret = heroInfo.GetFocusedByTower();
                    if (!(turret.Health > 0) || !turret.IsValid || !turret.IsValid<Obj_AI_Turret>() || turret.IsDead ||
                        LibraryOfAIexandria.GetHitboxDistance(hero, turret) > Constants.TURRET_RANGE)
                    {
                        heroInfo.SetFocusedByTower(null);
                    }
                }
            }
        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return Constants.MY_HERO.IsZombie;
        }

        public abstract void Action_ZombieCast(Node node, String stack);

        public bool Condition_IsDead(Node node, String stack)
        {
            return Constants.MY_HERO.IsDead;
        }

        public void Action_DropWard(Node node, String stack)
        {
            PossibleToWard((position, wardSlot) =>
            {
                if (IsWardSpellReady() && Constants.MY_HERO.Distance(position) < GetWardSpellRange())
                {
                    WardSpellCast(position);
                }
                else if (wardSlot != null && wardSlot.SpellSlot.IsReady())
                {
                    if (LibraryOfAIexandria.GetHitboxDistance(
                        position.To3D(),
                        Constants.MY_HERO) < Constants.WARD_PLACE_DISTANCE)
                    {
                        Constants.SERVER_INTERACTIONS.Add(
                            new ServerInteraction(new WardUsed(wardSlot),
                                () => Constants.MY_HERO.Spellbook.CastSpell(wardSlot.SpellSlot, position.To3D())));
                    }
                }
            });
        }

        private void PossibleToWard(WardAction action)
        {
            if (Constants.MY_HERO.Level > 1)
            {
                var wardSlot = LibraryOfAIexandria.GetWardSlot();

                if ((IsWardSpellReady() || wardSlot != null) &&
                    LibraryOfAIexandria.GetSecondsSince(lastWardDropped) > 4)
                {
                    var keys = Constants.WARD_SPOTS.Keys;
                    foreach (var key in keys)
                    {
                        if (key == GameObjectTeam.Neutral || key == Constants.MY_HERO.Team)
                        {
                            List<WardSpot> values;
                            if (Constants.WARD_SPOTS.TryGetValue(key, out values))
                            {
                                foreach (var wardSpot in values)
                                {
                                    var position = wardSpot.GetPosition();
                                    var isAWardNear = LibraryOfAIexandria.IsAWardNear(position);
                                    if (!isAWardNear)
                                    {
                                        action(position, wardSlot);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public abstract bool IsWardSpellReady();
        public abstract float GetWardSpellRange();
        public abstract void WardSpellCast(Vector2 position);
        public abstract bool Condition_WillInterruptSelf(Node node, String stack);

        public bool Condition_WillInterruptAA(Node node, String stack)
        {
            return !Orbwalking.CanMove(90);
        }

        public bool Condition_IsRecalling(Node node, String stack)
        {
            return Constants.MY_HERO.IsRecalling();
        }

        public abstract void Action_DoRecklesslyNoInterrupt(Node node, String stack);

        public void Action_RecklessSSAndItems(Node node, String stack)
        {
            // heal
            if (Constants.SUMMONER_HEAL.IsReady())
            {
                var healTarget = Targeting.FindAllyInDanger(Constants.SUMMONER_HEAL_RANGE);
                if (healTarget != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Summoner Heal"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(Constants.SUMMONER_HEAL, healTarget); }));
                    return;
                }
            }

            // mikaels
            var mikaelsSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Mikaels_Crucible);
            if (mikaelsSlot != null && mikaelsSlot.SpellSlot.IsReady())
            {
                var mikaelsTarget = Targeting.FindAllyInDanger(Constants.MIKAELS_RANGE);
                if (mikaelsTarget != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Mikaels"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(mikaelsSlot.SpellSlot, mikaelsTarget); }));
                    return;
                }
            }

            // ignite
            if (Constants.SUMMONER_IGNITE.IsReady())
            {
                var igniteTarget = Targeting.FindPriorityTarget(Constants.SUMMONER_IGNITE_RANGE, false, true);
                if (igniteTarget != null)
                {
                    var countOfAlliesNear = LibraryOfAIexandria.GetUsefulHeroesInRange(igniteTarget, false,
                        Constants.SCAN_DISTANCE/2).Count;
                    if (
                        (
                            LibraryOfAIexandria.IsTypicalHpUnder(igniteTarget, 0.6)
                            &&
                            (countOfAlliesNear > 1 || LibraryOfAIexandria.IsTypicalHpUnder(igniteTarget, 0.2))
                            &&
                            (countOfAlliesNear < 4 || !LibraryOfAIexandria.IsTypicalHpUnder(igniteTarget, 0.5))
                            )
                        ||
                        LibraryOfAIexandria.IsTypicalHpUnder(Constants.MY_HERO, Constants.DANGER_UNDER_PERCENT)
                        )
                    {
                        Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Summoner Ignite"),
                            () => { Constants.MY_HERO.Spellbook.CastSpell(Constants.SUMMONER_IGNITE, igniteTarget); }));
                        return;
                    }
                }
            }

            // queens
            var queensSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Frost_Queens_Claim);
            if (queensSlot != null && queensSlot.SpellSlot.IsReady())
            {
                var queensTarget = Targeting.FindPriorityTarget(Constants.QUEENS_RANGE, false, true);
                if (queensTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(0, 100, 1200, Constants.QUEENS_RANGE, false,
                        SkillshotType.SkillshotCircle, queensSlot.SpellSlot, queensTarget, true);
                    return;
                }
            }
            // locket
            var locketSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Locket_of_the_Iron_Solari);
            if (locketSlot != null && locketSlot.SpellSlot.IsReady())
            {
                var locketTarget = Targeting.FindAllyInDanger(Constants.LOCKET_RANGE);
                if (locketTarget != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Locket"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(locketSlot.SpellSlot); }));
                    return;
                }
            }
            // talisman
            var talismanSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Talisman_of_Ascension);
            if (talismanSlot != null && talismanSlot.SpellSlot.IsReady())
            {
                var talismanTarget = Targeting.FindAllyInDanger(Constants.TALISMAN_RANGE);
                if (talismanTarget == null)
                {
                    Targeting.FindPriorityTarget(Constants.TALISMAN_RANGE, false, true);
                }
                if (talismanTarget != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Talisman"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(talismanSlot.SpellSlot); }));
                    return;
                }
            }
            // twins
            var twinsSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Twin_Shadows);
            if (twinsSlot == null)
            {
                twinsSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Twin_Shadows_3290);
            }
            if (twinsSlot != null && twinsSlot.SpellSlot.IsReady())
            {
                var twinsTarget = Targeting.FindPriorityTarget(Constants.SCAN_DISTANCE, false, true);
                if (twinsTarget != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Twins"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(twinsSlot.SpellSlot); }));
                    return;
                }
            }
        }

        public abstract void Action_DoRecklessly(Node node, String stack);
        public abstract bool Action_RecklessMove(Node node, String stack);

        public bool Condition_DangerCooldown(Node node, String stack)
        {
            return LibraryOfAIexandria.GetSecondsSince(lastDanger) < Constants.DANGER_COOLDOWN;
        }

        public bool Condition_IsInDanger(Node node, String stack)
        {
            return LibraryOfAIexandria.IsAllyInDanger(Constants.MY_HERO);
        }

        public void Action_DoIfInDanger(Node node, String stack)
        {
            // flash
            if (Constants.SUMMONER_FLASH.IsReady())
            {
                var safeFlashPosition = LibraryOfAIexandria.GetNearestSafeFlashPosition();
                if (safeFlashPosition.HasValue)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Summoner Flash"),
                        () =>
                        {
                            Constants.MY_HERO.Spellbook.CastSpell(Constants.SUMMONER_FLASH,
                                safeFlashPosition.Value);
                        }));
                    return;
                }
            }
            // seraphs
            var seraphsSlot = LibraryOfAIexandria.GetItemSlot(ItemId.Archangels_Staff);
            if (seraphsSlot != null && seraphsSlot.SpellSlot.IsReady())
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Seraphs"),
                    () => { Constants.MY_HERO.Spellbook.CastSpell(seraphsSlot.SpellSlot); }));
            }
        }

        public abstract void Action_DoIfNotInDanger(Node node, String stack);

        public bool Condition_IsUnsafe(Node node, String stack)
        {
            return false; //!LibraryOfAIexandria.IsHeroSafe(Constants.MY_HERO);
        }

        public void Action_MoveToSafety(Node node, String stack)
        {
            //LibraryOfAlexandria.GetNearestSafePosition();
        }

        public void Action_AutoAttack(Node node, String stack)
        {
            if (Orbwalking.CanAttack())
            {
                var target = Targeting.FindPriorityTarget(Constants.MY_HERO.AttackRange, true, true);
                if (target != null)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new AutoAttack(),
                        () => { Constants.MY_HERO.IssueOrder(GameObjectOrder.AttackUnit, target); }));
                }
            }
        }

        public abstract void Action_DoIfSafe(Node node, String stack);

        public bool Condition_IsRegenerating(Node node, String stack)
        {
            return Constants.MY_HERO.InFountain() &&
                   (Constants.MY_HERO.Health < Constants.MY_HERO.MaxHealth ||
                    Constants.MY_HERO.Mana < Constants.MY_HERO.MaxMana);
        }

        public bool Condition_IsBuying(Node node, String stack)
        {
            var nextToBuy = LibraryOfAIexandria.GetNextBuyItemId(shoppingList);
            return Constants.MY_HERO.InShop() && nextToBuy.HasValue &&
                   Constants.MY_HERO.GoldCurrent >= nextToBuy.Value.GoldBase &&
                   !Constants.MY_HERO.IsDead;
        }

        public void Action_StopMoving(Node node, String stack)
        {
            if (Constants.MY_HERO.IsMoving)
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new HoldingPosition(),
                    () => { Constants.MY_HERO.IssueOrder(GameObjectOrder.HoldPosition, Constants.MY_HERO); }));
            }
        }

        public bool Action_GoBuy(Node node, String stack)
        {
            // recall or walk
            return false;
        }

        public bool Action_MoveToWard(Node node, String stack)
        {
            //LibraryOfAlexandria.SafeMoveToDestination(ProducedContext.ENEMY_SPAWN.Get().Position);
            return false;
        }

        public void Action_MoveToSpawn(Node node, String stack)
        {
            LibraryOfAIexandria.SafeMoveToDestination(lastDestination, ProducedContext.ALLY_SPAWN.Get().Position);
        }

        public bool Condition_IsOnSpawn(Node node, String stack)
        {
            return Constants.MY_HERO.Distance(ProducedContext.ALLY_SPAWN.Get()) <
                   Constants.MY_HERO.BoundingRadius;
        }

        public abstract void Action_Move(Node node, String stack);

        private delegate void WardAction(Vector2 position, InventorySlot wardSlot);
    }
}