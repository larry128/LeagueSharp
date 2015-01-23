using System.Collections.Generic;
using LeagueSharp.Common.Data;

namespace najsvan
{
    public class ItemMapper
    {
        private static readonly Logger LOG = Logger.GetLogger(typeof(ItemMapper).Name);

        private static Dictionary<int, ItemData.Item> ITEM_MAP;

        public static ItemData.Item? GetItem(int id)
        {
            if (ITEM_MAP == null)
            {
                InitItemMap();
            }
            ItemData.Item result;
            if (ITEM_MAP.TryGetValue(id, out result))
            {
                return result;
            }
            return null;
        }

        private static void InitItemMap()
        {
            ITEM_MAP = new Dictionary<int, ItemData.Item>();
            var data = new ItemData();
            var fields = typeof (ItemData).GetFields();
            foreach (var field in fields)
            {
                var item = (ItemData.Item) field.GetValue(data);
                var itemId = item.Id;
                try
                {
                    ITEM_MAP.Add(itemId, item);
                }
                catch (System.ArgumentException)
                {
                    LOG.Error("Already have: " + itemId);
                }
            }
        }
    }
}