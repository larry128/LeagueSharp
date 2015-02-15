namespace najsvan
{
    public class Produced<T> : Clearable
    {
        public delegate T Producer();

        private T value;
        private readonly Producer producer;

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