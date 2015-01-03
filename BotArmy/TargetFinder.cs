using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class TargetFinder
    {
        public static Obj_AI_Hero FindRecklessHelpAlly(float range)
        {
            Obj_AI_Hero lowestHpAlly = null;
            var lowestHp = float.MaxValue;

            GenericContext.allies.ForEach(ally =>
            {
                if (BotUtils.GetHitboxDistance(GenericContext.MY_HERO, ally) < range && !ally.InFountain())
                {
                    if (ally.Health < lowestHp && ally.Health > 1)
                    {
                        var enemies = GetDangerousEnemiesInRange(ally, GenericContext.SCAN_DISTANCE / 2);
                        if ((enemies.Count > 0 || ally.UnderTurret(true)) &&
                            ally.Health < BotUtils.GetTypicalHpPercent(ally.Level, GenericContext.PANIC_UNDER_PERCENT))
                        {
                            lowestHpAlly = ally;
                            lowestHp = ally.Health;
                        }
                    }
                }
            });
            return lowestHpAlly;
        }

        public static bool IsAllyInPanic(Obj_AI_Hero ally)
        {
            var enemies = GetDangerousEnemiesInRange(ally, GenericContext.SCAN_DISTANCE / 2);
            var panicHp = ally.Health < BotUtils.GetTypicalHpPercent(ally.Level, GenericContext.PANIC_UNDER_PERCENT);
            var isAllyInPanic = (enemies.Count > 0 || ally.UnderTurret(true)) &&
                                 panicHp;
            return isAllyInPanic;
        }

        private static List<Obj_AI_Hero> GetDangerousEnemiesInRange(Obj_AI_Hero ally, int range)
        {
            var result = new List<Obj_AI_Hero>();
            if (ally != null)
            {
                GenericContext.enemies.ForEach(enemy =>
                {
                    if (!enemy.IsDead && ally.Distance(enemy) < range &&
                        !enemy.IsStunned)
                    {
                        result.Add(enemy);
                    }
                });
            }
            return result;
        }
    }
}