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
        public string itemID;
        public long time;
        public string EventID;
        public string type;
        public string PlayerName;
        public int playerCount;
        public string worldID;
        public int instanceID;
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
            this.time = ((DateTimeOffset)item.time).ToUnixTimeSeconds();
            this.playerCount = item.playerCount;
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
            string query = $"mutation MyMutation {{\r\n  createPlayerEvent(input: {{EventID: \"{EventID}\", GroupAccessType: \"{GroupAccessType}\", PlayerName: \"{PlayerName}\", instanceID: {instanceID}, itemID: \"{itemID}\", playerCount: {playerCount}, region: \"{region}\", roomName: \"{roomName}\", time: {time}, type: \"{type}\", worldID: \"{worldID}\", logFrom: \"{logFrom}\"}}){{\r\n        _deleted\r\n    }} \r\n}}";
            return query;
        }
    }
}
