
namespace DataProcessorPrototype
{
    public class GlobalStatistics: GraphQLQuarriable
    {
        public int id = 1;
        public int totalPlayerCount = 0;
        public int eventCount = 0;
        public int mapCount = 0;
        public int oneTimePlayers = 0;
        public int q1EventAttendance = 0;
        public int MeanEventAttendance = 0;
        public int q3EventAttendance = 0;
        public int averageEventAttendance = 0;
        public long lastProssesTime = 0;
        public GlobalStatistics() { }

        public GlobalStatistics(PlayerReport[] playerReports)
        {
            int[] eventsJoinedCount = new int[playerReports.Length];
            totalPlayerCount = playerReports.Count();
            int sumOfEventsJoined=0;
            for (int i = 0; i < eventsJoinedCount.Length; i++) 
            {
                eventsJoinedCount[i] = playerReports[i].eventsjoined;
                sumOfEventsJoined += playerReports[i].eventsjoined;
                if (eventsJoinedCount[i] < 2) 
                {
                    oneTimePlayers++;
                }
            }
            Array.Sort(eventsJoinedCount);
            averageEventAttendance = sumOfEventsJoined / eventsJoinedCount.Length;
            MeanEventAttendance = FindMean(eventsJoinedCount);
            int halfwaypoint = eventsJoinedCount.Length / 2;
            if (eventsJoinedCount.Length % 2 == 0)
            {
                q1EventAttendance = FindMean(eventsJoinedCount.Take(halfwaypoint).ToArray());
                q3EventAttendance = FindMean(eventsJoinedCount.Skip(halfwaypoint).Take(halfwaypoint).ToArray());
            }
            else 
            {
                q1EventAttendance = FindMean(eventsJoinedCount.Take(halfwaypoint-1).ToArray());
                q3EventAttendance = FindMean(eventsJoinedCount.Skip(halfwaypoint).Take(halfwaypoint-1).ToArray());
            }

            foreach (var number in eventsJoinedCount)
            {
                Console.Write(", "+number);
            }

        }
        public static int FindMean(int[] array) 
        {
            int halfwaypoint = (array.Length / 2)+1;
            if (array.Length % 2 == 0) 
            {
                return (array[halfwaypoint-1] + array[halfwaypoint]) / 2;
            }
            else
            {
                return array[halfwaypoint];
            }
        }
        public override string ToString() 
        {
            return $"Total players {totalPlayerCount}\n" +
                    $"Total Events {eventCount}\n" +
                    $"Total maps {mapCount}\n" +
                    $"{oneTimePlayers} have only joined once\n" +
                    $"q1:{q1EventAttendance} mean:{MeanEventAttendance} q3:{q3EventAttendance} average:{averageEventAttendance}\n" ;
        }
        public override string GetGraphQLSuffix()
        {
            return "GlobalStatistics";
        }
        public override string ListGraphQLProperties()
        {
            return base.ListGraphQLProperties();
        }
        public override string GetGetField()
        {
            return $"id: {id}";
        }
        public override bool IsEmpty()
        {
            return totalPlayerCount==0;
        }

        public override string GetHashKey()
        {
            return id + ""; ;
        }

        public override string GetTableName()
        {
            throw new NotImplementedException();
        }
    }
}
