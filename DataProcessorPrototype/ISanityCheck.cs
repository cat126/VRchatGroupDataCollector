using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessorPrototype
{
    internal interface ISanityCheck
    {
        bool SanityCheck(log4net.ILog log);
    }
}
