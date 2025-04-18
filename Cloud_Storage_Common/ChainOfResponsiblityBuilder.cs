using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Interfaces;

namespace Cloud_Storage_Common
{
    public class ChainOfResponsiblityBuilder
    {
        private IHandler startingHandler;
        private IHandler prevHandler;

        public ChainOfResponsiblityBuilder Next(IHandler handler)
        {
            if (startingHandler == null)
            {
                startingHandler = handler;
                prevHandler = handler;
            }
            else
            {
                prevHandler.SetNext(handler);
                prevHandler = handler;
            }
            return this;
        }

        public IHandler Build()
        {
            return startingHandler;
        }
    }
}
