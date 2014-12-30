using LeagueSharp;

namespace najsvan
{
    public abstract class Context
    {
        // READ ONLY
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 200;

        // SET ONCE - LEAVE ALONE
        public SpellSlot[] levelSpellsOrder;
        public Obj_SpawnPoint allySpawn;
        public Obj_SpawnPoint enemySpawn; 

        // READ WRITE
        public int lastTickProcessed = 0;
    }
}
