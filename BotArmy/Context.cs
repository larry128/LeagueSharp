using LeagueSharp;

namespace najsvan
{
    public abstract class Context
    {
        // READ ONLY
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 200;

        // SET ONCE THEN LEAVE ALONE
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