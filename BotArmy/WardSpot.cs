using SharpDX;

namespace najsvan
{
    public class WardSpot
    {
        private Vector2 position;
        private readonly bool skillWardOnly;

        public WardSpot(float x, float y)
        {
            position = new Vector2(x, y);
            skillWardOnly = false;
        }

        public WardSpot(float x, float y, bool skillWardOnly)
        {
            position = new Vector2(x, y);
            this.skillWardOnly = skillWardOnly;
        }

        public float GetX()
        {
            return position.X;
        }

        public float GetY()
        {
            return position.Y;
        }

        public bool GetSkillWardOnly()
        {
            return skillWardOnly;
        }
    }
}