using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Security.Cryptography.X509Certificates;
using TestConsole;


internal class Program
{
    public static   AuthFlowResponse AuthenticateWithSrp(string username, string password)
    {
        var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), FallbackRegionFactory.GetRegionEndpoint());
        string clientID = "11kvh88sqq01rrpndjs4ah1tk9";
        var userPool = new CognitoUserPool("us-east-2_cMqHHqcY8", clientID, provider);
        var user = new CognitoUser(username, clientID, userPool, provider);

        

        var test =  user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest()
        {
            Password = password
        }).ConfigureAwait(false);

        AuthFlowResponse authResponse =  test.GetAwaiter().GetResult();
        return authResponse;
    }
    private static   void Main(string[] args)
    {
        string token = "";
        try

        {
            var logon1 = AuthenticateWithSrp("testdelete", "P@ssw0rd");
            Console.WriteLine(logon1.ChallengeName);

            var logon2 = AuthenticateWithSrp("magmaoverflow", "p");
            Console.WriteLine(logon2.ChallengeName);

            var logon3 = AuthenticateWithSrp("testdelete2", "P@ssw0rd");
            Console.WriteLine(logon3.ChallengeName);

            /*
            Console.WriteLine("Hello, World! " + logon.AuthenticationResult.TokenType+"    "+logon.AuthenticationResult.AccessToken);
             HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization",$"Bearer {logon.AuthenticationResult.AccessToken}");
            token = logon.AuthenticationResult.AccessToken;
            
            var test=  client.GetAsync("https://csthoypvyc.execute-api.us-east-2.amazonaws.com/dev");
            test.Wait();
            var test2=test.Result;
            Console.WriteLine(test2);
            */
            //Newtonsoft.Json.JsonConvert


        }
        catch (Exception e)
        {

            Console.WriteLine("logon failed");
            
        }
        string query = "mutation MyMutation {\r\n  createPlayerEvent(input: {CreatorID: \"\", EventID: \"\", GroupAccessType: \"\", PlayerName: \"\", _version: 10, instanceID: 10, id: \"\", itemID: \"\", instanceType: \"\", playerCount: \"\", roomName: \"\", worldID: \"\", type: \"\", region: \"\"}) {\r\n    id\r\n  }\r\n}";
        var graphQLClient = new GraphQLHttpClient("https://nsjigvkmhzbixj4c4xxjua4zkq.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var dataupdate = new GraphQLRequest(query);
        var sentrquest = graphQLClient.SendMutationAsync<vrChatLogitemJOSN>(dataupdate);
        sentrquest.Wait();
        
        var response = sentrquest.Result;
        foreach (var error in response.Errors)
        {
            Console.WriteLine(error.Message);
        }
        Console.WriteLine(response.Data);



    }

}
