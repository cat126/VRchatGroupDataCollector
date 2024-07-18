using VRchatLogDataModel;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using Amazon.Runtime.Internal;
using System.IO;
namespace VRchatDataCollector
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            //
            Console.WriteLine("testing one two three");
            var protector = MemoryProtection.Setup();
            string mydata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Path.DirectorySeparatorChar}VRchatDataCollector/data.txt";
            try
            {
                using StreamReader reader = new(mydata);
                string username = reader.ReadLine();
                username = protector.RemoveProtection(username);
                this.UserNameBox.Text = username;
                string password = reader.ReadLine();
                password = protector.RemoveProtection(password);
                this.PasswordBox.Text = password; ;

            }
            catch (Exception)
            {


            }


            string test = protector.Protect("Hello");
            Console.WriteLine(test);
            Console.WriteLine(protector.RemoveProtection(test));
            //

        }
        private void button1_Click(object sender, System.EventArgs e)
        {

            var logFiles = VRchatLogDataModel.LogProcessor.ListVrchatLogFiles();
            int fileCount = 1;
            foreach (var logFile in logFiles)
            {
                using StreamReader reader = new(logFile);
                var logitems = VRchatLogDataModel.LogProcessor.processVRchatLogFile(reader);
                logitems = VRchatLogDataModel.LogProcessor.FilterToOnlyPlayerJoinOrLeave(logitems);
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"{Path.DirectorySeparatorChar}cleanedlog{fileCount}.txt";
                using (StreamWriter outputFile = new StreamWriter(path))
                {
                    foreach (var item in logitems)
                    {



                        outputFile.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));


                    }
                }
                fileCount++;

            }

            MessageBox.Show("The preview files have been saved to your desktop as cleanedlog_.txt");
        }
        private void button2_Click(object sender, System.EventArgs e)
        {
            DataSend();
        }

        private async void DataSend()
        {
            string token = "";
            bool logedIn = false;
            try
            {
                var logon = AuthenticateWithSrp(this.UserNameBox.Text.Trim(), this.PasswordBox.Text.Trim());
                token = logon.AuthenticationResult.AccessToken;
                logedIn = true;
                //
                string mydata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Path.DirectorySeparatorChar}VRchatDataCollector{Path.DirectorySeparatorChar}";
                System.IO.Directory.CreateDirectory(mydata);
                mydata = mydata + "data.txt";
                var protector = MemoryProtection.Setup();

                using (StreamWriter outputFile = new StreamWriter(mydata))
                {
                    string username = this.UserNameBox.Text.Trim();
                    username = protector.Protect(username);
                    outputFile.WriteLine(username);
                    string password = this.PasswordBox.Text.Trim();
                    password = protector.Protect(password);
                    outputFile.WriteLine(password);
                }
                //


            }
            catch (Exception error)
            {
                MessageBox.Show("Error: " + error.Message);

            }
            if (logedIn)
            {
                //
                LinkedList<System.Threading.Tasks.Task<GraphQL.GraphQLResponse<VRchatLogDataModel.vrChatLogitemJOSN>>> requests = new LinkedList<System.Threading.Tasks.Task<GraphQL.GraphQLResponse<VRchatLogDataModel.vrChatLogitemJOSN>>>();
                var logFiles = VRchatLogDataModel.LogProcessor.ListVrchatLogFiles();
                int fileCount = logFiles.Length + 1;
                int currentFile = 1;
                var graphQLClient = new GraphQLHttpClient("https://hlfxzmed2jh4zgm37n4kdy5kna.appsync-api.us-east-2.amazonaws.com/graphql", new NewtonsoftJsonSerializer());
                graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                foreach (var logFile in logFiles)
                {
                    using StreamReader reader = new(logFile);
                    var logitems = VRchatLogDataModel.LogProcessor.processVRchatLogFile(reader);
                    logitems = VRchatLogDataModel.LogProcessor.FilterToOnlyPlayerJoinOrLeave(logitems);


                    int progress = 0;
                    foreach (var item in logitems)
                    {
                        string query = item.ToPlayeritem(this.UserNameBox.Text.Trim()).toGraphQLcreationString();
                        Console.WriteLine(query);
                        var dataupdate = new GraphQLRequest(query);
                        var sentRequest = graphQLClient.SendMutationAsync<vrChatLogitemJOSN>(dataupdate);
                        await sentRequest;
                        //sentRequest.Wait();
                        requests.AddLast(sentRequest);
                       
                        
                        sentRequest.Wait();
                        var response = sentRequest.Result;
                        if (response.Errors != null)
                        {
                            foreach (var error in response.Errors)
                            {
                                Console.WriteLine(error.Message);

                            }
                            throw new Exception(response.Errors[0].Message);
                        }
                        
                        Console.WriteLine(response.Data);
                        
                        progress++;
                        this.Output.Text = $"{progress} out of {logitems.Length} done in file: {currentFile} of {fileCount}";
                    }
                    currentFile++;
                }
                //
                MessageBox.Show("All Done! The program will now close");
                System.Windows.Forms.Application.Exit();
            }





        }

        public static AuthFlowResponse AuthenticateWithSrp(string username, string password)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), FallbackRegionFactory.GetRegionEndpoint());
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

        private void UserNameBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void Output_Click(object sender, EventArgs e)
        {

        }
    }
}
