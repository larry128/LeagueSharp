using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace najsvan
{
    public static class LibraryOfAIexandria
    {
        public delegate bool Condition<in T>(T hero);

        public static int GetSecondsSince(int actionTookPlaceAt)
        {
            return (Environment.TickCount - actionTookPlaceAt) / 1000;
        }

        public static int GetMinutesSince(int actionTookPlaceAt)
        {
            return (Environment.TickCount - actionTookPlaceAt) / 1000 / 60;
        }

        public static bool IsTypicalHpUnder(Obj_AI_Hero hero, double percent)
        {
            return hero.Health < (Constants.BASE_LVL1_HP + (hero.Level * Constants.BASE_PER_LVL_HP)) * percent;
        }

        public static float GetHitboxDistance(Vector3 position, GameObject obj)
        {
            return position.Distance(obj.Position) + obj.BoundingRadius + 40;
        }

        public static float GetHitboxDistance(Obj_AI_Base obj, Obj_AI_Base obj2)
        {
            return obj.Distance(obj2) + obj.BoundingRadius + obj2.BoundingRadius - 8;
        }

        public static InventorySlot GetItemSlot(ItemId itemId)
        {
            foreach (var inventorySlot in Constants.MY_HERO.InventoryItems)
            {
                if (inventorySlot.Id == itemId)
                {
                    return inventorySlot;
                }
            }
            return null;
        }

        public static ItemData.Item? GetNextBuyItemId(ItemId[] shoppingList)
        {
            if (shoppingList  != null && shoppingList.Length > 0)
            {
                // expand inventory list
                var expandedInventory = new List<int>();
                foreach (var inventorySlot in GetOccuppiedInventorySlots())
                {
                    ExpandRecipe((int)inventorySlot.Id, expandedInventory);
                }

                // reduce expandedInventoryList
                foreach (var itemId in shoppingList)
                {
                    if (!expandedInventory.Remove((int)itemId))
                    {
                        return ItemMapper.GetItem((int)itemId);
                    }
                }
            }
            return null;
        }

        public static bool IsAWardNear(Vector2 position)
        {
            var wardList = ProducedContext.WARDS.Get();
            foreach (var ward in wardList)
            {
                if (LibraryOfAIexandria.GetHitboxDistance(position.To3D(), ward) < Constants.WARD_SIGHT_RADIUS)
                {
                    return true;
                }
            }
            return false;
        }

        public static InventorySlot GetWardSlot()
        {
            InventorySlot ward;

            if ((ward = GetItemSlot(ItemId.Warding_Totem_Trinket)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Greater_Stealth_Totem_Trinket)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Stealth_Ward)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Sightstone)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Ruby_Sightstone)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Vision_Ward)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            if ((ward = GetItemSlot(ItemId.Greater_Vision_Totem_Trinket)) != null && ward.SpellSlot.IsReady())
            {
                return ward;
            }

            return null;
        }

        public static List<InventorySlot> GetOccuppiedInventorySlots()
        {
            var result = new List<InventorySlot>();
            foreach (var inventoryItem in Constants.MY_HERO.InventoryItems)
            {
                if (inventoryItem.IsValidSlot() && !"".Equals(inventoryItem.DisplayName) && !"No Name".Equals(inventoryItem.DisplayName))
                {
                    result.Add(inventoryItem);
                }
            }
            return result;
        }

        public static void ExpandRecipe(int itemId, List<int> into)
        {
            var item = ItemMapper.GetItem(itemId);
            if (item.HasValue)
            {
                if (item.Value.From != null && item.Value.From.Length > 0)
                {
                    var recipe = item.Value.From;
                    foreach (var id in recipe)
                    {
                        ExpandRecipe(id, @into);
                    }
                }
                @into.Add(itemId);
            }
        }

        public static int GetSummonerHealAmount()
        {
            return 75 + 15 * Constants.MY_HERO.Level;
        }

        public static void SafeMoveToDestination(Vector3 lastDestination, Vector3 destination)
        {
            if (destination.IsValid() &&
                (!Constants.MY_HERO.IsMoving ||
                 destination.Distance(lastDestination) > Constants.MY_HERO.BoundingRadius))
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new MovingTo(destination),
                    () => { Constants.MY_HERO.IssueOrder(GameObjectOrder.MoveTo, destination); }));
            }
        }

        public static bool IsAllyInDanger(Obj_AI_Hero ally)
        {
            if (ally.IsDead || ally.InFountain())
            {
                return false;
            }
            var enemies = GetUsefulHeroesInRange(ally, false, Constants.SCAN_DISTANCE / 2);
            var dangerHp = IsTypicalHpUnder(ally, Constants.DANGER_UNDER_PERCENT);
            var fearHp = IsTypicalHpUnder(ally, Constants.FEAR_UNDER_PERCENT);
            var allyInfo = Constants.GetHeroInfo(ally);
            var nearestEnemyTurret = GetNearestTower(ally, false);

            return
                (
                    dangerHp
                    &&
                    (
                        enemies.Count > 0
                        ||
                        LibraryOfAIexandria.GetHitboxDistance(ally, nearestEnemyTurret) < Constants.TURRET_RANGE
                        ||
                        allyInfo.GetHpLost() > 0.3 * ally.MaxHealth
                        )
                    )
                ||
                (
                    fearHp
                    &&
                    allyInfo.IsFocusedByTower()
                    );
        }

        public static bool IsHeroSafe(Obj_AI_Hero hero)
        {
            var hisSpawn = hero.Team == Constants.ALLY_TEAM
                ? ProducedContext.ALLY_SPAWN
                : ProducedContext.ENEMY_SPAWN;
            if (hero.IsZombie || hero.IsDead || hero.Distance(hisSpawn.Get()) < hero.BoundingRadius)
            {
                return true;
            }
            return false;
        }

        public static bool BBetweenAandC(Obj_AI_Base aObject, Obj_AI_Base bObject, Obj_AI_Base cObject)
        {
            Assert.False(aObject.NetworkId == bObject.NetworkId, "aObject.NetworkId == bObject.NetworkId");
            Assert.False(cObject.NetworkId == bObject.NetworkId, "cObject.NetworkId == bObject.NetworkId");
            Assert.False(aObject.NetworkId == cObject.NetworkId, "aObject.NetworkId == cObject.NetworkId");
            return BBetweenAandC(aObject.Position, bObject.Position, cObject.Position);
        }

        public static bool BBetweenAandC(Vector3 aObject, Vector3 bObject, Vector3 cObject)
        {
            var aCDistance = aObject.Distance(cObject);
            return cObject.Distance(bObject) < aCDistance && cObject.Distance(aObject) < aCDistance;
        }

        public static List<Obj_AI_Hero> GetUsefulHeroesInRange(Obj_AI_Hero hero, bool alliedToHero, int range)
        {
            var result = new List<Obj_AI_Hero>();
            var teamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = teamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            heroes.ForEach(other =>
            {
                if (LibraryOfAIexandria.GetHitboxDistance(hero, other) < range && other != hero &&
                    !other.IsStunned && !other.IsPacified)
                {
                    result.Add(other);
                }
            });
            return result;
        }

        public static List<Obj_AI_Hero> GetHeroesInRange(Obj_AI_Hero hero, bool alliedToHero, int range)
        {
            var result = new List<Obj_AI_Hero>();
            var teamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = teamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            heroes.ForEach(other =>
            {
                if (LibraryOfAIexandria.GetHitboxDistance(hero, other) < range && other != hero)
                {
                    result.Add(other);
                }
            });
            return result;
        }

        public static Vector3? GetNearestSafePosition()
        {
            return ProducedContext.ALLY_SPAWN.Get().Position;
        }

        public static Vector3? GetNearestSafeFlashPosition()
        {
            return GetNearestSafePosition();
        }

        public static List<T> ProcessEachGameObject<T>(Condition<T> cond) where T : GameObject, new()
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

        public static Obj_AI_Turret GetNearestTower(Obj_AI_Hero hero, bool alliedToHero)
        {
            Obj_AI_Turret result = null;
            var lowestDistance = Single.MaxValue;
            var turretTeamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var turrets = turretTeamCondition ? ProducedContext.ALLY_TURRETS.Get() : ProducedContext.ENEMY_TURRETS.Get();
            foreach (var turret in turrets)
            {
                var distance = LibraryOfAIexandria.GetHitboxDistance(turret, hero);
                if (distance < lowestDistance)
                {
                    result = turret;
                    lowestDistance = distance;
                }
            }
            return result;
        }

        public static float GetImpact(Obj_AI_Hero hero)
        {
            if (hero.FlatMagicDamageMod > hero.FlatAttackRangeMod)
            {
                return hero.BaseAbilityDamage + hero.FlatMagicDamageMod;
            }
            else
            {
                return (hero.BaseAttackDamage + hero.FlatAttackRangeMod) * hero.AttackSpeedMod;
            }

        }

        public static void PredictedSkillshot(float delay, float radius, float speed, float range, bool checkCollision, SkillshotType skillshotType, SpellSlot skillshot, Obj_AI_Hero target, bool isAoe)
        {
            PredictionOutput prediction = Prediction.GetPrediction(
                    new PredictionInput
                    {
                        Unit = target,
                        Delay = delay,
                        Radius = radius,
                        Speed = speed,
                        From = Constants.MY_HERO.ServerPosition,
                        Range = range,
                        Collision = checkCollision,
                        Type = skillshotType,
                        RangeCheckFrom = Constants.MY_HERO.ServerPosition,
                        Aoe = isAoe
                    });
            if (prediction.Hitchance.Equals(HitChance.High) || prediction.Hitchance.Equals(HitChance.VeryHigh) ||
                prediction.Hitchance.Equals(HitChance.Immobile) || prediction.Hitchance.Equals(HitChance.Medium))
            {
                Constants.SERVER_INTERACTIONS.Add(new ServerInteraction(new SpellCast(),
                    () => { Constants.MY_HERO.Spellbook.CastSpell(skillshot, prediction.CastPosition); }));
            }
        }
    }
}