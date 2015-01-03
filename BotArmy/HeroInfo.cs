using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace najsvan
{
    public class HeroInfo
    {
        private readonly Obj_AI_Hero realHero;

        public List<ExpectedChange> relevantChanges;

        public HeroInfo(Obj_AI_Hero realHero)
        {
            this.realHero = realHero;
        }
    }
}
