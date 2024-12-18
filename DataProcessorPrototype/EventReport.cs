using log4net;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class EventReport : GraphQLQuarriable, IFromvrChatLogitemJOSN, ISanityCheck
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
           

        }
        public void finalize()
        {
            peekPlayers = 0;
            totalPlayers = 0;
            LinkedList<string> allNames = new LinkedList<string>(); 
            var attendaceLogArray= attendaceLog.ToArray();
            Array.Sort(attendaceLogArray);


            LinkedList<AttendaceLog> newAttendaceLog = new LinkedList<AttendaceLog>();
            LinkedList<string> allreadyhere = new LinkedList<string>();
            bool instanceStarted = false;
            foreach (var item in attendaceLogArray)
            {
                if (item.joined  & allreadyhere.Contains(item.name)) 
                {
                    continue;
                }
                if (!item.joined & !allreadyhere.Contains(item.name))
                {
                    continue;
                }
                if (item.joined  & !allreadyhere.Contains(item.name))
                {
                    allreadyhere.AddLast(item.name);
                    instanceStarted = true;
                    totalPlayers++;
                }

                if (!item.joined & allreadyhere.Contains(item.name))
                {
                   var toRemove= allreadyhere.Find(item.name);
                    allreadyhere.Remove(toRemove);
                }
                if (instanceStarted) 
                {
                    newAttendaceLog.AddLast(item);
                    if (!allNames.Contains(item.name)) 
                    {
                        allNames.AddLast(item.name);
                    }
                }

                
                
            }
            attendaceLog= newAttendaceLog;

            int currentPlayerCount = 0;
            foreach (var item in attendaceLog) 
            {
                if (item.joined)
                {
                    currentPlayerCount++;

                }
                else 
                {
                    currentPlayerCount--;
                }
                if (currentPlayerCount<0) 
                {
                    currentPlayerCount = 0;
                }
                item.playerCount = currentPlayerCount;
                this.peekPlayers = Math.Max(this.peekPlayers, currentPlayerCount);
            }


            LinkedList<string> fixedFirstTimers = new LinkedList<string>();

            foreach (var item in firstTimeNames) 
            {
                if (!fixedFirstTimers.Contains(item) && allNames.Contains(item))
                {
                    fixedFirstTimers.AddFirst(item);
                }
            }
            firstTimeNames = fixedFirstTimers;
            firstTimers = firstTimeNames.Count;
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

        public bool SanityCheck(ILog log)
        {
            if (firstlog> lastlog) 
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} firstlog {firstlog} came after lastlog {lastlog}");
                return false;
            }

            if (peekPlayers < 1) 
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} peekPlayers {peekPlayers} is less than 1");
                return false;
            }
            if( totalPlayers<1)
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} totalPlayers {totalPlayers} is less than 1");
                return false;
            }
            if( peekPlayers> totalPlayers)
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} peekPlayers {peekPlayers} is greater than totalPlayers {totalPlayers}");
                return false;
            }
            if(firstTimers<0) 
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} firstTimers {firstTimers} is less than 0");
                return false;

            }

            if (eventID.Equals(""))
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} eventID is missing");
                return false;
            }
            
            if(worldID.Equals(""))
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} worldID is missing");
                return false;
            }

            if (firstTimeNames.Count!= firstTimers)
            {
                log.Error($"Sanity check failed for EventReport {GetHashKey()} the length of the firstTimeNames array is {firstTimeNames.Count} which is not equal to the firstTimers value of {firstTimers}");
                return false;
            }

            foreach (var item in attendaceLog)
            {
                if (item.playerCount < 0)
                {
                    log.Error($"Sanity check failed for attendaceLog {item.GetHashKey()} in EventReport {GetHashKey()} item.playerCount {item.playerCount} is less than 0");
                    return false;
                }
                if (totalPlayers < item.playerCount)
                {
                    log.Error($"Sanity check failed for attendaceLog {item.GetHashKey()} in EventReport {GetHashKey()} item.playerCount {item.playerCount} is less than totalPlayers {totalPlayers}");
                    return false;
                }
                if (peekPlayers< item.playerCount)
                {
                    log.Error($"Sanity check failed for attendaceLog {item.GetHashKey()} in EventReport {GetHashKey()} peekPlayers {peekPlayers} is less than item.playerCount {item.playerCount}");
                    return false;
                }
                if (item.time< firstlog)
                {
                    log.Error($"Sanity check failed for attendaceLog {item.GetHashKey()} in EventReport {GetHashKey()} item.time {item.time} is less than firstlog {firstlog}");
                    return false;
                } 
                if (item.time>lastlog)
                {
                    log.Error($"Sanity check failed for attendaceLog {item.GetHashKey()} in EventReport {GetHashKey()} item.time {item.time} is greater than lastlog {lastlog}");
                    return false;
                }
            }

            return true;

        }
    }
}
