
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class TargetFinder
    {
        private GenericContext context;        
        private ProducedContext producedContext;
        private List<ServerInteraction> serverInteractions;

        public TargetFinder(GenericContext context, ProducedContext producedContext, List<ServerInteraction> serverInteractions)
        {
            this.context = context;
            this.producedContext = producedContext;
            this.serverInteractions = serverInteractions;
        }

        private float GetAdjustedAllyHealth(Obj_AI_Hero ally)
        {
            float[] result = { ally.Health };
            serverInteractions.ForEach(interaction =>
            {
                var healed = interaction.change as AllyHealed;
                if (healed != null && healed.who.NetworkId == ally.NetworkId)
                {
                    result[0] += healed.amount;
                }
            });
            return result[0];
        }

        private float GetAdjustedEnemyHealth(Obj_AI_Hero enemy)
        {
            float[] result = { enemy.Health };
            serverInteractions.ForEach(interaction =>
            {
                var damaged = interaction.change as EnemyDamaged;
                if (damaged != null && damaged.who.NetworkId == enemy.NetworkId)
                {
                    result[0] -= damaged.amount;
                }
            });

            return result[0];
        }

        public Obj_AI_Hero GetLowestHpAlly(float range)
        {
            Obj_AI_Hero lowestHPAlly = null;
            float lowestHP = float.MaxValue;

            context.allies.ForEach(ally =>
            {
                if (GetHitboxDistance(context.myHero, ally) < range && !ally.InFountain())
                {
                    var adjustedAllyHealth = GetAdjustedAllyHealth(ally);
                    if (adjustedAllyHealth < lowestHP && adjustedAllyHealth > 1)
                    {
                        lowestHPAlly = ally;
                        lowestHP = adjustedAllyHealth;
                    }
                }
            });
            return lowestHPAlly;
        }

        public static float GetHitboxDistance(float distance, GameObject obj)
        {
            return distance + obj.BoundingRadius + 40;
        }

        public static float GetHitboxDistance(GameObject obj, GameObject obj2)
        {
            return obj.Position.Distance(obj2.Position) + obj.BoundingRadius + obj2.BoundingRadius - 8;
        }

    }
}
