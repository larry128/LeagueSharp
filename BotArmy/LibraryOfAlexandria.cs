﻿using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public static class LibraryOfAlexandria
    {
        public delegate bool Condition<in T>(T hero);

        public static int GetSecondsSince(int actionTookPlaceAt)
        {
            return (GenericContext.currentTick - actionTookPlaceAt) / 1000;
        }

        public static int GetMinutesSince(int actionTookPlaceAt)
        {
            return (GenericContext.currentTick - actionTookPlaceAt) / 1000 / 60;
        }

        public static double GetTypicalHp(int level, double percent)
        {
            return (GenericContext.BASE_LVL1_HP + (level * GenericContext.BASE_PER_LVL_HP)) * percent;
        }

        public static float GetHitboxDistance(float distance, GameObject obj)
        {
            return distance + obj.BoundingRadius + 40;
        }

        public static float GetHitboxDistance(Obj_AI_Base obj, Obj_AI_Base obj2)
        {
            return obj.Distance(obj2) + obj.BoundingRadius + obj2.BoundingRadius - 8;
        }

        public static InventorySlot GetItemSlot(ItemId itemId)
        {
            foreach (var inventorySlot in GenericContext.MY_HERO.InventoryItems)
            {
                if (inventorySlot.Id == itemId)
                {
                    return inventorySlot;
                }
            }
            return null;
        }

        public static LeagueSharp.Common.Data.ItemData.Item? GetNextBuyItemId()
        {
            if (GenericContext.shoppingList.Length > 0)
            {
                // expand inventory list
                var expandedInventory = new List<int>();
                foreach (var inventorySlot in GetOccuppiedInventorySlots())
                {
                    ExpandRecipe((int)inventorySlot.Id, expandedInventory);
                }

                // reduce expandedInventoryList
                foreach (var itemId in GenericContext.shoppingList)
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
                if (position.Distance(ward.Position) < GenericContext.WARD_SIGHT_RADIUS)
                {
                    return true;
                }
            }
            return false;
        }

        public static InventorySlot GetWardSlot()
        {
            InventorySlot ward;
            var wardsUsed = new List<InventorySlot>();

            GenericContext.SERVER_INTERACTIONS.ForEach(interaction =>
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

        public static List<InventorySlot> GetOccuppiedInventorySlots()
        {
            var result = new List<InventorySlot>();
            foreach (var inventoryItem in GenericContext.MY_HERO.InventoryItems)
            {
                if (!"".Equals(inventoryItem.DisplayName))
                {
                    result.Add(inventoryItem);
                }
            }
            return result;
        }

        public static void ExpandRecipe(int itemId, List<int> into)
        {
            @into.Add(itemId);
            var item = ItemMapper.GetItem(itemId);
            if (item.HasValue && item.Value.From != null && item.Value.From.Length > 0)
            {
                var recipe = item.Value.From;
                @into.AddRange(recipe);
                foreach (var id in recipe)
                {
                    ExpandRecipe(id, @into);
                }
            }
        }

        public static int GetSummonerHealAmount()
        {
            return 75 + 15 * GenericContext.MY_HERO.Level;
        }

        public static void SafeMoveToDestination(Vector3 destination)
        {
            if (destination.IsValid() &&
                (!GenericContext.MY_HERO.IsMoving ||
                 destination.Distance(GenericContext.lastDestination) > GenericContext.MY_HERO.BoundingRadius))
            {
                GenericContext.SERVER_INTERACTIONS.Add(new ServerInteraction(new MovingTo(destination),
                    () => { GenericContext.MY_HERO.IssueOrder(GameObjectOrder.MoveTo, destination); }));
            }
        }

        public static Obj_AI_Hero FindAllyInDanger(float range)
        {
            Obj_AI_Hero lowestHpAlly = null;
            var lowestHp = float.MaxValue;

            ProducedContext.ALL_ALLIES.Get().ForEach(ally =>
            {
                if (GetHitboxDistance(GenericContext.MY_HERO, ally) < range && !ally.InFountain() && !ally.IsDead)
                {
                    if (ally.Health < lowestHp && ally.Health > 1 && IsAllyInDanger(ally))
                    {
                        lowestHpAlly = ally;
                        lowestHp = ally.Health;
                    }
                }
            });
            return lowestHpAlly;
        }

        public static bool IsAllyInDanger(Obj_AI_Hero ally)
        {
            if (ally.IsDead || ally.InFountain())
            {
                return false;
            }
            var enemies = GetDangerousEnemiesInRange(ally, GenericContext.SCAN_DISTANCE / 2);
            var dangerHp = ally.Health < GetTypicalHp(ally.Level, GenericContext.DANGER_UNDER_PERCENT);
            var fearHp = ally.Health < GetTypicalHp(ally.Level, GenericContext.FEAR_UNDER_PERCENT);
            var allyInfo = GenericContext.GetHeroInfo(ally);
            var nearestEnemyTurret = GetNearestTower(ally, false);

            return
                (
                    dangerHp
                    &&
                    (
                        enemies.Count > 0
                        ||
                        ally.Distance(nearestEnemyTurret) < GenericContext.TURRET_RANGE
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

        public static bool IsAllySafe(Obj_AI_Hero ally)
        {
            if (ally.IsZombie || ally.IsDead || ally.Distance(ProducedContext.ALLY_SPAWN.Get()) < ally.BoundingRadius)
            {
                return true;
            }
            var aboveDangerHp = ally.Health > GetTypicalHp(ally.Level, GenericContext.DANGER_UNDER_PERCENT);
            var allyInfo = GenericContext.GetHeroInfo(ally);
            var nearestEnemy = GetNearestHero(ally, false);
            return
            (
                GetDangerousEnemiesInRange(ally, GenericContext.SCAN_DISTANCE * 2).Count == 0
            )
            ||
            (
                !allyInfo.IsFocusedByTower()
                &&
                aboveDangerHp
                &&
                (
                    ProducedContext.ALL_ALLIES.Get()
                        .Count(hero => hero != null && !hero.IsMe && BBetweenAandC(ally, hero, nearestEnemy)) > 0
                    ||
                    ProducedContext.ALL_ENEMIES.Get()
                        .Count(
                            enemy => enemy != null &&
                                BBetweenAandC(ally.Position, GenericContext.GetHeroInfo(enemy).GetFacing(),
                                    enemy.Position) && enemy.Distance(ally) < GenericContext.SCAN_DISTANCE) == 0
                )
            );
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

        public static List<Obj_AI_Hero> GetDangerousEnemiesInRange(Obj_AI_Hero ally, int range)
        {
            var result = new List<Obj_AI_Hero>();
            if (ally != null)
            {
                ProducedContext.ALL_ENEMIES.Get().ForEach(enemy =>
                {
                    if (!enemy.IsDead && ally.ServerPosition.Distance(enemy.ServerPosition) < range &&
                        !enemy.IsStunned)
                    {
                        result.Add(enemy);
                    }
                });
            }
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
            var lowestDistance = float.MaxValue;
            var turretTeamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var turrets = turretTeamCondition ? ProducedContext.ALLY_TURRETS.Get() : ProducedContext.ENEMY_TURRETS.Get();
            foreach (var turret in turrets)
            {
                var distance = turret.Distance(hero);
                if (distance < lowestDistance)
                {
                    result = turret;
                    lowestDistance = distance;
                }
            }
            return result;
        }

        public static Obj_AI_Hero GetNearestHero(Obj_AI_Hero hero, bool alliedToHero)
        {
            Obj_AI_Hero result = null;
            var lowestDistance = float.MaxValue;
            var turretTeamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = turretTeamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            foreach (var other in heroes)
            {
                var distance = other.Distance(hero);
                if (distance < lowestDistance && other != hero)
                {
                    result = other;
                    lowestDistance = distance;
                }
            }
            return result;
        }


    }
}