using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class PlayerReport : GraphQLQuarriable, IFromvrChatLogitemJOSN, IFirstLast
    {
        public string name ="";
        public long fristseen =0;
        public long lastseen =0;
        public int eventsjoined =0;
        public string firstEventID = "";


        public PlayerReport() { }

        public void firstLastUpdate(long time)
        {
            if (fristseen > time)
            {
                fristseen = time;
            }
            if (lastseen < time)
            {
                lastseen = time;
            }
        }

        public void fromvrChatLogitemJOSN(vrChatLogitemJOSN item)
        {
            name = item.PlayerName;
            fristseen = item.time;
            lastseen = item.time;
            eventsjoined = 0;
        }
        public override string GetGraphQLSuffix()
        {
            return "PlayerReport";
        }
        public override string ListGraphQLProperties()
        {
            return base.ListGraphQLProperties();
        }
        public override string GetGetField()
        {
            return $"name: \"{name}\"";
        }
        public override bool IsEmpty()
        {
            return name.Equals("");
        }

        public override string GetHashKey()
        {
            return name;
        }

        public override string GetTableName()
        {
            return "PlayerReport-yqjhtslhmngtjgdb3t5ifhsbza-master";
        }
    }
}
