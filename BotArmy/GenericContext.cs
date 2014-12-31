using System.Collections.Generic;
using LeagueSharp;

namespace najsvan
{
    public abstract class GenericContext
    {
        public Obj_SpawnPoint allySpawn;
        public int currentTick = 0;
        public Obj_SpawnPoint enemySpawn;
        public int lastElixirBought = 0;
        public int lastFailedBuy = 0;
        public int lastTickProcessed = 0;
        public int lastWardDropped = 0;
        public SpellSlot[] levelSpellsOrder;
        public ItemId[] shoppingList;
        public ItemId[] shoppingListConsumables;
        public ItemId shoppingListElixir;
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 300;
        public readonly int wardPlaceDistance = 600;
        public readonly int wardSightRadius = 1200;

        public readonly Dictionary<GameObjectTeam, List<WardSpot>> wardSpots = new Dictionary
            <GameObjectTeam, List<WardSpot>>
        {
            {
                GameObjectTeam.Neutral, new List<WardSpot>
                {
                    new WardSpot(6871, 3074),
                    new WardSpot(5580, 3543),
                    new WardSpot(6552, 4736),
                    new WardSpot(8542, 4802),
                    new WardSpot(8103, 6267),
                    new WardSpot(10796, 5213),
                    new WardSpot(11547, 7099),
                    new WardSpot(9965, 6572),
                    new WardSpot(8716, 6721),
                    new WardSpot(9942, 7872),
                    new WardSpot(6813, 8580),
                    new WardSpot(6258, 8128),
                    new WardSpot(4944, 8482),
                    new WardSpot(4811, 7125),
                    new WardSpot(3356, 7782),
                    new WardSpot(2387, 9698),
                    new WardSpot(2952, 11221),
                    new WardSpot(4453, 11836),
                    new WardSpot(5733, 12766),
                    new WardSpot(6755, 11500),
                    new WardSpot(8010, 11845),
                    new WardSpot(9249, 11446),
                    new WardSpot(8298, 10283)
                }
            },
            {
                GameObjectTeam.Order, new List<WardSpot>
                {
                    new WardSpot(11897, 3696),
                    new WardSpot(10477, 3101),
                    new WardSpot(12605, 5112)
                }
            },
            {
                GameObjectTeam.Chaos, new List<WardSpot>
                {
                    new WardSpot(11897, 3696),
                    new WardSpot(10477, 3101),
                    new WardSpot(12605, 5112)
                }
            }
        };
    }
}