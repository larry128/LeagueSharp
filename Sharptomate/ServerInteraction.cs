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

        public override bool Equals(object obj)
        {
            try
            {
                return ((ServerInteraction) obj).request.ToString().Equals(this.request.ToString());
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return request.ToString().GetHashCode();
        }
    }
}