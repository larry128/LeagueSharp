using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class KarthusAI : GenericAI
    {
        private bool defileOn;
        private const int Q_RANGE = 875;
        private const int W_RANGE = 1000;
        private const int E_RANGE = 425; 
        private const int R_RANGE = 50000;
        private const String DEFILE_OBJ_NAME = "karthus_base_e_defile.troy";

        public KarthusAI()
        {
            levelSpellsOrder = new[]
            {
                SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W,
                SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R,
                SpellSlot.E, SpellSlot.E
            };

            shoppingList = new[]
            {
                ItemId.Warding_Totem_Trinket, ItemId.Spellthiefs_Edge, ItemId.Frostfang, ItemId.Faerie_Charm,
                ItemId.Faerie_Charm, ItemId.Ruby_Crystal, ItemId.Null_Magic_Mantle, ItemId.Chalice_of_Harmony,
                ItemId.Faerie_Charm, ItemId.Faerie_Charm, ItemId.Forbidden_Idol, ItemId.Amplifying_Tome,
                ItemId.Haunting_Guise, ItemId.Boots_of_Speed, ItemId.Sorcerers_Shoes, ItemId.Amplifying_Tome,
                ItemId.Fiendish_Codex, ItemId.Mikaels_Crucible, ItemId.Blasting_Wand, ItemId.Frost_Queens_Claim,
                ItemId.Amplifying_Tome, ItemId.Void_Staff, ItemId.Amplifying_Tome, ItemId.Liandrys_Torment,
                ItemId.Giants_Belt, ItemId.Rylais_Crystal_Scepter, ItemId.Sorcerers_Shoes_Enchantment_Homeguard
            };

            shoppingListConsumables = new Stack<ItemId>();
            shoppingListConsumables.Push(ItemId.Stealth_Ward);
            shoppingListConsumables.Push(ItemId.Mana_Potion);
            shoppingListConsumables.Push(ItemId.Mana_Potion);

            shoppingListElixir = ItemId.Elixir_of_Sorcery;

            Game.PrintChat(GetType().Name + " - Loaded");
        }

        protected override void GameObject_OnDelete_Hook(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name != null && sender.Name.ToLower().Contains(DEFILE_OBJ_NAME))
            {
                defileOn = false;
            }
        }

        protected override void GameObject_OnCreate_Hook(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name != null && sender.Name.ToLower().Contains(DEFILE_OBJ_NAME))
            {
                defileOn = true;
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast_Hook(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
        }

        public override void Action_ZombieCast(Node node, string stack)
        {
            // WQR
            if (SpellSlot.W.IsReady())
            {
                var wTarget = Targeting.FindPriorityTarget(W_RANGE, true, true);
                if (wTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(0.5f, 50, float.MaxValue, W_RANGE, false, SkillshotType.SkillshotCircle, SpellSlot.W, wTarget, true);
                    return;
                }
            }
            if (SpellSlot.Q.IsReady())
            {
                var qTarget = Targeting.FindPriorityTarget(Q_RANGE, true, true);
                if (qTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(1, 100, float.MaxValue, Q_RANGE, false, SkillshotType.SkillshotCircle, SpellSlot.Q, qTarget, true);
                    return;
                }
            }
            if (SpellSlot.R.IsReady())
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Ulti"),
                    () => { Constants.MY_HERO.Spellbook.CastSpell(SpellSlot.R); }));
                return;
            }
        }

        public override bool IsWardSpellReady()
        {
            // W ready and enough mana and don't see all enemies alive and not fighting
            return false;
        }

        public override float GetWardSpellRange()
        {
            // just some pytghoras
            return 0;
        }

        public override void WardSpellCast(Vector2 position)
        {
            // W
        }

        public override bool Condition_WillInterruptSelf(Node node, string stack)
        {
            // karthus cant really interrupt himself
            return false;
        }

        public override void Action_DoRecklesslyButDontInterruptSelf(Node node, string stack)
        {
            // karthus cant really interrupt himself
        }

        public override void Action_DoRecklessly(Node node, string stack)
        {
            // E
            if (SpellSlot.E.IsReady())
            {
                var lowestHPEnemy = Targeting.FindLowestHpEnemy(true, false, E_RANGE);
                if (lowestHPEnemy != null && !defileOn)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Defile ON"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(SpellSlot.E); }));
                }
                else if (lowestHPEnemy == null && defileOn)
                {
                    Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Defile OFF"),
                        () => { Constants.MY_HERO.Spellbook.CastSpell(SpellSlot.E); }));
                } 
            }
        }

        public override bool Action_RecklessMove(Node node, string stack)
        {
            // go kill someone if you can do it in a few seconds (? flash ? ... if you can get out safe)
            return false;
        }

        public override void Action_DoIfNotInDanger(Node node, string stack)
        {
            // WQ
            if (SpellSlot.W.IsReady())
            {
                var wTarget = Targeting.FindPriorityTarget(W_RANGE, false, false);
                if (wTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(0.5f, 50, float.MaxValue, W_RANGE, false, SkillshotType.SkillshotCircle, SpellSlot.W, wTarget, true);
                    return;
                }
            }
            if (SpellSlot.Q.IsReady())
            {
                var qTarget = Targeting.FindPriorityTarget(Q_RANGE, true, false);
                if (qTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(1, 100, float.MaxValue, Q_RANGE, false, SkillshotType.SkillshotCircle, SpellSlot.Q, qTarget, true);
                    return;
                }
            }
        }

        public override void Action_DoIfSafe(Node node, string stack)
        {
            // R
            if (SpellSlot.R.IsReady())
            {
                var lowestHPEnemy = Targeting.FindLowestHpEnemy(false, true, R_RANGE);
                if (lowestHPEnemy != null)
                {
                    var ultiDmg = GetUltiDmg();
                    float minHealth = 0;
                    var closeAllies = LibraryOfAIexandria.GetUsefulHeroesInRange(lowestHPEnemy.GetTarget(), false,
                        Constants.SCAN_DISTANCE / 2);
                    if (closeAllies.Count > 0 && closeAllies.First().Health + Constants.BASE_PER_LVL_HP > lowestHPEnemy.GetValue() && !LibraryOfAIexandria.IsTypicalHpUnder(closeAllies.First(), Constants.DANGER_UNDER_PERCENT))
                    {
                        minHealth = ultiDmg * 2;
                    }
                    if (lowestHPEnemy.GetTarget().Health > minHealth && lowestHPEnemy.GetTarget().Health < ultiDmg + minHealth)
                    {
                        Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Ulti"),
                            () => { Constants.MY_HERO.Spellbook.CastSpell(SpellSlot.R); }));
                    }
                }
            }

        }

        public override void Action_Move(Node node, string stack)
        {
            // try to harass or
            // delegate to supportmovement or other movement class
        }

        private float GetUltiDmg()
        {
            var ultiDamage = 250;
            var rLevel = Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.R).Level;
            if (rLevel == 2)
            {
                ultiDamage = 400;
            }
            else if (rLevel == 3)
            {
                ultiDamage = 550;
            }
            return (float)(ultiDamage + (Constants.MY_HERO.FlatMagicDamageMod + Constants.MY_HERO.BaseAbilityDamage) * 0.5);
        }
    }
}