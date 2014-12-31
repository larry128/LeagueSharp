namespace najsvan
{
    public class ServerInteraction
    {
        public delegate void Action();

        public readonly ExpectedChange change;
        public readonly Action serverAction;

        public ServerInteraction(ExpectedChange change, Action serverAction)
        {
            this.change = change;
            this.serverAction = serverAction;
        }
    }
}