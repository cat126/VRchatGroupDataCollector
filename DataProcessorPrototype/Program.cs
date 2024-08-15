using Amazon.AppSync;
using Amazon.AppSync.Model;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRchatLogDataModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataProcessorPrototype
{
    internal class Program
    {
        static  void Main(string[] args)
        {


            var awaiter =CreateJsonFileFromDatabase();
            awaiter.Wait();
        }

        private static async Task CreateJsonFileFromDatabase()
        {
            
            var logon = AuthenticateWithSrp(Secret.GetUSername(), Secret.GetPassword());
            string token = logon.AuthenticationResult.AccessToken;
            var graphQLClient = new GraphQLHttpClient("https://hlfxzmed2jh4zgm37n4kdy5kna.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            Console.WriteLine(token);
            Console.WriteLine("Hello, World!");
            Console.WriteLine(Path.Combine(AppContext.BaseDirectory, "worldReports.json"));
            AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();
            var dbLogTable = Table.LoadTable(dynamoDB, "PlayerEvent-yqjhtslhmngtjgdb3t5ifhsbza-master");
            Dictionary<string, WorldReport> worldReports = new Dictionary<string, WorldReport>();
            Dictionary<string, PlayerReport> playerReports = new Dictionary<string, PlayerReport>();
            Dictionary<string, EventReport> eventReports = new Dictionary<string, EventReport>();
            Dictionary<string, AttendaceLog> attendaceLogs = new Dictionary<string, AttendaceLog>();




            int onItem = 0;

            List<AttributeValue> scanparms = new List<AttributeValue>();
            scanparms.Add(new AttributeValue { N= "1722218500" });
            ScanFilter filter = new ScanFilter();
            //1722218500
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
                    //Console.WriteLine(onItem);
                    onItem++;
                    vrChatLogitemJOSN item = JsonConvert.DeserializeObject<vrChatLogitemJOSN>(doucmentItem.ToJson());
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
                    var aPlayerReport = playerReports[name];
                    if (aPlayerReport.fristseen >= report.firstlog &
                        aPlayerReport.fristseen <= report.lastlog)
                    {
                        report.firstTimers++;
                        report.firstTimeNames.AddLast(name);
                        playerReports[name].firstEventID = report.eventID;
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

        private static void SendToDatabase<type>(GraphQLHttpClient graphQLClient, Dictionary<string, type> Reports, LinkedList<Task> tasks) where type : GraphQLQuarriable
        {
            var reportsArray = DictionaryToArray<type>(Reports);
            foreach (var report in reportsArray)
            {
                var quarry = "";
                if (report.GetIsInDatabase())
                {
                    report._version++;
                    quarry = report.GetUpdateQuarry();
                }
                else
                {
                    quarry = report.GetCreateQuarry();
                }
                tasks.AddLast(SendQuery<WorldReport>(graphQLClient, quarry));
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
                //AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();

                var tableName = report.GetTableName();
                var theDBTable = Table.LoadTable(dynamoDB, tableName);
                var itemFromDbWaiter=  theDBTable.GetItemAsync(itemkey);
                var itemFromDb = await itemFromDbWaiter;

                //var results = dbitemTask.Result;
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

                /*
                var quarry = report.GetGetQuarry();
                var resultWaiter = SendQuery<type>(graphQLClient, quarry);
                resultWaiter.Wait();
                
                var result=resultWaiter.GetAwaiter().GetResult();
                if (result.Errors == null) 
                {
                    
                    var tempreport = result.Data;
                    tempreport.SetIsInDatabase(true);
                    if (!tempreport.IsEmpty()) 
                    {
                        report = tempreport;
                    }
                   
                }
                */
                reports.Add(itemkey, report);

            }
            return report;
        }

        private static async Task<GraphQLResponse<type>> SendQuery<type>(GraphQLHttpClient graphQLClient, string quarry) where type : GraphQLQuarriable, IFromvrChatLogitemJOSN, new()
        {
            var dataupdate = new GraphQLRequest(quarry);
            var sentRequest = graphQLClient.SendQueryAsync<type>(dataupdate);
            var temp=await sentRequest;
            if (temp.Errors != null) 
            {
                Console.WriteLine("Query failed:");
                Console.WriteLine(quarry);
                foreach (var error in temp.Errors)
                {
                    Console.WriteLine(error.Message);
                }
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
