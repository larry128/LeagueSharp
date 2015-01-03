using LeagueSharp;
using SharpDX;

namespace najsvan
{
    public abstract class ExpectedChange
    {
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public class SpellLeveledUp : ExpectedChange
    {
    }

    public class SpellCast : ExpectedChange
    {
    }

    public class BuyItem : ExpectedChange
    {
    }

    public class SellItem : ExpectedChange
    {
    }

    public class HoldingPosition : ExpectedChange
    {
    }

    public class MovingTo : ExpectedChange
    {
        public readonly Vector3 destination;

        public MovingTo(Vector3 destination)
        {
            this.destination = destination;
        }
    }

    public class WardUsed : ExpectedChange
    {
        public readonly InventorySlot wardSlot;

        public WardUsed(InventorySlot wardSlot)
        {
            this.wardSlot = wardSlot;
        }
    }
}