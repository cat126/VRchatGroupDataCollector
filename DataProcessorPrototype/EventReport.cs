using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class EventReport : GraphQLQuarriable, IFromvrChatLogitemJOSN
    {
        public string eventID ="";
        public string worldID ="";
        public string worldName ="";
        public int instanceID =0;
        public string region ="";
        public int peekPlayers = 0;
        public int totalPlayers = 0;
        public int firstTimers = 0;
        public long firstlog = long.MaxValue;
        public long lastlog = 0;
        
        private LinkedList<AttendaceLog> attendaceLog = new LinkedList<AttendaceLog> ();
        public LinkedList<string> firstTimeNames = new LinkedList<string>();

        public void fromvrChatLogitemJOSN(vrChatLogitemJOSN item)
        {
            eventID = item.EventID;
            worldID = item.worldID;
            worldName = item.roomName;
            instanceID = item.instanceID;
            region = item.region;
            peekPlayers = 0;

        }
        public void addAttendaceLog(AttendaceLog item) 
        {
            if (item.time < firstlog) 
            {
                firstlog = item.time;
            }
            if (item.time > lastlog)
            {
                lastlog = item.time;
            }
           attendaceLog.AddLast(item);
           //Console.WriteLine(attendaceLog.Count);

        }
        public void finalize()
        {
            var attendaceLogArray= attendaceLog.ToArray();
            Array.Sort(attendaceLogArray);


            LinkedList<AttendaceLog> newAttendaceLog = new LinkedList<AttendaceLog>();
            LinkedList<string> allreadyhere = new LinkedList<string>();
            foreach (var item in attendaceLogArray)
            {
                if (item.joined  & allreadyhere.Contains(item.name)) 
                {
                    continue;
                }
                if (item.joined  & !allreadyhere.Contains(item.name))
                {
                    allreadyhere.AddLast(item.name);
                }
                if (!item.joined & !allreadyhere.Contains(item.name))
                {
                    continue;
                }
                if (!item.joined & allreadyhere.Contains(item.name))
                {
                   var toRemove= allreadyhere.Find(item.name);
                    allreadyhere.Remove(toRemove);
                }



                newAttendaceLog.AddLast(item);
            }
            attendaceLog= newAttendaceLog;
        }
        public override string GetGraphQLSuffix()
        {
            return "EventReport";
        }
        public override string ListGraphQLProperties()
        {
            return base.ListGraphQLProperties();
        }
        public override string GetGetField()
        {
            return $"eventID: \"{eventID}\"";
        }
        public override bool IsEmpty()
        {
            return eventID.Equals("");
        }
        public override string GetGetQuarry()
        {
            return
                $"query GetEventReport {{\r\n" +
                $"    getEventReport({GetGetField()}) {{\r\n" +
                $"        eventID\r\n" +
                $"        worldID\r\n" +
                $"        worldName\r\n" +
                $"        instanceID\r\n" +
                $"        region\r\n" +
                $"        peekPlayers\r\n" +
                $"        totalPlayers\r\n" +
                $"        firstTimers\r\n" +
                $"        firstlog\r\n" +
                $"        lastlog\r\n" +
                $"        attendaceLog {{\r\n" +
                $"            nextToken\r\n" +
                $"            startedAt\r\n" +
                $"        }}\r\n" +
                $"    }}\r\n}}";
        }
        public override string GetCreateQuarry()
        {
            return
                "mutation CreateEventReport {\r\n" +
                "    createEventReport(input: "+GetGraphQLData()+") {\r\n" +
                "        eventID\r\n" +
                "        worldID\r\n" +
                "        worldName\r\n" +
                "        instanceID\r\n" +
                "        region\r\n" +
                "        peekPlayers\r\n" +
                "        totalPlayers\r\n" +
                "        firstTimers\r\n" +
                "        firstlog\r\n" +
                "        lastlog\r\n" +
                "        createdAt\r\n" +
                "        updatedAt\r\n" +
                "        _version\r\n" +
                "        _deleted\r\n" +
                "        _lastChangedAt\r\n" +
                "    }\r\n" +
                "}"
                ;  
        }

        public override string GetHashKey()
        {
            return eventID;
        }

        public override string GetTableName()
        {
            return "EventReport-yqjhtslhmngtjgdb3t5ifhsbza-master";
        }

        
        public LinkedList<AttendaceLog> GetAttendaceLog() 
        {
            return this.attendaceLog;
        }

    }
}
