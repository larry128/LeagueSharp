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