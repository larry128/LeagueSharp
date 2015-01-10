using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class HeroInfo
    {
        private readonly Obj_AI_Hero realHero;
        private readonly Stack hpHistory = new Stack();
        public Vector3 facing = Vector3.Zero;

        public HeroInfo(Obj_AI_Hero realHero)
        {
            this.realHero = realHero;
        }

        public void UpdateHpHistory()
        {
            hpHistory.Push(realHero.Health);
            if (hpHistory.Count > 3)
            {
                hpHistory.Pop();
            }
        }

        public float GetHpLost()
        {
            var hpLastTick = (float) hpHistory.Peek();
            return hpLastTick - realHero.Health;
        }

        public bool IsGoingTowards(Obj_AI_Hero unit)
        {
            return false;
        }

        public bool IsRamming(Obj_AI_Hero unit)
        {
            return false;
        }

        public bool IsFocusing(Obj_AI_Hero unit)
        {
            return false;
        }

        public int CanKillInMillis(Obj_AI_Hero unit)
        {
            return 0;
        }
    }
}
