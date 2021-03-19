using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Library.Models;
using Library.Service;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot1
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        public async Task MainAsync()
        {

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            //Set Firestore Cred Env Var
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Discord-f9415e7f6219.json");

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddScoped<IDatabaseService>(x => new DatabaseService(config))
                .BuildServiceProvider();

            var databaseService = serviceProvider.GetService<IDatabaseService>();

            _client = new DiscordSocketClient();

            _client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            //var token = "token";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            //var token = config.Token;

            //await _client.LoginAsync(TokenType.Bot, config.Token);
            //await _client.StartAsync();
            var newResponse = databaseService.CreateCustomResponse("Hooligan", "Hoos your daddy, Boot!");
            var responses = databaseService.GetCustomResponses("Hooligan");

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        //private async Task<string> GetCommands()
        //{

        //}

        private Task Log(LogMessage msg)
        {
            
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
