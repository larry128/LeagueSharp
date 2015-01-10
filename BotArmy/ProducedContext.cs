using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;

namespace najsvan
{
    public static class ProducedContext
    {
        public static readonly Produced<List<GameObject>> WARDS = new Produced<List<GameObject>>(Producer_Wards);
        public static readonly Produced<bool> IS_MY_HERO_SAFE = new Produced<bool>(Producer_IsMyHeroSafe);
        public static readonly Produced<List<Obj_AI_Hero>> ALL_ALLIES = new Produced<List<Obj_AI_Hero>>(Producer_AllAllies);
        public static readonly Produced<List<Obj_AI_Hero>> ALL_ENEMIES = new Produced<List<Obj_AI_Hero>>(Producer_AllEnemies);
        public static readonly Produced<List<Obj_AI_Turret>> ALLY_TURRETS = new Produced<List<Obj_AI_Turret>>(Producer_AllyTurrets);
        public static readonly Produced<List<Obj_AI_Turret>> ENEMY_TURRETS = new Produced<List<Obj_AI_Turret>>(Producer_EnemyTurrets);
        public static readonly Produced<Obj_SpawnPoint> ALLY_SPAWN = new Produced<Obj_SpawnPoint>(Producer_AllySpawn);
        public static readonly Produced<Obj_SpawnPoint> ENEMY_SPAWN = new Produced<Obj_SpawnPoint>(Producer_EnemySpawn);

        public static void Clear()
        {
            var fields = typeof (ProducedContext).GetFields();
            foreach (var field in fields)
            {
                var prod = field.GetRawConstantValue();
                ((Clearable)prod).Clear();
            }
        }

        private static List<GameObject> Producer_Wards()
        {
            return
                LibraryOfAlexandria.ProcessEachGameObject<GameObject>(
                    obj => obj.IsValid && obj.IsVisible && obj.IsAlly && obj.Name.ToLower().Contains("ward"));
        }

        private static bool Producer_IsMyHeroSafe()
        {
            return LibraryOfAlexandria.IsAllySafe(GenericContext.MY_HERO);
        }

        private static List<Obj_AI_Hero> Producer_AllAllies()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_AI_Hero>(hero => hero.IsAlly);
        }

        private static List<Obj_AI_Hero> Producer_AllEnemies()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_AI_Hero>(hero => !hero.IsAlly);
        }

        private static List<Obj_AI_Turret> Producer_AllyTurrets()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_AI_Turret>(turret => turret.IsAlly);
        }

        private static List<Obj_AI_Turret> Producer_EnemyTurrets()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_AI_Turret>(turret => !turret.IsAlly);
        }

        private static Obj_SpawnPoint Producer_AllySpawn()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_SpawnPoint>(spawn => spawn.IsAlly).First();
        }

        private static Obj_SpawnPoint Producer_EnemySpawn()
        {
            return LibraryOfAlexandria.ProcessEachGameObject<Obj_SpawnPoint>(spawn => !spawn.IsAlly).First();
        }
    }
}