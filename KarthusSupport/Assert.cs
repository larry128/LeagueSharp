using System;

namespace najsvan
{
    public class Assert
    {
        public static void True(bool condition, String message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
