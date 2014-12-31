using LeagueSharp;
using LeagueSharp.Common;

namespace najsvan
{
    public class KarthusBot : GenericBot
    {
        public KarthusBot()
            : base(new KarthusContext())
        {
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