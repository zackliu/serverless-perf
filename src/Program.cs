using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.SignalR.Perf.Serverless;
using Microsoft.Extensions.Configuration;

namespace ServerlessPerf
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .Build();

            var connectionString = configuration["Azure:SignalR:ConnectionString"];
            connectionString =
                "Endpoint=http://localhost;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789;Version=1.0;Port=8080";

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("Azure:SignalR:ConnectionString");
            }

            Console.WriteLine("******Starting Serverless Perf Tests********");

            var client = new ClientHandler(connectionString, "SignalRHub1", 100);

            await client.StartAsync(10, 1);
            await client.SendMessagesAsync(10);
        }
    }
}
