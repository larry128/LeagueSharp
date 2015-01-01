using System.Collections.Generic;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class ItemMapper
    {
        private static Dictionary<ItemId, ItemData.Item> ITEM_MAP;

        public static ItemData.Item? GetItem(ItemId id)
        {
            if (ITEM_MAP == null)
            {
                ITEM_MAP = new Dictionary<ItemId, ItemData.Item>();
                FieldInfo[] fields = typeof(ItemData).GetFields();
                foreach (var field in fields)
                {
                    var item = (ItemData.Item)field.GetRawConstantValue();
                    var itemId = (ItemId)item.Id;
                    ITEM_MAP.Add(itemId, item);
                }
            }

            ItemData.Item result;
            if (ITEM_MAP.TryGetValue(id, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}