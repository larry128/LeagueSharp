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

    public class WardCast : ExpectedChange
    {
    }

    public class BuyItem : ExpectedChange
    {
    }

    public class SellItem : ExpectedChange
    {
    }

    public class SpellLeveledUp : ExpectedChange
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

    public class EnemyDamaged : ExpectedChange
    {
        public readonly float amount;
        public readonly Obj_AI_Hero[] who;

        public EnemyDamaged(Obj_AI_Hero[] who, float amount)
        {
            this.amount = amount;
            this.who = who;
        }
    }

    public class EnemyStunned : ExpectedChange
    {
    }

    public class EnemySlowed : ExpectedChange
    {
    }

    public class AllyHealed : ExpectedChange
    {
        public readonly float amount;
        public readonly Obj_AI_Hero[] who;

        public AllyHealed(Obj_AI_Hero[] who, float amount)
        {
            this.amount = amount;
            this.who = who;
        }
    }

    public class AllyHastened : ExpectedChange
    {
    }
}