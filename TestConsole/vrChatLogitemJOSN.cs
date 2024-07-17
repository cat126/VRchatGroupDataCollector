using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class vrChatLogitemJOSN
    {
        public DateTime time;
        public string type;
        public string PlayerName;
        public int playerCount;
        //public vrChatinstance instance;
        public string itemID { get; }


        public string worldID;
        public int instanceID;
        public string instanceType;
        public string CreatorID { get; private set; }
        public string GroupAccessType { get; private set; }
        public string region;
        public string roomName;

        public string EventID;


        public vrChatLogitemJOSN()
        {
            /*
             * vrChatLogItem item
            this.time = item.time.ToUniversalTime();
            this.playerCount = item.playerCount;
            //this.instance = item.instance;
            string temp = item.item;
            temp = temp.Trim();
            var temp2 = temp.Split(' ');
            this.type = temp2[0].Trim();
            this.PlayerName = temp2[1].Trim();
            this.worldID = item.instance.worldID;
            this.instanceID = item.instance.instanceID;
            this.instanceType = item.instance.instanceType;
            this.CreatorID = item.instance.CreatorID;
            this.GroupAccessType = item.instance.GroupAccessType;
            this.region = item.instance.region;
            this.roomName = item.instance.roomName;
            this.EventID= item.instance.ToString().GetHashCode()+"";
            itemID =$"{type} {PlayerName} {playerCount} {item.instance} {time}".GetHashCode()+"";
            */

        }
    }
}
