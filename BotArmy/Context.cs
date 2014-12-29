using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public class Context
    {
        // READ ONLY
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 100;
        public readonly int spawnBuyRange = 900;

        // SET ONCE - LEAVE ALONE
        public Obj_SpawnPoint allySpawn;
        public Obj_SpawnPoint enemySpawn; 

        // READ WRITE
        public int lastTickProcessed = 0;
        public bool disableTickProcessing = false;
        public Vector3 moveTo;
    }
}
