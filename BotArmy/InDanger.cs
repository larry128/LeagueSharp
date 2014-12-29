using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace najsvan
{
    public class InDanger
    {
        private static readonly Logger LOG = Logger.GetLogger("InDanger");
        private Context context;
        private ProducedContext producedContext;

        public InDanger(Context context, ProducedContext producedContext)
        {
            this.context = context;
            this.producedContext = producedContext;
        }
    }
}
