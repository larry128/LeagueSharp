using System.Collections.Generic;
using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public static class GenericContext
    {
        public static Obj_SpawnPoint allySpawn;
        public static int currentTick = 0;
        public static Obj_SpawnPoint enemySpawn;
        public static int lastElixirBought = 0;
        public static int lastFailedBuy = 0;
        public static int lastTickProcessed = 0;
        public static int lastWardDropped = 0;
        public static SpellSlot[] levelSpellsOrder;
        public static ItemId[] shoppingList;
        public static ItemId[] shoppingListConsumables;
        public static ItemId shoppingListElixir;
        public static SpellSlot summonerHeal;
        public static SpellSlot summonerIgnite;
        public static SpellSlot summonerFlash;
        public static Vector3 lastDestination = Vector3.Zero;
        public static List<Obj_AI_Hero> allies = new List<Obj_AI_Hero>();
        public static List<Obj_AI_Hero> enemies = new List<Obj_AI_Hero>();
        public static readonly List<ServerInteraction> SERVER_INTERACTIONS = new List<ServerInteraction>();
        public static readonly Obj_AI_Hero MY_HERO = ObjectManager.Player;
        public static readonly int SCAN_DISTANCE = 700;
        public static readonly int BASE_PER_LVL_HP = 77;
        public static readonly int BASE_LVL1_HP = 600;
        public static readonly double AFRAID_UNDER_PERCENT = 0.5;
        public static readonly double PANIC_UNDER_PERCENT = 0.25;
        public static readonly int TICK_DELAY = 100;
        public static readonly int WARD_PLACE_DISTANCE = 600;
        public static readonly int SUMMONER_HEAL_RANGE = 700;
        public static readonly int WARD_SIGHT_RADIUS = 1200;

        public static readonly Dictionary<GameObjectTeam, List<WardSpot>> WARD_SPOTS = new Dictionary
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