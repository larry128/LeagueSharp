using System.Collections.Generic;
using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public class KarthusAI : GenericAI
    {
        public KarthusAI()
        {
            levelSpellsOrder = new[]
            {
                SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W,
                SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R,
                SpellSlot.E, SpellSlot.E
            };

            shoppingList = new[]
            {
                ItemId.Warding_Totem_Trinket, ItemId.Spellthiefs_Edge, ItemId.Frostfang, ItemId.Faerie_Charm,
                ItemId.Faerie_Charm, ItemId.Ruby_Crystal, ItemId.Null_Magic_Mantle, ItemId.Chalice_of_Harmony,
                ItemId.Faerie_Charm, ItemId.Faerie_Charm, ItemId.Forbidden_Idol, ItemId.Amplifying_Tome,
                ItemId.Haunting_Guise, ItemId.Boots_of_Speed, ItemId.Sorcerers_Shoes, ItemId.Amplifying_Tome,
                ItemId.Fiendish_Codex, ItemId.Mikaels_Crucible, ItemId.Blasting_Wand, ItemId.Frost_Queens_Claim,
                ItemId.Amplifying_Tome, ItemId.Void_Staff, ItemId.Amplifying_Tome, ItemId.Liandrys_Torment,
                ItemId.Giants_Belt, ItemId.Rylais_Crystal_Scepter, ItemId.Sorcerers_Shoes_Enchantment_Homeguard
            };

            shoppingListConsumables = new Stack<ItemId>();
            shoppingListConsumables.Push(ItemId.Stealth_Ward); 
            shoppingListConsumables.Push(ItemId.Mana_Potion); 
            shoppingListConsumables.Push(ItemId.Mana_Potion);

            shoppingListElixir = ItemId.Elixir_of_Sorcery;

            Game.PrintChat(GetType().Name + " - Loaded");
        }

        public override void Action_ZombieCast(Node node, string stack)
        {
            // QWER
        }

        public override bool IsWardSpellReady()
        {
            // W ready and enough mana and don't see all enemies alive and not fighting
            return false;
        }

        public override float GetWardSpellRange()
        {
            // just some pytghoras
            return 0;
        }

        public override void WardSpellCast(Vector2 position)
        {
            // W
        }

        public override bool Condition_WillInterruptSelf(Node node, string stack)
        {
            // karthus cant really interrupt himself
            return false;
        }

        public override void Action_DoRecklesslyButDontInterruptSelf(Node node, string stack)
        {
            // karthus cant really interrupt himself
        }

        public override void Action_DoRecklessly(Node node, string stack)
        {
            // E
        }

        public override bool Action_RecklessMove(Node node, string stack)
        {
            // go kill someone if you can do it in a few seconds (? flash ? ... if you can get out safe)
            return false;
        }

        public override void Action_DoIfNotInDanger(Node node, string stack)
        {
            // Q
            // W
        }

        public override void Action_DoIfSafe(Node node, string stack)
        {
            // R
        }

        public override void Action_Move(Node node, string stack)
        {
            // try to harass or
            // delegate to supportmovement or other movement class
        }
    }
}