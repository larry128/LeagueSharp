using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    class Targeting
    {
        private static int WAITING_FOR_BETTER_TARGET_SINCE = 0;
        private static int WAIT_FOR_BETTER_TARGET_SEC = 3;

        public class TargetValuePair
        {
            private readonly Obj_AI_Hero target;
            private readonly int value;

            public TargetValuePair(Obj_AI_Hero target, int value)
            {
                this.target = target;
                this.value = value;
            }

            public Obj_AI_Hero GetTarget()
            {
                return target;
            }

            public int GetValue()
            {
                return value;
            }
        }

        public enum PriorityMode
        {
            Default,
            HighestAp
        }

        public enum AttackType
        {
            Damage,
            Disable
        }

        public static Obj_AI_Hero FindAllyInDanger(int range)
        {
            Obj_AI_Hero lowestHpAlly = null;
            var lowestHp = float.MaxValue;

            ProducedContext.ALL_ALLIES.Get().ForEach(ally =>
            {
                if (LibraryOfAIexandria.GetHitboxDistance(GenericContext.MY_HERO, ally) < range && !ally.InFountain() && !ally.IsDead)
                {
                    if (ally.Health < lowestHp && ally.Health > 1 && LibraryOfAIexandria.IsAllyInDanger(ally))
                    {
                        lowestHpAlly = ally;
                        lowestHp = ally.Health;
                    }
                }
            });
            return lowestHpAlly;
        }

        public static Obj_AI_Hero FindPriorityTarget(int range, bool unrestricted)
        {
            return FindPriorityTarget(range, unrestricted, PriorityMode.Default);
        }

        public static Obj_AI_Hero FindNearestHero(Obj_AI_Hero hero, bool alliedToHero)
        {
            Obj_AI_Hero result = null;
            var lowestDistance = float.MaxValue;
            var turretTeamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = turretTeamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            foreach (var other in heroes)
            {
                var distance = other.Distance(hero);
                if (distance < lowestDistance && other != hero)
                {
                    result = other;
                    lowestDistance = distance;
                }
            }
            return result;
        }

        public static Obj_AI_Hero FindPriorityTarget(int range, bool unrestricted, PriorityMode mode)
        {
            var lowestHpEnemyRange = FindLowestHpTarget(GenericContext.ENEMY_TEAM, false, range);
            var emergencyTarget = FindAllyInDanger(range);
            Obj_AI_Hero threatToEmergencyTarget = null;

            if (emergencyTarget != null)
            {
                threatToEmergencyTarget = FindNearestHero(emergencyTarget, false);
            }

            if (threatToEmergencyTarget.IsValidTarget() && LibraryOfAIexandria.GetHitboxDistance(GenericContext.MY_HERO, threatToEmergencyTarget) < range)
            {
                return threatToEmergencyTarget;
            }
            else if (lowestHpEnemyRange != null && LibraryOfAIexandria.IsTypicalHpUnder(lowestHpEnemyRange.GetTarget(), GenericContext.DANGER_UNDER_PERCENT))
            {
                return lowestHpEnemyRange.GetTarget();
            }
            else
            {

                TargetValuePair alternateTarget = null;
                TargetValuePair attackTarget = null;

                if (mode == PriorityMode.HighestAp)
                {
                    attackTarget = FindHighestApTarget(GenericContext.ENEMY_TEAM, false, range);
                    alternateTarget = FindHighestApTarget(GenericContext.ENEMY_TEAM, false, GenericContext.SCAN_DISTANCE);
                    if (alternateTarget.GetTarget().IsValid && alternateTarget.GetValue() < 100)
                    {
                        mode = PriorityMode.Default;
                        attackTarget = null;
                        alternateTarget = null;
                    }
                }

                if (mode == PriorityMode.Default)
                {
                    attackTarget = FindHighestImpactTarget(GenericContext.ENEMY_TEAM, unrestricted, range);
                    alternateTarget = FindHighestImpactTarget(GenericContext.ENEMY_TEAM, unrestricted, GenericContext.SCAN_DISTANCE);
                }

                if (attackTarget != null && !attackTarget.GetTarget().Equals(alternateTarget.GetTarget()) && alternateTarget.GetValue() > attackTarget.GetValue() && WAITING_FOR_BETTER_TARGET_SINCE == 0)
                {
                    WAITING_FOR_BETTER_TARGET_SINCE = Environment.TickCount;
                }
                if (LibraryOfAIexandria.GetSecondsSince(WAITING_FOR_BETTER_TARGET_SINCE) < WAIT_FOR_BETTER_TARGET_SEC)
                {
                    attackTarget = null;
                }
                else
                {
                    WAITING_FOR_BETTER_TARGET_SINCE = 0;
                }
                return attackTarget == null ? null : attackTarget.GetTarget();
            }
        }

        private static TargetValuePair FindHighestImpactTarget(GameObjectTeam team, bool unrestricted, int range)
        {
            return null;
        }

        private static TargetValuePair FindHighestApTarget(GameObjectTeam team, bool unrestricted, int range)
        {
            return null;
        }

        private static TargetValuePair FindLowestHpTarget(GameObjectTeam team, bool unrestricted, int range)
        {
            return null;
        }
    }
}
