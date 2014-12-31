using SharpDX;

namespace najsvan
{
    public class WardSpot
    {
        private readonly Vector2 position;
        private readonly bool spellWardOnly;

        public WardSpot(float x, float y)
        {
            position = new Vector2(x, y);
            spellWardOnly = false;
        }

        public WardSpot(float x, float y, bool spellWardOnly)
        {
            position = new Vector2(x, y);
            this.spellWardOnly = spellWardOnly;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public bool GetSpellWardOnly()
        {
            return spellWardOnly;
        }
    }
}