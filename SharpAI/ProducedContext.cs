using System.Collections.Generic;
using System.Linq;
using LeagueSharp;

namespace najsvan
{
    public static class ProducedContext
    {
        public static void Clear()
        {
            var fields = typeof (ProducedContext).GetFields();
            var counter = 0;
            foreach (var field in fields)
            {
                var prod = field.GetValue(null);
                ((Clearable) prod).Clear();
                counter++;
            }
        }

        private static List<GameObject> Producer_Wards()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<GameObject>(
                    obj => obj.IsValid && obj.IsVisible && obj.IsAlly && obj.Name.ToLower().Contains("ward"));
        }

        private static List<Obj_AI_Hero> Producer_AllAllies()
        {
            var result = new List<Obj_AI_Hero>();
            ALL_HEROES.Get().ForEach(hero => { if (hero.IsAlly) result.Add(hero); });
            return result;
        }

        private static List<Obj_AI_Hero> Producer_AllEnemies()
        {
            var result = new List<Obj_AI_Hero>();
            ALL_HEROES.Get().ForEach(hero => { if (!hero.IsAlly) result.Add(hero); });
            return result;
        }

        private static List<Obj_AI_Hero> Producer_AllHeroes()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<Obj_AI_Hero>(
                    hero => !hero.IsDead && hero.IsValid && hero.IsVisible);
        }

        private static List<Obj_AI_Turret> Producer_AllyTurrets()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<Obj_AI_Turret>(
                    turret => turret.IsAlly && !turret.IsDead && turret.IsValid && turret.IsVisible);
        }

        private static List<Obj_AI_Turret> Producer_EnemyTurrets()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<Obj_AI_Turret>(
                    turret => !turret.IsAlly && !turret.IsDead && turret.IsValid && turret.IsVisible);
        }

        private static Obj_SpawnPoint Producer_AllySpawn()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<Obj_SpawnPoint>(spawn => spawn.IsAlly && spawn.IsValid)
                    .First();
        }

        private static Obj_SpawnPoint Producer_EnemySpawn()
        {
            return
                LibraryOfAIexandria.ProcessEachGameObject<Obj_SpawnPoint>(spawn => !spawn.IsAlly && spawn.IsValid)
                    .First();
        }

        public static readonly Produced<List<GameObject>> WARDS = new Produced<List<GameObject>>(Producer_Wards);

        public static readonly Produced<List<Obj_AI_Hero>> ALL_ALLIES =
            new Produced<List<Obj_AI_Hero>>(Producer_AllAllies);

        public static readonly Produced<List<Obj_AI_Hero>> ALL_ENEMIES =
            new Produced<List<Obj_AI_Hero>>(Producer_AllEnemies);

        public static readonly Produced<List<Obj_AI_Hero>> ALL_HEROES =
            new Produced<List<Obj_AI_Hero>>(Producer_AllHeroes);

        public static readonly Produced<List<Obj_AI_Turret>> ALLY_TURRETS =
            new Produced<List<Obj_AI_Turret>>(Producer_AllyTurrets);

        public static readonly Produced<List<Obj_AI_Turret>> ENEMY_TURRETS =
            new Produced<List<Obj_AI_Turret>>(Producer_EnemyTurrets);

        public static readonly Produced<Obj_SpawnPoint> ALLY_SPAWN = new Produced<Obj_SpawnPoint>(Producer_AllySpawn);
        public static readonly Produced<Obj_SpawnPoint> ENEMY_SPAWN = new Produced<Obj_SpawnPoint>(Producer_EnemySpawn);
    }
}