using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public abstract class GenericBot
    {
        public delegate bool HeroCondition(Obj_AI_Hero hero);

        protected Context context;
        protected ProducedContext producedContext;
        private readonly JSONBTree bTree;
        private readonly Menu config;
        private readonly List<ServerInteraction> serverInteractions = new List<ServerInteraction>();

        protected GenericBot(Context context)
        {
            try
            {
                GetLogger().Info("Constructor");
                var botName = GetType().Name;
                Game.PrintChat(botName + " - Loading");
                config = new Menu(botName, botName, true);
                SetupMenu();

                bTree = new JSONBTree(this, "GenericBot");
                this.context = context;
                SetupContext();

                producedContext = new ProducedContext();
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
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }

        private void StopProcessing()
        {
            CustomEvents.Game.OnGameEnd -= Game_OnGameEnd;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnWndProc -= Game_OnWndProc;
        }

        private void SetupProducedContextCallbacks()
        {
            producedContext.Set(ProducedContextKey.EnemyHeroes, Producer_EnemyHeroes);
            producedContext.Set(ProducedContextKey.AllyHeroes, Producer_AllyHeroes);
        }

        private void SetupContext()
        {
            Assert.True(context.levelSpellsOrder.Count() > 0, "context.levelSpellsOrder is not setup");

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
        }

        private void SetupMenu()
        {
            var configDebugMode = new MenuItem("debugMode", "Debug Mode");
            // default value
            configDebugMode.SetValue(false);
            Logger.debugEnabled = configDebugMode.GetValue<bool>();
            configDebugMode.ValueChanged += ConfigDebugMode_ValueChanged;
            config.AddItem(configDebugMode);

            config.AddToMainMenu();
        }

        private void ConfigDebugMode_ValueChanged(Object obj, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<bool>();
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
            context.currentTick = Environment.TickCount;
            if (context.currentTick - context.lastTickProcessed > context.tickDelay)
            {
                ProcessTick();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ProcessTick()
        {
            try
            {
                if (serverInteractions.Count > 0)
                {
                    GetLogger().Debug("Left over serverInteractions, skipping tick.");
                    return;
                }

                bTree.Tick();

                // process server interactions
                if (serverInteractions.Count > 0)
                {
                    GetLogger().Debug("serverInteractions.Count: " + serverInteractions.Count);
                    var timePerAction = context.tickDelay/(serverInteractions.Count + 1);
                    var delay = 0;
                    foreach (var interaction in serverInteractions)
                    {
                        delay += timePerAction;
                        var interactionLocal = interaction;
                        Utility.DelayAction.Add(delay, () =>
                        {
                            GetLogger().Debug("ServerInteraction at tick: " + context.currentTick);
                            interactionLocal();
                            serverInteractions.Remove(interactionLocal);
                        });
                    }
                }
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

        private Logger GetLogger()
        {
            return Logger.GetLogger(GetType().Name);
        }

        public void Action_LevelSpells(Node node, String stack)
        {
            var abilityLevel = context.myHero.Spellbook.GetSpell(SpellSlot.Q).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.W).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.E).Level +
                               context.myHero.Spellbook.GetSpell(SpellSlot.R).Level;
            if (context.myHero.Level > abilityLevel && abilityLevel < context.levelSpellsOrder.Count())
            {
                serverInteractions.Add(
                    () => { context.myHero.Spellbook.LevelSpell(context.levelSpellsOrder[abilityLevel]); });
            }
        }

        public void Action_Buy(Node node, String stack)
        {
            // if you fail to buy at any point you have a 10 seconds timeout
            if ((context.myHero.InShop() || context.myHero.IsDead) && GetSecondsSince(context.lastFailedBuy) > 10)
            {
                // handle initial consumables first
                if (context.shoppingListConsumables.Count() > 0 && context.myHero.Level == 1)
                {
                    foreach (var consumable in context.shoppingListConsumables)
                    {
                        var consumableLocal = consumable;
                        serverInteractions.Add(() => { if (!context.myHero.BuyItem(consumableLocal)) context.lastFailedBuy = context.currentTick; });
                    }
                    context.shoppingListConsumables = new ItemId[] { };
                }

                var nextToBuy = GetNextBuyItemId();

                if (context.myHero.InventoryItems.Count() == 7)
                {
                    InventorySlot wardSlot = GetItemSlot(ItemId.Stealth_Ward);
                    InventorySlot manaPotSlot = GetItemSlot(ItemId.Mana_Potion);
                    InventorySlot healthPotSlot = GetItemSlot(ItemId.Health_Potion);
                    if (wardSlot != null)
                    {
                        serverInteractions.Add(() => { context.myHero.SellItem(wardSlot.Slot); });
                    }
                    else if (manaPotSlot != null)
                    {
                        serverInteractions.Add(() => { context.myHero.SellItem(manaPotSlot.Slot); });
                    }
                    else if (healthPotSlot != null)
                    {
                        serverInteractions.Add(() => { context.myHero.SellItem(healthPotSlot.Slot); });
                    }
                }
                else
                {

                if (nextToBuy != ItemId.Unknown)
                {
                    serverInteractions.Add(() => { if (!context.myHero.BuyItem(nextToBuy)) context.lastFailedBuy = context.currentTick; });
                }
                else if (GetMinutesSince(context.lastElixirBought) > 3)
                {
                    serverInteractions.Add(() => { if (!context.myHero.BuyItem(context.shoppingListElixir)) context.lastFailedBuy = context.currentTick; });
                    context.lastElixirBought = context.currentTick;
                }
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

        private ItemId GetNextBuyItemId()
        {
            if (context.shoppingList.Count() > 0)
            {
                // expand inventory list
                var expandedInventory = new List<ItemId>();
                foreach (var inventorySlot in context.myHero.InventoryItems)
                {
                    ExpandRecipe(inventorySlot.Id, expandedInventory);
                }

                // reduce expandedInventoryList
                foreach (var itemId in context.shoppingList)
                {
                    if (!expandedInventory.Remove(itemId))
                    {
                        return itemId;
                    }
                }
            }
            return ItemId.Unknown;
        }

        private void ExpandRecipe(ItemId itemId, List<ItemId> into)
        {
            into.Add(itemId);
            var recipe = ItemRecipes.GetRecipe(itemId);
            if (recipe != null)
            {
                into.AddRange(recipe);
                foreach (var id in recipe)
                {
                    ExpandRecipe(id, into);
                }
            }
        }

        public bool Condition_IsZombie(Node node, String stack)
        {
            return context.myHero.IsZombie;
        }

        public virtual void Action_ZombieCast(Node node, String stack)
        {
            // not many bots can do anything with this, so thats why it's not abstract
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
            return context.myHero.IsRecalling();
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
                serverInteractions.Add(
                    () => { context.myHero.IssueOrder(GameObjectOrder.HoldPosition, context.myHero); });
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
            if (destination.IsValid() &&
                (!context.myHero.IsMoving ||
                 destination.Distance(context.myHero.Path.Last(), true) < context.myHero.BoundingRadius))
            {
                serverInteractions.Add(() => { context.myHero.IssueOrder(GameObjectOrder.MoveTo, destination); });
            }
        }

        private List<Obj_AI_Hero> ForeachHeroes(HeroCondition cond)
        {
            var result = new List<Obj_AI_Hero>();
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
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
            return ForeachHeroes(hero => hero.IsValid<Obj_AI_Hero>() && !hero.IsAlly && !hero.IsDead);
        }

        public List<Obj_AI_Hero> Producer_AllyHeroes()
        {
            return ForeachHeroes(hero => hero.IsValid<Obj_AI_Hero>() && hero.IsAlly && !hero.IsMe && !hero.IsDead);
        }

        private delegate void ServerInteraction();
    }
}