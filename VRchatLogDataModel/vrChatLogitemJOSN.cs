using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VRchatLogDataModel
{
    public class vrChatLogitemJOSN
    {
        //FEClogsData
        //feclogsGraphQL
        public string itemID;
        public DateTime time;
        public string EventID;
        public string type;
        public string PlayerName;
        public int playerCount;
        public string worldID;
        public int instanceID;
        //public string instanceType;
        public string CreatorID;
        public string GroupAccessType;
        public string region;
        public string roomName;
        public string logFrom;





        public vrChatLogitemJOSN()
        { 
        }
            public vrChatLogitemJOSN(vrChatLogItem item, string from)
        {
            this.logFrom = from;
            this.time = item.time.ToUniversalTime();
            this.playerCount = item.playerCount;
            //this.instance = item.instance;
            string temp = item.item;
            temp = temp.Trim();
            var temp2 = temp.Split(' ');
            this.type = temp2[0].Trim();
            this.PlayerName = "";
            for (int i = 1; i < temp2.Length; i++) 
            {
                this.PlayerName += System.Web.HttpUtility.JavaScriptStringEncode(temp2[i].Trim())+" ";
            }
            this.PlayerName= PlayerName.Trim();
            this.worldID = item.instance.worldID;
            this.instanceID = item.instance.instanceID;
            //this.instanceType = item.instance.instanceType;
            this.CreatorID = item.instance.CreatorID;
            this.GroupAccessType = item.instance.GroupAccessType;
            string GroupAccessTypePattern = "~groupAccessType\\((.+)\\)";
            var PatternMacth = Regex.Match(this.GroupAccessType, GroupAccessTypePattern);
            this.GroupAccessType = PatternMacth.Groups[1].Value;
            this.region = item.instance.region;
            this.roomName = System.Web.HttpUtility.JavaScriptStringEncode(item.instance.roomName);
            string eventidbeforhash = $"|{this.instanceID} {this.GroupAccessType} {this.region} {this.roomName}|";
            
            this.EventID= LogProcessor.CreateMD5(eventidbeforhash);
            this.itemID = LogProcessor.CreateMD5($"|{type} {PlayerName} {playerCount} {this.EventID} {time}|");
            
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
            long unixtimestamp=  ((DateTimeOffset)time).ToUnixTimeSeconds();
            //string query = $"mutation MyMutation {openbracket}\r\n  createPlayerEvent(input: {openbracket} time: {unixtimestamp}, EventID: {EventID}, GroupAccessType: \"{GroupAccessType}\", PlayerName: \"{PlayerName}\", _version: 10, instanceID: {instanceID}, itemID: {itemID}, instanceType: \"{instanceType}\", playerCount: {playerCount}, roomName: \"{roomName}\", worldID: \"{worldID}\", type: \"{instanceType}\", region: \"{region}\"{closebracket}) \r\n{closebracket}";
            string query = $"mutation MyMutation {{\r\n  createPlayerEvent(input: {{EventID: \"{EventID}\", GroupAccessType: \"{GroupAccessType}\", PlayerName: \"{PlayerName}\", _version: 10, instanceID: {instanceID}, itemID: \"{itemID}\", playerCount: {playerCount}, region: \"{region}\", roomName: \"{roomName}\", time: {unixtimestamp}, type: \"{type}\", worldID: \"{worldID}\", logFrom: \"{logFrom}\"}}){{\r\n        _deleted\r\n    }} \r\n}}";
            return query;
        }
    }
}
