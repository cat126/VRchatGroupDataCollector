using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class AttendaceLog: GraphQLQuarriable, IFromvrChatLogitemJOSN, IComparable<AttendaceLog>
    {
        public string itemID = "";
        public string name ="";
        public long time =0;
        public bool joined = false;
        public int playerCount = 0;
        public string eventReportAttendaceLogEventID = "";
        public AttendaceLog() { }
        public AttendaceLog(string itemID, string name, long time, bool joined, int playerCount, string eventID)
        {
            this.itemID = itemID; 
            this.name = name;
            this.time = time;
            this.joined = joined;
            this.playerCount = playerCount;
            this.eventReportAttendaceLogEventID = eventID;
        }

        public int CompareTo(AttendaceLog item)
        {
            int test = this.time.CompareTo(item.time);
            if (test == 0) 
            {
                return this.playerCount.CompareTo(item.playerCount);
            }
            return test;
        }
        public override string ToString() 
        {
            return $"{time} {name} {joined} {playerCount}";
        }

        public override string GetGraphQLSuffix()
        {
            return "AttendaceLog";
        }
        public override string ListGraphQLProperties()
        {
            return base.ListGraphQLProperties();
        }
        public override bool IsEmpty()
        {
            return name.Equals("");
        }

        public override string GetHashKey()
        {
            return itemID;
        }

        public override string GetTableName()
        {
            return "AttendaceLog-yqjhtslhmngtjgdb3t5ifhsbza-master";
        }

        public void fromvrChatLogitemJOSN(vrChatLogitemJOSN item)
        {
            
            this.itemID = item.itemID;
            this.name = item.PlayerName;
            this.time = item.time;
         
            this.playerCount = item.playerCount;
            this.eventReportAttendaceLogEventID = item.EventID;
            if (item.type.Contains("OnPlayerJoined"))
            {
                this.joined = true;
            }
            else 
            {
                this.joined = false;
            }
        }
    }
}
