using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public static class BotUtils
    {
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

        public static float GetHitboxDistance(GameObject obj, GameObject obj2)
        {
            return obj.Position.Distance(obj2.Position) + obj.BoundingRadius + obj2.BoundingRadius - 8;
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

        public static ItemData.Item? GetNextBuyItemId()
        {
            if (GenericContext.shoppingList.Length > 0)
            {
                // expand inventory list
                var expandedInventory = new List<int>();
                foreach (var inventorySlot in GetOccuppiedInventorySlots())
                {
                    ExpandRecipe(inventorySlot.Id, expandedInventory);
                }

                // reduce expandedInventoryList
                foreach (var itemId in GenericContext.shoppingList)
                {
                    if (!expandedInventory.Remove((int)itemId))
                    {
                        ItemMapper.GetItem(itemId);
                    }
                }
            }
            return null;
        }

        public static bool IsAWardNear(Vector2 position)
        {
            var wardList = ProducedContext.Get(ProducedContextKey.Wards) as List<GameObject>;
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

        public static void ExpandRecipe(ItemId itemId, List<int> into)
        {
            @into.Add((int)itemId);
            var item = ItemMapper.GetItem(itemId);
            if (item != null && item.Value.RecipeItems != null && item.Value.RecipeItems.Length > 0)
            {
                var recipe = item.Value.RecipeItems;
                @into.AddRange(recipe);
                foreach (var id in recipe)
                {
                    ExpandRecipe((ItemId)id, @into);
                }
            }
        }

        public static float GetAdjustedAllyHealth(Obj_AI_Hero ally)
        {
            float[] result = { ally.Health };
            GenericContext.SERVER_INTERACTIONS.ForEach(interaction =>
            {
                var healed = interaction.change as AllyHealed;
                if (healed != null)
                {
                    foreach (var healedAlly in healed.who)
                    {
                        if (healedAlly.NetworkId == ally.NetworkId)
                        {
                            result[0] += healed.amount;
                        }
                    }
                }
            });
            return result[0];
        }

        public static float GetAdjustedEnemyHealth(Obj_AI_Hero enemy)
        {
            float[] result = { enemy.Health };
            GenericContext.SERVER_INTERACTIONS.ForEach(interaction =>
            {
                var damaged = interaction.change as EnemyDamaged;
                if (damaged != null)
                {
                    foreach (var damagedEnemy in damaged.who)
                    {
                        if (damagedEnemy.NetworkId == enemy.NetworkId)
                        {
                            result[0] -= damaged.amount;
                        }
                    }
                }
            });

            return result[0];
        }

        public static bool GetAdjustedEnemyDisabled(Obj_AI_Hero enemy)
        {
            if (enemy.IsStunned)
            {
                return true;
            }
            else
            {

                bool[] result = { false };
                GenericContext.SERVER_INTERACTIONS.ForEach(interaction =>
                {
                    var damaged = interaction.change as EnemyStunned;
                    if (damaged != null)
                    {
                        foreach (var stunnedEnemy in damaged.who)
                        {
                            if (stunnedEnemy.NetworkId == enemy.NetworkId)
                            {
                                result[0] = true;
                            }
                        }
                    }
                });

                return result[0];
            }
        }

        public static int GetSummonerHealAmount()
        {
            return 75 + 15 * GenericContext.MY_HERO.Level;
        }
    }
}