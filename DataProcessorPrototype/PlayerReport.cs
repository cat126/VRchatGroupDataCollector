using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class PlayerReport : GraphQLQuarriable, IFromvrChatLogitemJOSN, IFirstLast, ISanityCheck
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

        public bool SanityCheck(ILog log)
        {
            

            if (fristseen < 1718830800) 
            {
                log.Error($"Sanity check failed for PlayerReport {GetHashKey()} firstSeen {fristseen} came before data collection started");
                return false;
            }

            if (lastseen < fristseen) 
            {
                log.Error($"Sanity check failed for PlayerReport {GetHashKey()} lastSeen {lastseen} came before firstSeen {fristseen}");
                return false;
            }
            if (eventsjoined <1)
            {
                log.Error($"Sanity check failed for PlayerReport {GetHashKey()} eventsJoined {eventsjoined} is less then 1");
                return false;
            }
            if (firstEventID.Equals("")) 
            {
                log.Error($"Sanity check failed for PlayerReport {GetHashKey()} no firstEventID");
                return false;
            }
           
            return true;
        }
    }
}
