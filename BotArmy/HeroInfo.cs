using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public class HeroInfo
    {
        private readonly int realHeroNetId;
        private readonly Stack hpHistory = new Stack();
        private Vector3 facing = Vector3.Zero;
        private Obj_AI_Turret focusedByTower;

        public HeroInfo(int realHeroNetId)
        {
            this.realHeroNetId = realHeroNetId;
        }

        public void UpdateHpHistory()
        {
            Obj_AI_Hero realHero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(realHeroNetId);
            hpHistory.Push(realHero.Health);
            if (hpHistory.Count > 3)
            {
                hpHistory.Pop();
            }
        }

        public float GetHpLost()
        {
            Obj_AI_Hero realHero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(realHeroNetId);
            var hpLastTick = (float)hpHistory.Peek();
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

        public void SetFocusedByTower(Obj_AI_Turret focusedByTower)
        {
            this.focusedByTower = focusedByTower;
        }

        public Obj_AI_Turret GetFocusedByTower()
        {
            return focusedByTower;
        }

        public bool IsFocusedByTower()
        {
            return focusedByTower != null;
        }

        public Vector3 GetFacing()
        {
            return facing;
        }

        public void SetFacing(Vector3 facing)
        {
            this.facing = facing;
        }

        public Obj_AI_Hero GetRealHero()
        {
            Obj_AI_Hero realHero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(realHeroNetId);
            return realHero;
        }
    }
}
