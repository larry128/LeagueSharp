using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class KarthusBot : GenericBot
    {
        public KarthusBot()
        {
            GenericContext.levelSpellsOrder = new[]
            {
                SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W,
                SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R,
                SpellSlot.E, SpellSlot.E
            };

            GenericContext.shoppingList = new[]
            {
                ItemId.Warding_Totem_Trinket, ItemId.Spellthiefs_Edge, ItemId.Frostfang, ItemId.Faerie_Charm,
                ItemId.Faerie_Charm, ItemId.Ruby_Crystal, ItemId.Null_Magic_Mantle, ItemId.Chalice_of_Harmony,
                ItemId.Faerie_Charm, ItemId.Faerie_Charm, ItemId.Forbidden_Idol, ItemId.Amplifying_Tome,
                ItemId.Haunting_Guise, ItemId.Boots_of_Speed, ItemId.Sorcerers_Shoes, ItemId.Amplifying_Tome,
                ItemId.Fiendish_Codex, ItemId.Mikaels_Crucible, ItemId.Blasting_Wand, ItemId.Frost_Queens_Claim,
                ItemId.Amplifying_Tome, ItemId.Void_Staff, ItemId.Amplifying_Tome, ItemId.Liandrys_Torment,
                ItemId.Giants_Belt, ItemId.Rylais_Crystal_Scepter, ItemId.Sorcerers_Shoes_Enchantment_Homeguard
            };

            GenericContext.shoppingListConsumables = new[] {ItemId.Stealth_Ward, ItemId.Mana_Potion};

            GenericContext.shoppingListElixir = ItemId.Elixir_of_Sorcery;

            Game.PrintChat(GetType().Name + " - Loaded");
        }

        public override void Action_ZombieCast(Node node, string stack)
        {
        }

        protected override Spell GetWardSpell()
        {
            return null;
        }

        public override bool Condition_WillInterruptSelf(Node node, string stack)
        {
            // karthus can't interrupt his skills
            return false;
        }

        public override void Action_CastSafeSpells(Node node, string stack)
        {
        }

        public override void Action_RecklessCastSpells(Node node, string stack)
        {
        }

        public override void Action_RecklessAutoAttack(Node node, string stack)
        {
        }

        public override bool Action_RecklessMove(Node node, string stack)
        {
            return false;
        }

        public override void Action_Move(Node node, string stack)
        {
        }
    }
}