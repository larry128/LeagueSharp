using System;
using System.Collections.Generic;
using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public abstract class Context
    {
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 300;
        public readonly Dictionary<GameObjectTeam, List<Vector2>> wardSpots = new Dictionary<GameObjectTeam, List<Vector2>>
        {
            {GameObjectTeam.Neutral, new List<Vector2> 
            { 
                new Vector2(6871,3074),
                new Vector2(5580, 3543),
                new Vector2(6552, 4736),
                new Vector2(8542, 4802),
                new Vector2(8103, 6267),
                new Vector2(10796, 5213),
                new Vector2(11547, 7099),
                new Vector2(9965, 6572),
                new Vector2(8716, 6721),
                new Vector2(9942, 7872),
                new Vector2(6813, 8580),
                new Vector2(6258, 8128),
                new Vector2(4944, 8482),
                new Vector2(4811, 7125),
                new Vector2(3356, 7782),
                new Vector2(2387, 9698),
                new Vector2(2952, 11221),
                new Vector2(4453, 11836),
                new Vector2(5733, 12766),
                new Vector2(6755, 11500),
                new Vector2(8010, 11845),
                new Vector2(9249, 11446),
                new Vector2(8298, 10283)
            }},
            {GameObjectTeam.Order, new List<Vector2>
            {
                new Vector2(11897, 3696),
                new Vector2(10477, 3101),
                new Vector2(12605, 5112),
            }},                    
            {GameObjectTeam.Chaos, new List<Vector2>
            {
                new Vector2(11897, 3696),
                new Vector2(10477, 3101),
                new Vector2(12605, 5112),
            }}                    
        };

        public Obj_SpawnPoint allySpawn;
        public Obj_SpawnPoint enemySpawn;
        public SpellSlot[] levelSpellsOrder;
        public ItemId[] shoppingList;
        public ItemId[] shoppingListConsumables;
        public ItemId shoppingListElixir;

        // READ WRITE
        public int currentTick = 0;
        public int lastElixirBought = 0;
        public int lastTickProcessed = 0;
        public int lastFailedBuy = 0;

    }
}