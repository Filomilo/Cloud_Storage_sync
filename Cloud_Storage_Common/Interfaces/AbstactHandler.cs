using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common.Interfaces
{
  public  abstract class   AbstactHandler: IHandler
    {
        protected IHandler _nextHandler;

        public IHandler SetNext(IHandler handler)
        {
            this._nextHandler = handler;

            return handler;
        }

       abstract public object Handle(object request);
    }
}
