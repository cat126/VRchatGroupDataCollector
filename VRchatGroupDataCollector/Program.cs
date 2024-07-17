// See https://aka.ms/new-console-template for more information
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using VRchatGroupDataCollector;
using VRchatGroupDataCollector.Models;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
//2024.06.21 15:55:19 Log        -  [Behaviour] Joining wrld_dd036610-a246-4f52-bf01-9d7cea3405d7:80864~group(grp_8250f46e-7b91-4244-95ec-ebdeb87d9be8)~groupAccessType(plus)~region(eu)
Console.WriteLine("Hello, World!");
//"C:\\Users\\%USERNAME%\\AppData\\LocalLow\\VRChat\\vrchat";
//%APPDATA%\..\LocalLow\VRChat\VRChat

string vrchatLogDir = "C:\\Users\\cat126\\AppData\\LocalLow\\VRChat\\vrchat";
vrchatLogDir = System.IO.Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + $"{Path.DirectorySeparatorChar}LocalLow{Path.DirectorySeparatorChar}VRChat{Path.DirectorySeparatorChar}vrchat";
var files = Directory.GetFiles(vrchatLogDir);

IEnumerable<string> logfiles =
    from file in files
    where Regex.IsMatch(file, "output_log_.+\\.txt")
    orderby file
    select file;


LinkedList<vrChatLogItem> vrChatLogItems = new LinkedList<vrChatLogItem>();

foreach (var logFile in logfiles)
{
    Console.WriteLine(logFile);
    using StreamReader reader = new(logFile);
    //var text =reader.ReadToEnd();
    string currentLogText = "";
    int playerCount = 0;
    vrChatinstance currentInstance = new vrChatinstance();
    while (!reader.EndOfStream)
    {

        var text = reader.ReadLine();

        currentLogText += text;
        if (text.Equals(""))
        {
            if (!currentLogText.Equals(""))
            {
                //Console.WriteLine(currentLogText);
                vrChatLogItem currentLogitem = new vrChatLogItem(currentLogText);
                if (currentLogitem.item.Contains("Joining wrld_"))
                {
                    currentInstance = currentInstance.buildfromlogString(currentLogitem.RawText);
                    playerCount = 0;
                }
                string joiningorCreatingRoom = "Joining or Creating Room: ";
                if (currentLogitem.item.Contains(joiningorCreatingRoom))
                {

                    string roomName = currentLogitem.item.Substring(joiningorCreatingRoom.Length);
                    currentInstance.roomName = roomName;
                    playerCount = 0;
                }
                if (currentLogitem.item.Contains("OnLeftRoom"))
                {
                    currentInstance = new vrChatinstance();
                    playerCount = 0;
                }
                if (currentLogitem.item.Contains("OnPlayerJoined"))
                {
                    playerCount += 1;
                }
                if (currentLogitem.item.Contains("OnPlayerLeft "))
                {
                    playerCount -= 1;
                }
                currentLogitem.instance = currentInstance;
                currentLogitem.playerCount = playerCount;
                vrChatLogItems.AddLast(currentLogitem);

            }
            currentLogText = "";
        }

    }

}

LinkedList<string> itemsOfType1 = new LinkedList<string>();
LinkedList<string> itemsOfType2 = new LinkedList<string>();
foreach (var item in vrChatLogItems)
{
    if (!itemsOfType1.Contains(item.type1))
    {
        itemsOfType1.AddLast(item.type1);
    }
    if (!itemsOfType2.Contains(item.type2))
    {
        itemsOfType2.AddLast(item.type2);
    }
}

foreach (var item in itemsOfType1)
{
    Console.WriteLine(item);
}

foreach (var item in itemsOfType2)
{
    Console.WriteLine(item);
}

LinkedList<vrChatLogItem> vrChatLogItemsToKeep = new LinkedList<vrChatLogItem>();

bool shouldkeep(vrChatLogItem item)
{
    string[] keepitems = { "[Behaviour] Joining wrld_", "[Behaviour] Joining or Creating Room", " [Behaviour] OnPlayerJoined ", "[Behaviour] OnLeftRoom", " is local", "[Behaviour] OnPlayerLeft ", "Instance closed" };
    string Groupid = "grp_8250f46e-7b91-4244-95ec-ebdeb87d9be8";
    if (!item.instance.CreatorID.Contains(Groupid))
    {
        return false;
    }

    foreach (var pattern in keepitems)
    {
        if (item.RawText.Contains(pattern))
        {
            return true;
        }

    }
    return false;
}
foreach (var item in vrChatLogItems)
{
    if (shouldkeep(item))
    {
        vrChatLogItemsToKeep.AddLast(item);
    }
}

