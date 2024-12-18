
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using VRchatLogDataModel;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

namespace DataProcessorPrototype
{
    internal class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            
            log.Info("Program Starting");
            var awaiter =CreateJsonFileFromDatabase();
            awaiter.Wait();
            log.Info("Program Finished");
             

            var awaiter2 = GetAllReportsFromDynanoDB();
            awaiter2.Wait();
            Console.WriteLine("Finished");
           
            
            
            var awaiter3 =  rawDataDownload();
            awaiter3.Wait();
            Console.WriteLine("Done");
            

            var awaiter4 = StaffReport();
            awaiter4.Wait();
        }
        private static async Task StaffReport() 
        {

            string[] staff = { };

            Console.WriteLine("staff count "+staff.Length);
            using StreamReader reader = new(Path.Combine(AppContext.BaseDirectory, "DataProcessorPrototype.PlayerReport[].json"));
            string text = reader.ReadToEnd();
            PlayerReport[] playerReports= JsonConvert.DeserializeObject<PlayerReport[]>(text);

            LinkedList<PlayerReport> staffreports = new LinkedList<PlayerReport>();
            LinkedList<string> staffPresent = new LinkedList<string>();

            foreach (var player in playerReports)
            {
                foreach (var staffName in staff)
                {
                    if (player.name.Equals(staffName))
                    {
                        staffreports.AddLast(player);
                    }
                }
            }

            var staffreportsSorted = staffreports.OrderBy(p=>p.lastseen*-1).ToArray();
            foreach (var staffreport in staffreportsSorted)
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(staffreport.lastseen).ToLocalTime();
                //Console.WriteLine($"{staffreport.name} has been to {staffreport.eventsjoined} recorded events the last recored event was on {dateTime}");
                Console.WriteLine($"{staffreport.name} has been to {staffreport.eventsjoined} recorded events the last recored event was on <t:{staffreport.lastseen}:D>");
                staffPresent.AddLast(staffreport.name);
            }

            foreach (var staffName in staff)
            {
                if (!staffPresent.Contains(staffName))
                {
                    Console.WriteLine($"{staffName} has not been to any recorded events");
                }
            }

        }
        private static async Task CreateJsonFileFromDatabase()
        {
            var logon = AuthenticateWithSrp(Secret.GetUSername(), Secret.GetPassword());
            string token = logon.AuthenticationResult.AccessToken;
            var graphQLClient = new GraphQLHttpClient("https://hlfxzmed2jh4zgm37n4kdy5kna.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            Console.WriteLine(token);
            Console.WriteLine(Path.Combine(AppContext.BaseDirectory, "worldReports.json"));
            AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();
            var dbLogTable = Table.LoadTable(dynamoDB, "PlayerEvent-yqjhtslhmngtjgdb3t5ifhsbza-master");
            Dictionary<string, WorldReport> worldReports = new Dictionary<string, WorldReport>();
            Dictionary<string, PlayerReport> playerReports = new Dictionary<string, PlayerReport>();
            Dictionary<string, EventReport> eventReports = new Dictionary<string, EventReport>();
            Dictionary<string, AttendaceLog> attendaceLogs = new Dictionary<string, AttendaceLog>();




            int onItem = 0;

            List<AttributeValue> scanparms = new List<AttributeValue>();
            //1724619323 
            scanparms.Add(new AttributeValue { N= "1730073600" });
            ScanFilter filter = new ScanFilter();
            filter.AddCondition("time", ScanOperator.GreaterThan, scanparms);
            var scan = dbLogTable.Scan(filter);
            var pre = scan.GetNextSetAsync();
            pre.Wait();
            var items = pre.Result;
            bool moreTtems = true;
            
           

            Console.WriteLine(items.Count);
            while (moreTtems)
            {
                foreach (var doucmentItem in items)
                {
                    Console.WriteLine(onItem);
                    onItem++;
                    vrChatLogitemJOSN item = JsonConvert.DeserializeObject<vrChatLogitemJOSN>(doucmentItem.ToJson());

                    //temp fix
                    String pattern = "\\s\\((usr[_\\S]+)\\)";
                 
                    item.PlayerName=Regex.Replace(item.PlayerName, pattern, String.Empty);
                    
                    //temp fix
                    WorldReport worldReport = await CreateOrGetAsync<WorldReport>(worldReports, item, item.worldID, graphQLClient, dynamoDB);
                    PlayerReport playerReport = await CreateOrGetAsync<PlayerReport>(playerReports, item, item.PlayerName, graphQLClient, dynamoDB);
                    EventReport eventReport = await CreateOrGetAsync<EventReport>(eventReports, item, item.EventID, graphQLClient, dynamoDB);
                    AttendaceLog attendaceLog= await CreateOrGetAsync<AttendaceLog>(attendaceLogs, item, item.itemID, graphQLClient, dynamoDB);

                    eventReport.peekPlayers = Math.Max(eventReport.peekPlayers, item.playerCount);
                    worldReport.firstLastUpdate(item.time);
                    playerReport.firstLastUpdate(item.time);
                    eventReport.addAttendaceLog(attendaceLog);

                }
                if (!scan.IsDone)
                {
                     pre = scan.GetNextSetAsync();
                    pre.Wait();
                     items = pre.Result;
                }
                else 
                {
                    moreTtems = false;
                }
            }
            foreach (var preReport in eventReports)
            {
                var report = preReport.Value;
                LinkedList<string> names = new LinkedList<string>();
                foreach (var logitem in report.GetAttendaceLog())
                {
                    if (!names.Contains(logitem.name))
                    {
                        names.AddLast(logitem.name);
                        report.totalPlayers++;
                    }
                }
                foreach (var name in names)
                {
                    //TODO remove try cacth
                    try
                    {
                        var aPlayerReport = playerReports[name];
                        if (aPlayerReport.fristseen >= report.firstlog &
                            aPlayerReport.fristseen <= report.lastlog &
                            !report.firstTimeNames.Contains(aPlayerReport.name)
                            )
                        {
                            report.firstTimers++;
                            report.firstTimeNames.AddLast(name);
                            playerReports[name].firstEventID = report.eventID;
                        }
                    }
                    catch (Exception)
                    {

                       
                    }
                }
            }

            foreach (var report in eventReports) 
            {
                report.Value.finalize();
                if (!report.Value.GetIsInDatabase()) 
                {
                    worldReports[report.Value.worldID].timesused++;
                }
                LinkedList<string> names = new LinkedList<string>();
                foreach (var item in report.Value.GetAttendaceLog())
                {
                    if (!names.Contains(item.name) & !item.GetIsInDatabase()) 
                    {
                        
                        names.AddLast(item.name);
                    }
                }
                foreach (var name in names) 
                {
                    playerReports[name].eventsjoined++;
                }

                
            }


            var aEventReport = eventReports.First().Value;
            Console.WriteLine(aEventReport.GetAttendaceLog().Count);
            GlobalStatistics globalStatistics = new GlobalStatistics(DictionaryToArray<PlayerReport>(playerReports));
            globalStatistics.mapCount = worldReports.Count;
            globalStatistics.eventCount = eventReports.Count;
            globalStatistics.lastProssesTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            Console.WriteLine(globalStatistics);


            using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "worldReports.json")))
            {

                outputFile.WriteLine(JsonConvert.SerializeObject(worldReports));
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "playerReports.json")))
            {

                outputFile.WriteLine(JsonConvert.SerializeObject(playerReports));
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "eventReports.json")))
            {

                outputFile.WriteLine(JsonConvert.SerializeObject(eventReports));
            }

            //DictionaryToArray

            LinkedList<Task> tasks = new LinkedList<Task>();

            SendToDatabase<WorldReport>(graphQLClient, worldReports, tasks);
            SendToDatabase<PlayerReport>(graphQLClient, playerReports, tasks);
            SendToDatabase<EventReport>(graphQLClient, eventReports, tasks);
            LinkedList<AttendaceLog> keepedAttendaceLogs= new LinkedList<AttendaceLog>();
            foreach (var eventReport in eventReports)
            {
                foreach (var item in eventReport.Value.GetAttendaceLog())
                {
                    keepedAttendaceLogs.AddLast(item);
                }
            }
            foreach (var attendaceLog in attendaceLogs)
            {
                if (!keepedAttendaceLogs.Contains(attendaceLog.Value))
                {
                    attendaceLogs.Remove(attendaceLog.Key);
                }
            }

            SendToDatabase<AttendaceLog>(graphQLClient, attendaceLogs, tasks);

            Console.WriteLine("sending data");

            while (tasks.Count > 0) 
            {

                Console.WriteLine($"sending data {tasks.Count} left");
                var task = tasks.First();
                if (task.IsCompleted) 
                {
                    tasks.Remove(task);
                }
            }

        }
        private static async Task rawDataDownload()
        {
            var logon = AuthenticateWithSrp(Secret.GetUSername(), Secret.GetPassword());
            string token = logon.AuthenticationResult.AccessToken;
            var graphQLClient = new GraphQLHttpClient("https://hlfxzmed2jh4zgm37n4kdy5kna.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            Console.WriteLine(token);
            Console.WriteLine(Path.Combine(AppContext.BaseDirectory, "worldReports.json"));
            AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();
            var dbLogTable = Table.LoadTable(dynamoDB, "PlayerEvent-yqjhtslhmngtjgdb3t5ifhsbza-master");
            int onItem = 0;
            List<AttributeValue> scanparms = new List<AttributeValue>();
            scanparms.Add(new AttributeValue { N = "0" });
            ScanFilter filter = new ScanFilter();
            filter.AddCondition("time", ScanOperator.GreaterThan, scanparms);
            var scan = dbLogTable.Scan(filter);
            var pre = scan.GetNextSetAsync();
            pre.Wait();
            var items = pre.Result;
            bool moreTtems = true;
            Console.WriteLine(items.Count);
            LinkedList<vrChatLogitemJOSN> allrawLogItems = new();
            while (moreTtems)
            {
                foreach (var doucmentItem in items)
                {
                    Console.WriteLine(onItem);
                    onItem++;
                    vrChatLogitemJOSN item = JsonConvert.DeserializeObject<vrChatLogitemJOSN>(doucmentItem.ToJson());
                    allrawLogItems.AddLast(item);

                }
                if (!scan.IsDone)
                {
                    pre = scan.GetNextSetAsync();
                    pre.Wait();
                    items = pre.Result;
                }
                else
                {
                    moreTtems = false;
                }
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "rawData.json")))
            {

                outputFile.WriteLine(JsonConvert.SerializeObject(allrawLogItems));
            }
        }
        private static async Task GetAllReportsFromDynanoDB() 
        {
            AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();
            var worldReportsWaiter = listAll<WorldReport>(dynamoDB);
            var playerReportsWaiter = listAll<PlayerReport> (dynamoDB);
            var eventReportsWaiter = listAll<EventReport> (dynamoDB);
            var attendaceLogsWaiter = listAll<AttendaceLog> (dynamoDB);

            WorldReport[] worldReports = await worldReportsWaiter;
            PlayerReport[] playerReports = await playerReportsWaiter;
            EventReport[] eventReports = await eventReportsWaiter;
            AttendaceLog[] attendaceLogs = await attendaceLogsWaiter;

            object[] Alldata = { worldReports, playerReports , eventReports, attendaceLogs };

            foreach (var item in Alldata)
            {
                item.GetType().ToString();
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, item.GetType().ToString()+".json")))
                {

                    outputFile.WriteLine(JsonConvert.SerializeObject(item));
                }
            }
                




        }

        private static void SendToDatabase<type>(GraphQLHttpClient graphQLClient, Dictionary<string, type> Reports, LinkedList<Task> tasks) where type : GraphQLQuarriable, ISanityCheck
        {
            var reportsArray = DictionaryToArray<type>(Reports);
            foreach (var report in reportsArray)
            {
                if (!report.SanityCheck(log))
                {
                    continue;
                }

                var quarry = "";
                if (report.GetIsInDatabase())
                {
                    quarry = report.GetUpdateQuarry();
                }
                else
                {
                    quarry = report.GetCreateQuarry();
                }
                tasks.AddLast(SendQuery<type>(graphQLClient, quarry));
            }
        }

        private static async Task<type> CreateOrGetAsync<type>(Dictionary<string, type> reports, vrChatLogitemJOSN item, string itemkey, GraphQLHttpClient graphQLClient, AmazonDynamoDBClient dynamoDB) where type : GraphQLQuarriable, IFromvrChatLogitemJOSN, new()
        {
          
            type report= new ();
            bool ReportCheck = reports.TryGetValue(itemkey, out report);
            if (!ReportCheck)
            {
                
                report = new();
                report.fromvrChatLogitemJOSN(item);


                var tableName = report.GetTableName();
                var theDBTable = Table.LoadTable(dynamoDB, tableName);
                var itemFromDbWaiter=  theDBTable.GetItemAsync(itemkey);
                var itemFromDb = await itemFromDbWaiter;

             
                if (itemFromDb != null) 
                {
                  
                    
                    var tempreport = JsonConvert.DeserializeObject<type>(itemFromDb.ToJson());
                    if (tempreport != null) 
                    {
                        if (!tempreport.IsEmpty()) 
                        {
                            report = tempreport;
                            report.SetIsInDatabase(true);
                        }
                    }
                }

                reports.Add(itemkey, report);

            }
            return report;
        }
        private static async Task<type[]> listAll<type>( AmazonDynamoDBClient dynamoDB) where type : GraphQLQuarriable, IFromvrChatLogitemJOSN, new()
        {

            LinkedList<type> data = new();
            type typeData = new type();
            var tableName = typeData.GetTableName();
            var theDBTable = Table.LoadTable(dynamoDB, tableName);
            List<AttributeValue> scanparms = new List<AttributeValue>();
            scanparms.Add(new AttributeValue { N = "0" });
            ScanFilter filter = new ScanFilter();
            filter.AddCondition("_lastChangedAt", ScanOperator.GreaterThan, scanparms);
            
             var dbScaner = theDBTable.Scan(filter);
            while (!dbScaner.IsDone) { 
            var itemsFromDb = await dbScaner.GetNextSetAsync();
            Console.WriteLine($"got {itemsFromDb.Count} items from {tableName}");
            foreach (var item in itemsFromDb) 
            {
                var tempReport = JsonConvert.DeserializeObject<type>(item.ToJson());
                data.AddLast(tempReport);
            }
            }
            return data.ToArray();
        }

        private static async Task<GraphQLResponse<type>> SendQuery<type>(GraphQLHttpClient graphQLClient, string quarry) where type : GraphQLQuarriable
        {
            var dataupdate = new GraphQLRequest(quarry);
            var sentRequest = graphQLClient.SendQueryAsync<type>(dataupdate);
            var temp=await sentRequest;
            if (temp.Errors != null) 
            {
                string errorMessage = "";
                foreach (var error in temp.Errors)
                {
                    errorMessage+=error.Message+"\n";
                }
                log.Error($"Query failed:\n{quarry}\n{errorMessage}");
            }
            
            return temp;
        }

        public static type[] DictionaryToArray<type>(Dictionary<string, type> Dictionary)
        {
            var keyPairArray = Dictionary.ToArray();
            type[] arrary= new type[keyPairArray.Length];
            for (int i = 0; i < keyPairArray.Length; i++) 
            {
                arrary[i] = keyPairArray[i].Value;
            }
            return arrary;
        }
        public static AuthFlowResponse AuthenticateWithSrp(string username, string password)
        {

            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.USEast2);
            string clientID = "6fvkibcs8r4devuic7k12g70p9";
            var userPool = new CognitoUserPool("us-east-2_cvAEdgOs0", clientID, provider);
            var user = new CognitoUser(username, clientID, userPool, provider);



            var test = user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
            {
                Password = password
            }).ConfigureAwait(false);

            AuthFlowResponse authResponse = test.GetAwaiter().GetResult();
            return authResponse;
        }

    }
}
