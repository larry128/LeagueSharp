using LeagueSharp;

namespace najsvan
{
    public class ImmutableContext
    {
        public readonly Obj_AI_Hero myHero = ObjectManager.Player;
        public readonly int tickDelay = 100;
    }
}
