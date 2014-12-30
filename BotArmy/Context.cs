using LeagueSharp;

namespace najsvan
{
    public abstract class Context
    {
        public Obj_SpawnPoint allySpawn;
        // READ WRITE
        public int currentTick = 0;
        public Obj_SpawnPoint enemySpawn;
        public int lastElixirBought = 0;
        public int lastTickProcessed = 0;
        // SET ONCE THEN LEAVE ALONE
        public SpellSlot[] levelSpellsOrder;
        public ItemId[] shoppingList;
        public ItemId[] shoppingListConsumables;
        public ItemId shoppingListElixir;
        // READ ONLY
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 200;
    }
}