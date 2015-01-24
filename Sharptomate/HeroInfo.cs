using System.Collections;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public class HeroInfo
    {
        private Vector2 direction = Vector2.Zero;
        private Obj_AI_Turret focusedByTower;
        private readonly Stack hpHistory = new Stack();
        private readonly int realHeroNetId;

        public HeroInfo(int realHeroNetId)
        {
            this.realHeroNetId = realHeroNetId;
        }

        public void UpdateHpHistory()
        {
            hpHistory.Push(GetRealHero().Health);
            if (hpHistory.Count > 3)
            {
                hpHistory.Pop();
            }
        }

        public float GetHpLost()
        {
            var hpLastTick = (float)hpHistory.Peek();
            return hpLastTick - GetRealHero().Health;
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
            return GetFocusedByTower() != null;
        }

        public Vector2 GetDirection()
        {
            return direction;
        }

        public void SetDirection(Vector2 start, Vector2 end)
        {
            if (!start.Equals(end))
            {
                direction = end - start;
            }
        }

        public void UpdateDirection()
        {
            var start = GetRealHero().Position.To2D();
            var end = GetRealHero().ServerPosition.To2D();
            SetDirection(start, end);
        }

        public Vector3 GetFacing()
        {
            return GetRealHero().Position + direction.To3D();
        }

        private Obj_AI_Hero GetRealHero()
        {
            return ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(realHeroNetId);
        }
    }
}