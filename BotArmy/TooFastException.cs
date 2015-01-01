using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace najsvan
{
    class TooFastException : Exception
    {
        public TooFastException()
            : base("JSONBTree node requests to be executed later")
        {
        }
    }
}
