using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRchatGroupDataCollector
{
    public class vrChatLogitemJOSN
    {
        //FEClogsData
        //feclogsGraphQL
        public int itemID { get; }
        public long time;
        public int EventID;
        public string type;
        public string PlayerName;
        public int playerCount;
        public string worldID;
        public int instanceID;
        public string instanceType;
        public string CreatorID { get; private set; }
        public string GroupAccessType { get; private set; }
        public string region;
        public string roomName;

        


        public vrChatLogitemJOSN()
        { 
        }
            public vrChatLogitemJOSN(vrChatLogItem item)
        {
            
            this.time = ((DateTimeOffset)item.time).ToUnixTimeSeconds();
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
            this.EventID= item.instance.ToString().GetHashCode();
            itemID =$"{type} {PlayerName} {playerCount} {item.instance} {time}".GetHashCode();
            
        }

        public string toGraphQLcreationString() 
        {
            /*
            Newtonsoft.Json.JsonConvert.SerializeObject()
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            string query = "mutation MyMutation {\r\n  createPlayerEvent(input: "+ data+") {\r\n    id\r\n  }\r\n}";
            */
            //string openbracket = "{";
            //string closebracket = "}";
            //long unixtimestamp=  ((DateTimeOffset)time).ToUnixTimeSeconds();
            //string query = $"mutation MyMutation {openbracket}\r\n  createPlayerEvent(input: {openbracket} time: {unixtimestamp}, EventID: {EventID}, GroupAccessType: \"{GroupAccessType}\", PlayerName: \"{PlayerName}\", _version: 10, instanceID: {instanceID}, itemID: {itemID}, instanceType: \"{instanceType}\", playerCount: {playerCount}, roomName: \"{roomName}\", worldID: \"{worldID}\", type: \"{instanceType}\", region: \"{region}\"{closebracket}) \r\n{closebracket}";
            string query = $"mutation MyMutation {{\r\n  createPlayerEvent(input: {{EventID: {EventID}, GroupAccessType: \"{GroupAccessType}\", PlayerName: \"{PlayerName}\", _version: 10, instanceID: {instanceID}, instanceType: \"{instanceType}\", itemID: {itemID}, playerCount: {playerCount}, region: \"{region}\", roomName: \"{roomName}\", time: {time}, type: \"{type}\", worldID: \"{worldID}\"}}){{\r\n        _deleted\r\n    }} \r\n}}";
            return query;
        }
    }
}
