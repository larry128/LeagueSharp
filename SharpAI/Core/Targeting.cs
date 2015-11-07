using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    internal class Targeting
    {
        public enum AttackType
        {
            Damage,
            Disable
        }

        public enum PriorityMode
        {
            HighestImpact,
            HighestAp
        }

        private const int WAIT_FOR_BETTER_TARGET_SEC = 3;
        private static int WAITING_FOR_BETTER_TARGET_SINCE;

        public static Obj_AI_Hero FindAllyInDanger(float range)
        {
            Obj_AI_Hero lowestHpAlly = null;
            var lowestHp = float.MaxValue;

            ProducedContext.ALL_ALLIES.Get().ForEach(ally =>
            {
                if (LibraryOfAIexandria.GetHitboxDistance(Constants.MY_HERO, ally) < range && !ally.InFountain())
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

        public static Obj_AI_Hero FindPriorityTarget(float range, bool spammable, bool manaless)
        {
            return FindPriorityTarget(range, spammable, manaless, PriorityMode.HighestImpact);
        }

        public static Obj_AI_Hero FindNearestHero(Obj_AI_Hero hero, bool alliedToHero)
        {
            Obj_AI_Hero result = null;
            var lowestDistance = float.MaxValue;
            var teamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = teamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            heroes.ForEach(other =>
            {
                var distance = LibraryOfAIexandria.GetHitboxDistance(other, hero);
                if (distance < lowestDistance && other != hero)
                {
                    result = other;
                    lowestDistance = distance;
                }
            }
                );
            return result;
        }

        public static Obj_AI_Hero FindPriorityTarget(float range, bool spammable, bool manaless, PriorityMode mode)
        {
            var lowestHpEnemyRange = FindLowestHpEnemy(spammable, manaless, range);
            var emergencyTarget = FindAllyInDanger(range);
            Obj_AI_Hero threatToEmergencyTarget = null;

            if (emergencyTarget != null)
            {
                threatToEmergencyTarget = FindNearestHero(emergencyTarget, false);
            }

            if (threatToEmergencyTarget.IsValidTarget() &&
                LibraryOfAIexandria.GetHitboxDistance(Constants.MY_HERO, threatToEmergencyTarget) < range)
            {
                return threatToEmergencyTarget;
            }
            if (lowestHpEnemyRange != null &&
                LibraryOfAIexandria.IsTypicalHpUnder(lowestHpEnemyRange.GetTarget(), Constants.DANGER_UNDER_PERCENT))
            {
                return lowestHpEnemyRange.GetTarget();
            }
            TargetValuePair alternateTarget = null;
            TargetValuePair attackTarget = null;

            if (mode == PriorityMode.HighestAp)
            {
                attackTarget = FindHighestApEnemy(spammable, manaless, range);
                if (!spammable)
                {
                    alternateTarget = FindHighestApEnemy(false, manaless, Constants.SCAN_DISTANCE);
                    if (alternateTarget.GetTarget().IsValid && alternateTarget.GetValue() < 100)
                    {
                        mode = PriorityMode.HighestImpact;
                        attackTarget = null;
                        alternateTarget = null;
                    }
                }
            }

            if (mode == PriorityMode.HighestImpact)
            {
                attackTarget = FindHighestImpactEnemy(spammable, manaless, range);

                if (!spammable)
                {
                    alternateTarget = FindHighestImpactEnemy(false, manaless, Constants.SCAN_DISTANCE);
                }
            }

            if (!spammable)
            {
                if (attackTarget != null && !attackTarget.GetTarget().Equals(alternateTarget.GetTarget()) &&
                    alternateTarget.GetValue() > attackTarget.GetValue() && WAITING_FOR_BETTER_TARGET_SINCE == 0)
                {
                    WAITING_FOR_BETTER_TARGET_SINCE = Environment.TickCount;
                }
                if (LibraryOfAIexandria.GetSecondsSince(WAITING_FOR_BETTER_TARGET_SINCE) <
                    WAIT_FOR_BETTER_TARGET_SEC)
                {
                    attackTarget = null;
                }
                else
                {
                    WAITING_FOR_BETTER_TARGET_SINCE = 0;
                }
            }
            return attackTarget == null ? null : attackTarget.GetTarget();
        }

        public static TargetValuePair FindHighestImpactEnemy(bool spammable, bool manaless, float range)
        {
            return FindBestStatEnemy(spammable, manaless, range, LibraryOfAIexandria.GetImpact, true);
        }

        public static TargetValuePair FindHighestApEnemy(bool spammable, bool manaless, float range)
        {
            return FindBestStatEnemy(spammable, manaless, range, enemy => enemy.FlatMagicDamageMod, true);
        }

        public static TargetValuePair FindLowestHpEnemy(bool spammable, bool manaless, float range)
        {
            return FindBestStatEnemy(spammable, manaless, range, enemy => enemy.Health, false);
        }

        private static TargetValuePair FindBestStatEnemy(bool spammable, bool manaless, float range, GetStat function,
            bool higherIsBetter)
        {
            TargetValuePair result = null;
            float bestStat;
            if (higherIsBetter)
            {
                bestStat = 0f;
            }
            else
            {
                bestStat = float.MaxValue;
            }
            ProducedContext.ALL_ENEMIES.Get().ForEach(enemy =>
            {
                var distance = LibraryOfAIexandria.GetHitboxDistance(Constants.MY_HERO, enemy);
                var stat = function(enemy);
                bool isBestStat;
                if (higherIsBetter)
                {
                    isBestStat = bestStat < stat;
                }
                else
                {
                    isBestStat = bestStat > stat;
                }
                if (
                    distance < range && isBestStat && enemy.IsValidTarget()
                    &&
                    (
                        spammable
                        &&
                        (
                            manaless
                            ||
                            Constants.MY_HERO.Mana/Constants.MY_HERO.MaxMana > 0.5
                        )
                        ||
                        LibraryOfAIexandria.GetHeroesInRange(enemy, false, Constants.SCAN_DISTANCE/2).Count > 0
                        ||
                        LibraryOfAIexandria.IsTypicalHpUnder(enemy, Constants.FEAR_UNDER_PERCENT)
                        )
                    )
                {
                    result = new TargetValuePair(enemy, stat);
                    bestStat = stat;
                }
            }
                );
            return result;
        }

        public class TargetValuePair
        {
            private readonly Obj_AI_Hero target;
            private readonly float value;

            public TargetValuePair(Obj_AI_Hero target, float value)
            {
                this.target = target;
                this.value = value;
            }

            public Obj_AI_Hero GetTarget()
            {
                return target;
            }

            public float GetValue()
            {
                return value;
            }
        }

        private delegate float GetStat(Obj_AI_Hero hero);
    }
}