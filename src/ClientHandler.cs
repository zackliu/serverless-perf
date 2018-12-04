using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Microsoft.Azure.SignalR.Perf.Serverless
{
    public class ClientHandler
    {
        private static string argument = new string('0', 2048);

        private Utils _utils;
        private int _clientCount;
        private string _url;
        private string _audiences;
        private List<HubConnection> _connections = new List<HubConnection>();

        public ClientHandler(string connectionString, string hubName, int clientCount)
        {
            _utils = new Utils(connectionString);

            _url = GetClientUrl(_utils.Endpoint, _utils.Port, hubName);
            _audiences = GetAudiences(_utils.Endpoint, hubName);

            _clientCount = clientCount;
        }

        public async Task StartAsync(int connectPerRound, int roundInterval)
        {
            int currentConnected = 0;
            while (currentConnected < _clientCount)
            {
                var tasks = new List<Task>();

                for (int i = 0; i < connectPerRound && currentConnected < _clientCount; i++, currentConnected++)
                {
                    string userId = Guid.NewGuid().ToString();
                    var connection = new HubConnectionBuilder()
                        .WithUrl(_url,
                            option =>
                            {
                                option.AccessTokenProvider =
                                    () => Task.FromResult(_utils.GenerateAccessToken(_audiences, userId));
                            })
                        .Build();
                    _connections.Add(connection);
                    tasks.Add(connection.StartAsync());
                }

                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromSeconds(roundInterval));
            }
        }

        public async Task SendMessagesAsync(int interval)
        {
            while (true)
            {
                var tasks = new List<Task>();
                Console.WriteLine($"Sending messages...");
                foreach (var connection in _connections)
                {
                    tasks.Add(connection.SendAsync("TestMethod", argument));
                }

                await Task.WhenAll(tasks);
                Console.WriteLine($"{tasks.Count} messages sent");
                await Task.Delay(TimeSpan.FromSeconds(interval));
            }
        }

        private string GetClientUrl(string endpoint, string port, string hubName)
        {
            return $"{endpoint}:{port}/client/?hub={hubName}";
        }

        private string GetAudiences(string endpoint, string hubName)
        {
            return $"{endpoint}/client/?hub={hubName}";
        }
    }
}
