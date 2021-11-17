using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace DataverseUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            ConfigHelper config = ConfigHelper.ReadFromJsonFile("appsettings.json");
            var appConfig = config.PublicClientApplicationOptions;
            var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(appConfig)
                                                    .Build();
            var httpClient = new HttpClient();

            DataverseHelper dataverseUtil = new DataverseHelper(app, httpClient, config.DataverseBaseEndpoint);
            await dataverseUtil.DisplayWhoAmIUsingDataverseRetryingWhenWrongCredentialsAsync();
        }
    }
}
