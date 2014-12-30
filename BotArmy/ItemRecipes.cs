using System.Collections.Generic;
using LeagueSharp;

namespace najsvan
{
    public class ItemRecipes
    {
        private static readonly Dictionary<ItemId, ItemId[]> RECIPES = new Dictionary<ItemId, ItemId[]>
        {
            {ItemId.Frostfang, new[] {ItemId.Spellthiefs_Edge}},
            {ItemId.Frost_Queens_Claim, new[] {ItemId.Frostfang, ItemId.Fiendish_Codex}},
            {ItemId.Fiendish_Codex, new[] {ItemId.Amplifying_Tome}},
            {ItemId.Chalice_of_Harmony, new[] {ItemId.Null_Magic_Mantle, ItemId.Faerie_Charm, ItemId.Faerie_Charm}},
            {ItemId.Forbidden_Idol, new[] {ItemId.Faerie_Charm, ItemId.Faerie_Charm}},
            {ItemId.Haunting_Guise, new[] {ItemId.Amplifying_Tome, ItemId.Ruby_Crystal}},
            {ItemId.Sorcerers_Shoes, new[] {ItemId.Boots_of_Speed}},
            {ItemId.Mikaels_Crucible, new[] {ItemId.Forbidden_Idol, ItemId.Chalice_of_Harmony}},
            {ItemId.Void_Staff, new[] {ItemId.Blasting_Wand, ItemId.Amplifying_Tome}},
            {ItemId.Liandrys_Torment, new[] {ItemId.Haunting_Guise, ItemId.Amplifying_Tome}},
            {ItemId.Rylais_Crystal_Scepter, new[] {ItemId.Giants_Belt, ItemId.Blasting_Wand, ItemId.Amplifying_Tome}},
            {ItemId.Sorcerers_Shoes_Enchantment_Homeguard, new[] {ItemId.Sorcerers_Shoes}},
            {ItemId.Greater_Stealth_Totem_Trinket, new[] {ItemId.Warding_Totem_Trinket}},
            {ItemId.Greater_Vision_Totem_Trinket, new[] {ItemId.Warding_Totem_Trinket}}
        };

        public static ItemId[] GetRecipe(ItemId itemId)
        {
            ItemId[] recipe;
            RECIPES.TryGetValue(itemId, out recipe);
            return recipe;
        }
    }
}