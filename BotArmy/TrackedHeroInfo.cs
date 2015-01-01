using LeagueSharp;

namespace najsvan
{
    public class TrackedHeroInfo
    {
        public readonly Obj_AI_Hero realHero;

        public TrackedHeroInfo(Obj_AI_Hero realHero)
        {
            this.realHero = realHero;
        }
    }
}
