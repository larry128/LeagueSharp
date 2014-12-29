using System;

namespace najsvan
{
    public class Assert
    {
        public static void Fail(bool condition, String message)
        {
            False(true, message);
        }

        public static void True(bool condition, String message)
        {
            False(!condition, message);
        }

        public static void False(bool condition, String message)
        {
            if (condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
