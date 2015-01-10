namespace najsvan
{
    public class Produced<T> : Clearable
    {
        public delegate T Producer();

        private readonly Producer producer;
        private T value;

        public Produced(Producer producer)
        {
            this.producer = producer;
        }

        public T Get()
        {
            if (clear)
            {
                value = producer();
                clear = false;
            }
            return value;
        }
    }
}