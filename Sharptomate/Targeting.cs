using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    class Targeting
    {
        private static int WAITING_FOR_BETTER_TARGET_SINCE = 0;
        private const int WAIT_FOR_BETTER_TARGET_SEC = 3;

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

        public enum PriorityMode
        {
            HighestImpact,
            HighestAp
        }

        public enum AttackType
        {
            Damage,
            Disable
        }

        public static Obj_AI_Hero FindAllyInDanger(float range)
        {
            Obj_AI_Hero lowestHpAlly = null;
            var lowestHp = float.MaxValue;

            ProducedContext.ALL_ALLIES.Get().ForEach(ally =>
            {
                if (LibraryOfAIexandria.GetHitboxDistance(GenericContext.MY_HERO, ally) < range && !ally.InFountain())
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

        public static Obj_AI_Hero FindPriorityTarget(float range, bool unrestricted)
        {
            return FindPriorityTarget(range, unrestricted, PriorityMode.HighestImpact);
        }

        public static Obj_AI_Hero FindNearestHero(Obj_AI_Hero hero, bool alliedToHero)
        {
            Obj_AI_Hero result = null;
            var lowestDistance = float.MaxValue;
            var teamCondition = alliedToHero ? hero.IsAlly : !hero.IsAlly;
            var heroes = teamCondition ? ProducedContext.ALL_ALLIES.Get() : ProducedContext.ALL_ENEMIES.Get();
            heroes.ForEach(other =>
            {
                var distance = other.Distance(hero);
                if (distance < lowestDistance && other != hero)
                {
                    result = other;
                    lowestDistance = distance;
                }
            }
            );
            return result;
        }

        public static Obj_AI_Hero FindPriorityTarget(float range, bool spamming, PriorityMode mode)
        {
            var lowestHpEnemyRange = FindLowestHpEnemy(spamming, range);
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
                    attackTarget = FindHighestApEnemy(spamming, range);
                    if (!spamming)
                    {
                        alternateTarget = FindHighestApEnemy(false, GenericContext.SCAN_DISTANCE);
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
                    attackTarget = FindHighestImpactEnemy(spamming, range);
                    if (!spamming)
                    {
                        alternateTarget = FindHighestImpactEnemy(false, GenericContext.SCAN_DISTANCE);
                    }
                }

                if (!spamming)
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
        }

        private static TargetValuePair FindHighestImpactEnemy(bool spamming, float range)
        {
            return FindBestStatEnemy(spamming, range, LibraryOfAIexandria.GetImpact, true);
        }

        private static TargetValuePair FindHighestApEnemy(bool spamming, float range)
        {
            return FindBestStatEnemy(spamming, range, enemy => enemy.BaseAbilityDamage, true);
        }

        private static TargetValuePair FindLowestHpEnemy(bool spamming, float range)
        {
            return FindBestStatEnemy(spamming, range, enemy => enemy.Health, false);
        }

        private delegate float GetStat(Obj_AI_Hero hero);

        private static TargetValuePair FindBestStatEnemy(bool spamming, float range, GetStat function, bool higherIsBetter)
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
                var distance = GenericContext.MY_HERO.Distance(enemy);
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
                if (distance < range && isBestStat
                    && (spamming
                    || LibraryOfAIexandria.GetHeroesInRange(enemy, false, GenericContext.SCAN_DISTANCE / 2).Count > 0
                    || LibraryOfAIexandria.IsTypicalHpUnder(enemy, GenericContext.FEAR_UNDER_PERCENT))
                    || GenericContext.MY_HERO.Mana / GenericContext.MY_HERO.MaxMana > 0.5)
                {
                    result = new TargetValuePair(enemy, stat);
                    bestStat = stat;
                }
            }
            );
            return result;
        }
    }
}
