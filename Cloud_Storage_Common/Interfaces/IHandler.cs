using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Storage_Common.Interfaces
{
  public  interface IHandler
    {
        IHandler SetNext(IHandler handler);
        object Handle(object Request);

    }
}