;
using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "WriteLines.csv")))
{
    foreach (var item in vrChatLogItemsToKeep)
    {
        //Console.WriteLine(item.RawText);
        var itemIngoogleSheetFormat = item.googleSheetFormat();
        if (!itemIngoogleSheetFormat.Equals(string.Empty))
        {
            Console.WriteLine(itemIngoogleSheetFormat);
            outputFile.WriteLine(itemIngoogleSheetFormat);
        }

    }
}
using (StreamWriter outputFile = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "cleanedlog.txt")))
{
    foreach (var item in vrChatLogItemsToKeep)
    {
        //Console.WriteLine(item.RawText);


        outputFile.WriteLine(item.RawText);


    }
}







//DatabaseStore(vrChatLogItemsToKeep);

Console.WriteLine("Done");

//await directToDynmomdb(vrChatLogItemsToKeep);

static async Task directToDynmomdb(LinkedList<vrChatLogItem> vrChatLogItemsToKeep)
{
    AmazonDynamoDBClient dynamoDB = new AmazonDynamoDBClient();
    var dbLogTable = Table.LoadTable(dynamoDB, "FEClogs");
    foreach (var item in vrChatLogItemsToKeep)
    {
        if (item.item.Contains("OnPlayerJoined") | item.item.Contains("OnPlayerLeft"))
        {
            Console.WriteLine(" ");
            var newitem = item.ToPlayeritem();
            var test = Newtonsoft.Json.JsonConvert.SerializeObject(newitem);
            Console.WriteLine(test);
            //{"time":"2024-07-08T21:03:43Z","type":"OnPlayerLeft","PlayerName":"BriarTheShark","playerCount":7,"worldID":"wrld_796a73f6-6fad-4c08-a842-6b518589d809","instanceID":40977,"instanceType":"group","region":"eu","roomName":" Corgi Beans Cafe!","EventID":"-1961193702","itemID":"1403444800","CreatorID":"grp_8250f46e-7b91-4244-95ec-ebdeb87d9be8","GroupAccessType":"~groupAccessType(plus)"}
            var temp = new Amazon.DynamoDBv2.DocumentModel.Document();
            temp["time"] = newitem.time;
            temp["type"] = newitem.type;
            temp["PlayerName"] = newitem.PlayerName;
            temp["playerCount"] = newitem.playerCount;
            temp["itemID"] = newitem.itemID;
            temp["worldID"] = newitem.worldID;
            temp["instanceID"] = newitem.instanceID;
            temp["instanceType"] = newitem.instanceType;
            temp["CreatorID"] = newitem.CreatorID;
            temp["GroupAccessType"] = newitem.GroupAccessType;
            temp["region"] = newitem.region;
            temp["roomName"] = newitem.roomName;
            temp["EventID"] = newitem.EventID;
            await dbLogTable.PutItemAsync(temp);


        }

    }
}
 static AuthFlowResponse AuthenticateWithSrpAsync()
{
    var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), FallbackRegionFactory.GetRegionEndpoint());
    string clientID = "11kvh88sqq01rrpndjs4ah1tk9";
    var userPool = new CognitoUserPool("us-east-2_cMqHHqcY8", clientID, provider);
    var user = new CognitoUser("magmaoverflow", clientID, userPool, provider);

    var password = "%7+2k{7sp<m`HX~eVc\\#62wymv*X3:XcNJXy$l|J'SNBM07@xH!3LKIKDJ<i$*fSf>k9XOEMM(lY.hMo(NLF3BGP]Z~l`9:<iij:";

    var test = user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
    {
        Password = password
    }).ConfigureAwait(false);

    AuthFlowResponse authResponse = test.GetAwaiter().GetResult();
    return authResponse;
}
string token = "";
try

{
    var logon = AuthenticateWithSrpAsync();

    Console.WriteLine("Hello, World! " + logon.AuthenticationResult.TokenType + "    " + logon.AuthenticationResult.AccessToken);
    HttpClient client = new();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {logon.AuthenticationResult.AccessToken}");
    token = logon.AuthenticationResult.AccessToken;

}
catch (Exception)
{

    Console.WriteLine("logon failed");
}

var graphQLClient = new GraphQLHttpClient("https://mg2shshe3zaclks56t5bxbxbpe.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
foreach (var item in vrChatLogItemsToKeep) 
{
    if (item.item.Contains("OnPlayerJoined") | item.item.Contains("OnPlayerLeft"))
    {
        string query = item.ToPlayeritem().toGraphQLcreationString();
        Console.WriteLine(query);
        var dataupdate = new GraphQLRequest(query);
        var sentrquest = graphQLClient.SendMutationAsync<vrChatLogitemJOSN>(dataupdate);
        sentrquest.Wait();
        var response = sentrquest.Result;
        if (response.Errors != null)
        {
            foreach (var error in response.Errors)
            {
                Console.WriteLine("");
                Console.WriteLine(error.Message);
                Console.WriteLine("");
                //throw new Exception(error.Message);
            }
            //break;
        }

        Console.WriteLine(response.Data);
    }
}
    





