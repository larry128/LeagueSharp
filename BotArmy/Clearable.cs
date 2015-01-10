using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
