using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace najsvan
{
    public static class Constants
    {
        public static readonly Logger LOG = Logger.GetLogger("AI");
        public static readonly HashSet<ServerInteraction> SERVER_INTERACTIONS = new HashSet<ServerInteraction>();
        public static readonly Obj_AI_Hero MY_HERO = ObjectManager.Player;
        public static readonly SpellSlot SUMMONER_HEAL = MY_HERO.GetSpellSlot("summonerheal");
        public static readonly SpellSlot SUMMONER_IGNITE = MY_HERO.GetSpellSlot("summonerflash");
        public static readonly SpellSlot SUMMONER_FLASH = MY_HERO.GetSpellSlot("summonerdot");
        public static readonly GameObjectTeam ALLY_TEAM = ObjectManager.Player.Team;
        public static readonly GameObjectTeam ENEMY_TEAM = ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Order : GameObjectTeam.Chaos;
        public static readonly int SCAN_DISTANCE = 1400;
        public static readonly int BASE_PER_LVL_HP = 77;
        public static readonly int BASE_LVL1_HP = 600;
        public static readonly double FEAR_UNDER_PERCENT = 0.5;
        public static readonly double DANGER_UNDER_PERCENT = 0.25;
        public static readonly int TICK_DELAY = 50;
        public static readonly int WARD_PLACE_DISTANCE = 600;
        public static readonly int SUMMONER_HEAL_RANGE = 700;
        public static readonly int SUMMONER_IGNITE_RANGE = 600;
        public static readonly int MIKAELS_RANGE = 750;
        public static readonly int QUEENS_RANGE = 750;
        public static readonly int WARD_SIGHT_RADIUS = 1200;
        public static readonly int TURRET_RANGE = 950;
        public static readonly int DANGER_COOLDOWN = 3;
        public static readonly String TARGETED_BY_TOWER_OBJ_NAME = "yikes";

        public static readonly Dictionary<GameObjectTeam, List<WardSpot>> WARD_SPOTS = new Dictionary
            <GameObjectTeam, List<WardSpot>>
        {
            {
                GameObjectTeam.Neutral, new List<WardSpot>
                {
                    new WardSpot(6871, 3074),
                    new WardSpot(5580, 3543),
                    new WardSpot(6552, 4736),
                    new WardSpot(8542, 4802),
                    new WardSpot(8103, 6267),
                    new WardSpot(10796, 5213),
                    new WardSpot(11547, 7099),
                    new WardSpot(9965, 6572),
                    new WardSpot(8716, 6721),
                    new WardSpot(9942, 7872),
                    new WardSpot(6813, 8580),
                    new WardSpot(6258, 8128),
                    new WardSpot(4944, 8482),
                    new WardSpot(4811, 7125),
                    new WardSpot(3356, 7782),
                    new WardSpot(2387, 9698),
                    new WardSpot(2952, 11221),
                    new WardSpot(4453, 11836),
                    new WardSpot(5733, 12766),
                    new WardSpot(6755, 11500),
                    new WardSpot(8010, 11845),
                    new WardSpot(9249, 11446),
                    new WardSpot(8298, 10283)
                }
            },
            {
                GameObjectTeam.Order, new List<WardSpot>
                {
                    new WardSpot(11897, 3696),
                    new WardSpot(10477, 3101),
                    new WardSpot(12605, 5112)
                }
            },
            {
                GameObjectTeam.Chaos, new List<WardSpot>
                {
                    new WardSpot(11897, 3696),
                    new WardSpot(10477, 3101),
                    new WardSpot(12605, 5112)
                }
            }
        };

        private static readonly Dictionary<int, HeroInfo> HERO_INFO_DICT = new Dictionary<int, HeroInfo>();
        public static HeroInfo GetHeroInfo(Obj_AI_Hero hero)
        {
            HeroInfo result;
            if (!HERO_INFO_DICT.TryGetValue(hero.NetworkId, out result))
            {
                result = new HeroInfo(hero.NetworkId);
                HERO_INFO_DICT.Add(hero.NetworkId, result);
            }
            return result;
        }
    }
}