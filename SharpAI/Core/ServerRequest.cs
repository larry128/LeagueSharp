using System;
using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public abstract class ServerRequest
    {
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public class SpellLeveledUp : ServerRequest
    {
    }

    public class SpellCast : ServerRequest
    {
        private readonly String spellName;

        public SpellCast(String spellName)
        {
            this.spellName = spellName;
        }

        public override string ToString()
        {
            return spellName;
        }
    }

    public class BuyItem : ServerRequest
    {
    }

    public class SellItem : ServerRequest
    {
    }

    public class HoldingPosition : ServerRequest
    {
    }

    public class AutoAttack : ServerRequest
    {
    }

    public class MovingTo : ServerRequest
    {
        public readonly Vector3 destination;

        public MovingTo(Vector3 destination)
        {
            this.destination = destination;
        }
    }

    public class WardUsed : ServerRequest
    {
        public readonly InventorySlot wardSlot;

        public WardUsed(InventorySlot wardSlot)
        {
            this.wardSlot = wardSlot;
        }
    }
}