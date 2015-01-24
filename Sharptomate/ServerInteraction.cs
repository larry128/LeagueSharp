namespace najsvan
{
    public class ServerInteraction
    {
        public delegate void Action();

        public readonly ServerRequest request;
        public readonly Action serverAction;

        public ServerInteraction(ServerRequest request, Action serverAction)
        {
            this.request = request;
            this.serverAction = serverAction;
        }
    }
}