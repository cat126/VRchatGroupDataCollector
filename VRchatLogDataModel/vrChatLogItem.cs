using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VRchatLogDataModel
{
    public class vrChatLogItem
    {
        public DateTime time;

        public string type1;
        public string type2;
        public string item;
        public int playerCount;
        public vrChatinstance instance;
        public vrChatLogItem(string rawText)
        {
            string timeStampPattern = "(\\d\\d\\d\\d)\\.(\\d\\d)\\.(\\d\\d)([^\\d]+)(\\d+):(\\d+):(\\d+) (\\w+)(\\s+)-(\\s+)(\\[[^\\s\\{]+\\])?(.+)";
            //Console.WriteLine(timeStampPattern);
            RawText = rawText;
            this.item = "";
            this.playerCount = 0;
            //time
            var PatternMacth = Regex.Match(rawText, timeStampPattern);
            if (PatternMacth.Success)
            {
                var logParts = PatternMacth.Groups;
                //2024.06.21 17:11:47 Log
                // year, month, day, hour, minute, and second.
                int year = int.Parse(logParts[1].Value);
                int month = int.Parse(logParts[2].Value);
                int day = int.Parse(logParts[3].Value);
                int hour = int.Parse(logParts[5].Value);
                int minute = int.Parse(logParts[6].Value);
                int second = int.Parse(logParts[7].Value);
                this.time = new DateTime(year, month, day, hour, minute, second);
                this.type1 = logParts[8].Value;
                this.type2 = logParts[11].Value;
                this.item = logParts[12].Value;
                //Console.WriteLine(time+" "+this.type1+this.type2+this.item);
            }
            //end time

        }

        public string RawText { get; }

        internal string googleSheetFormat()
        {
            //6/22/2024 11:20:33
            //OnPlayerLeft
            //OnPlayerJoined
            if (!(this.item.Contains("OnPlayerJoined") | this.item.Contains("OnPlayerLeft"))) 
            {
                return "";
            }
            string output = "";
            DateTime utcTime= this.time.ToUniversalTime();
            string temp =this.item;
            temp=temp.Trim();
            var temp2=temp.Split(' ');
            temp = $"{temp2[0]}, {temp2[1]}";
            output = $"{utcTime.Month}/{utcTime.Day}/{utcTime.Year} {utcTime.Hour}:{utcTime.Minute}:{utcTime.Second}, {this.instance.roomName}, {this.instance.instanceID} ,{this.instance.region}, {temp}, {this.playerCount}";

            return output;
        }

        public vrChatLogitemJOSN ToPlayeritem(string from)
        {
            return new vrChatLogitemJOSN(this, from); 
        }
    }
}
