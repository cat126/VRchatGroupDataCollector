using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public interface IFromvrChatLogitemJOSN
    {
        void fromvrChatLogitemJOSN(vrChatLogitemJOSN item);
    }
}
