using System;

namespace najsvan
{
    internal class TooFastException : Exception
    {
        public TooFastException()
            : base("JSONBTree node requests to be executed later")
        {
        }
    }
}