namespace najsvan
{
    public abstract class Clearable
    {
        protected bool clear = true;

        public void Clear()
        {
            clear = true;
        }
    }
}