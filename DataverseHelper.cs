using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

namespace DataverseUtil
{
    class DataverseHelper
    {
        public DataverseHelper(IPublicClientApplication app, HttpClient client, string dataVerseBaseEndpoint)
        {
            tokenAcquisitionHelper = new PublicAppUsingUsernamePassword(app);
            protectedApiCallHelper = new ProtectedApiCallHelper(client);
            DataverseBaseEndpoint = dataVerseBaseEndpoint;
            Scopes = new string[] { $"{DataverseBaseEndpoint}/.default" };
        }

        protected PublicAppUsingUsernamePassword tokenAcquisitionHelper;

        protected ProtectedApiCallHelper protectedApiCallHelper;

        /// <summary>
        /// Scopes to request access to the protected Web API (here Microsoft Graph)
        /// </summary>
        private static string[] Scopes { get; set; } //= new string[] { $"{DataverseBaseEndpoint}/.default" };

        /// <summary>
        /// Base endpoint for Microsoft Graph
        /// </summary>
        private string DataverseBaseEndpoint { get; set; }

        /// <summary>
        /// URLs of the protected Web APIs to call (here Microsoft Graph endpoints)
        /// </summary>
        private string WebApiUrlWhoAmI { get { return $"{DataverseBaseEndpoint}/api/data/v9.1/WhoAmI"; } }
        private string WebApiUrlSpecificWorkflow { get { return $"{DataverseBaseEndpoint}/api/data/v9.1/workflows(9f89ec4b-8956-eb11-a812-000d3a8df0ed)"; } }


        /// <summary>
        /// Calls the Web API and displays its information
        /// </summary>
        /// <returns></returns>
        public async Task DisplayMeAndMyManagerRetryingWhenWrongCredentialsAsync()
        {
            bool again = true;
            while (again)
            {
                again = false;
                try
                {
                    //await DisplayMeAndMyManagerAsync();
                    await DisplayDVWhoAmI();
                }
                catch (ArgumentException ex) when (ex.Message.StartsWith("U/P"))
                {
                    // Wrong user or password
                    WriteTryAgainMessage();
                    again = true;
                }
            }
        }

        /// <summary>
        /// Calls the Web API and displays its information
        /// </summary>
        /// <returns></returns>
        public async Task DisplayWhoAmIUsingDataverseRetryingWhenWrongCredentialsAsync()
        {
            bool again = true;
            while (again)
            {
                again = false;
                try
                {
                    await DisplayDVWhoAmI();
                }
                catch (ArgumentException ex) when (ex.Message.StartsWith("U/P"))
                {
                    // Wrong user or password
                    WriteTryAgainMessage();
                    again = true;
                }
            }
        }

        private async Task DisplayMeAndMyManagerAsync()
        {
            string username = ReadUsername();
            SecureString password = ReadPassword();

            AuthenticationResult authenticationResult = await tokenAcquisitionHelper.AcquireATokenFromCacheOrUsernamePasswordAsync(Scopes, username, password);
            if (authenticationResult != null)
            {
                DisplaySignedInAccount(authenticationResult.Account);

                string accessToken = authenticationResult.AccessToken;
                //await CallWebApiAndDisplayResultAsync(WebApiUrlMe, accessToken, "Me");
                //await CallWebApiAndDisplayResultAsync(WebApiUrlMyManager, accessToken, "My manager");
            }
        }

        private async Task DisplayDVWhoAmI()
        {
            string username = "admin@M365x229910.onmicrosoft.com"; // ReadUsername();
            SecureString password = ReadPassword();
            var dvScopes = new string[] { "https://org1fda8597.crm.dynamics.com/.default" };
            AuthenticationResult authenticationResult = await tokenAcquisitionHelper.AcquireATokenFromCacheOrUsernamePasswordAsync(Scopes, username, password);

            if (authenticationResult != null)
            {
                DisplaySignedInAccount(authenticationResult.Account);

                string dvUrl = "https://org1fda8597.crm.dynamics.com/api/data/v9.1/WhoAmI";
                string accessToken = authenticationResult.AccessToken;
                //Console.WriteLine(accessToken);
                await CallWebApiAndDisplayResultAsync(WebApiUrlWhoAmI, accessToken, "DV_WhoAmI");

                dvUrl = "https://org1fda8597.crm.dynamics.com/api/data/v9.1/workflows(9f89ec4b-8956-eb11-a812-000d3a8df0ed)";
                await CallWebApiAndDisplayResultAsync(WebApiUrlSpecificWorkflow, accessToken, "DV_GetSpecificFlowSchema");
            }
        }

        private static void WriteTryAgainMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Wrong user or password. Try again!");
            Console.ResetColor();
        }

        private static SecureString ReadPassword()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter your password (no backspace possible)");
            Console.ResetColor();
            SecureString password = new SecureString();
            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey(true);
                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }
                password.AppendChar(c.KeyChar);
                Console.Write("*");
            }
            Console.WriteLine();
            return password;
        }

        private static string ReadUsername()
        {
            string username;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter your username");
            Console.ResetColor();
            username = Console.ReadLine();
            return username;
        }

        private static void DisplaySignedInAccount(IAccount account)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{account.Username} successfully signed-in");
        }

        private async Task CallWebApiAndDisplayResultAsync(string url, string accessToken, string title)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(title);
            Console.ResetColor();
            await protectedApiCallHelper.CallWebApiAndProcessResultAsync(url, accessToken, Display);
            Console.WriteLine();
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
        }
    }
}
