using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class KarthusAI : GenericAI
    {
        private const int Q_RANGE = 875;
        private const int W_RANGE = 1000;
        private const int E_RANGE = 425;
        private const int R_RANGE = 50000;
        private const String DEFILE_OBJ_NAME = "karthus_base_e_defile.troy";
        private bool defileOn;

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
            if (sender != null && sender.IsValid && sender.Name != null &&
                sender.Name.ToLower().Contains(DEFILE_OBJ_NAME))
            {
                defileOn = false;
            }
        }

        protected override void GameObject_OnCreate_Hook(GameObject sender, EventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Name != null &&
                sender.Name.ToLower().Contains(DEFILE_OBJ_NAME))
            {
                defileOn = true;
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast_Hook(Obj_AI_Base unit,
            GameObjectProcessSpellCastEventArgs args)
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
                    LibraryOfAIexandria.PredictedSkillshot(0.5f, 50, float.MaxValue, W_RANGE, false,
                        SkillshotType.SkillshotCircle, SpellSlot.W, wTarget, true);
                    return;
                }
            }
            if (SpellSlot.Q.IsReady())
            {
                var qTarget = Targeting.FindPriorityTarget(Q_RANGE, true, true);
                if (qTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(1, 100, float.MaxValue, Q_RANGE, false,
                        SkillshotType.SkillshotCircle, SpellSlot.Q, qTarget, true);
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
            return SpellSlot.W.IsReady() &&
                   (Constants.MY_HERO.Mana / Constants.MY_HERO.MaxMana > 0.5 ||
                    LibraryOfAIexandria.GetHeroesInRange(Constants.MY_HERO, false, Constants.SCAN_DISTANCE).Count == 0);
        }

        public override float GetWardSpellRange()
        {
            // just some pytghoras
            var halfLegth = GetGateHalfLength();
            double wardSpellRange = Math.Sqrt(halfLegth * halfLegth + W_RANGE * W_RANGE);
            return (float)wardSpellRange;
        }

        public override void WardSpellCast(Vector2 position)
        {
            // W
            var castPosition = GetCirlesCommonPointApprox(Constants.MY_HERO.ServerPosition.To2D(), position, W_RANGE, GetGateHalfLength());
            if (!castPosition.Equals(Vector2.Zero))
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast("Warding with W"),
                    () => { Constants.MY_HERO.Spellbook.CastSpell(SpellSlot.W, castPosition.To3D()); }));
            }
        }

        public override bool Condition_WillInterruptSelf(Node node, string stack)
        {
            return false;
        }

        public override void Action_DoRecklesslyNoInterrupt(Node node, string stack)
        {
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
                    LibraryOfAIexandria.PredictedSkillshot(0.5f, 50, float.MaxValue, W_RANGE, false,
                        SkillshotType.SkillshotCircle, SpellSlot.W, wTarget, true);
                    return;
                }
            }
            if (SpellSlot.Q.IsReady())
            {
                var qTarget = Targeting.FindPriorityTarget(Q_RANGE, true, false);
                if (qTarget != null)
                {
                    LibraryOfAIexandria.PredictedSkillshot(1, 100, float.MaxValue, Q_RANGE, false,
                        SkillshotType.SkillshotCircle, SpellSlot.Q, qTarget, true);
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
                    if (closeAllies.Count > 0 &&
                        closeAllies.First().Health + Constants.BASE_PER_LVL_HP > lowestHPEnemy.GetValue() &&
                        !LibraryOfAIexandria.IsTypicalHpUnder(closeAllies.First(), Constants.DANGER_UNDER_PERCENT))
                    {
                        minHealth = ultiDmg * 2;
                    }
                    if (Constants.MY_HERO.Distance(lowestHPEnemy.GetTarget()) > Constants.SCAN_DISTANCE/2 && lowestHPEnemy.GetTarget().Health > minHealth &&
                        lowestHPEnemy.GetTarget().Health < ultiDmg + minHealth)
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
            return
                (float)(ultiDamage + (Constants.MY_HERO.FlatMagicDamageMod + Constants.MY_HERO.BaseAbilityDamage) * 0.5);
        }

        private static Vector2 GetCirlesCommonPointApprox(Vector2 centerA, Vector2 centerB, float radiusA, float radiusB, float tolerance = 40)
        {
            for (int angle = 1; angle < 361; angle++)
            {
                var xOnCircleA = centerA.X + (radiusA * Math.Cos((Math.PI / 180) * angle));
                var yOnCircleA = centerA.Y + (radiusA * Math.Sin((Math.PI / 180) * angle));

                var xClippedToMesh = (float)Math.Floor(xOnCircleA);
                var yClippedToMesh = (float)Math.Floor(yOnCircleA);

                var potentialVector = new Vector2(xClippedToMesh, yClippedToMesh);
                if (potentialVector.Distance(centerB) < radiusB + tolerance &&
                    potentialVector.Distance(centerB) > radiusB - tolerance)
                {
                    return potentialVector;
                }
            }
            return Vector2.Zero;
        }

        private static int GetGateHalfLength()
        {
            var wLevel = Constants.MY_HERO.Spellbook.GetSpell(SpellSlot.W).Level;
            int length = 700 + wLevel * 100;
            int halfLegth = length / 2;
            return halfLegth;
        }
    }
}