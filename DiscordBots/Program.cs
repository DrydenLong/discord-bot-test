using Discord;
using Discord.WebSocket;
using Library.Models;
using Library.Service;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TheArchitek
{
    class Program
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _client;
        private readonly IDatabaseService _dbService;

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.Equals(environment, "PROD"))
            {
                _config.Token = Environment.GetEnvironmentVariable("Token");
                _config.DatabaseConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            }
            else
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            }

            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddScoped<IDatabaseService>(x => new DatabaseService(_config))
                .BuildServiceProvider();

            _dbService = serviceProvider.GetService<IDatabaseService>();

            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();
            // Block this task until the program is closed.
            await Task.Delay(Timeout.Infinite);
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself. Contains(x, StringComparer.OrdinalIgnoreCase)
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            if (message.Content.StartsWith("+Architek"))
            {
                await AdminTools(message);
                return;
            }


            var words = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var allResponses = _dbService.GetAllCustomResponses();
            var command = allResponses.FirstOrDefault(x => words.Contains(x.Command.Trim(), StringComparer.OrdinalIgnoreCase));

            if (command != null)
            {
                await message.Channel.SendMessageAsync(command.Response);
            }
                
        }

        private async Task AdminTools(SocketMessage message)
        {
            try
            {
                var admins = _dbService.GetAdmins();
                var admin = admins.FirstOrDefault(x => x.DiscriminatorValue == message.Author.DiscriminatorValue && x.Active);
                if (admin == null)
                {
                    return;
                }

                var helpMessages = new List<string>
                {
                    "Here's what I can do:",
                    "\"+Architek Add Command {Command}/{Response}\" - Adds a custom command and repsonse.",
                    "\"+Architek Remove Command {Command}\" - Removes a custom command and repsonse."
                };


                if (admin.SuperAdmin)
                {
                    helpMessages.Add("\"+Architek Add Admin #{UserId}\" - Creates a new admin user who can add, edit and delete commands.");
                    helpMessages.Add("\"+Architek Add Super Admin #{UserId}\" - Creates a new super admin user who can add, edit and delete commands and users.");
                    helpMessages.Add("\"+Architek Remove Admin #{UserId}\" - Removes an existing admin.");

                    if (message.Content.Contains("Add Admin #", StringComparison.OrdinalIgnoreCase))
                    {
                        var newAdmin = message.Content.Substring(message.Content.LastIndexOf('#') + 1);
                        int.TryParse(newAdmin, out var newAdminId);
                        if (newAdminId > 0 && _dbService.AddAdmin(newAdminId, false))
                        {
                            await message.Channel.SendMessageAsync("Admin Added");
                            return;
                        }
                    }

                    if (message.Content.Contains("Add Super Admin #", StringComparison.OrdinalIgnoreCase))
                    {
                        var newAdmin = message.Content.Substring(message.Content.LastIndexOf('#') + 1);
                        int.TryParse(newAdmin, out var newAdminId);
                        if (newAdminId > 0 && _dbService.AddAdmin(newAdminId, true))
                        {
                            await message.Channel.SendMessageAsync("Super Admin Added");
                            return;
                        }
                    }

                    if (message.Content.Contains("Remove Admin #", StringComparison.OrdinalIgnoreCase))
                    {
                        var newAdmin = message.Content.Substring(message.Content.LastIndexOf('#') + 1);
                        int.TryParse(newAdmin, out var newAdminId);
                        if (newAdminId > 0 && _dbService.RemoveAdmin(newAdminId))
                        {
                            await message.Channel.SendMessageAsync("Bye Bitch!");
                            return;
                        }
                    }
                }

                if (message.Content.Contains("Help", StringComparison.OrdinalIgnoreCase))
                {
                    await message.Channel.SendMessageAsync(string.Join(Environment.NewLine, helpMessages));
                    return;
                }

                if (message.Content.Contains("Add Command", StringComparison.OrdinalIgnoreCase))
                {
                    var newMessage = message.Content.Substring(message.Content.LastIndexOf("Add Command ", StringComparison.OrdinalIgnoreCase) + 1);
                    var newCommand = newMessage.Split('/').FirstOrDefault();
                    var newResponse = newMessage.Split('/').LastOrDefault();

                    if (!string.IsNullOrEmpty(newCommand) && _dbService.CreateCustomResponse(newCommand, newResponse))
                    {
                        await message.Channel.SendMessageAsync($"Command \"{newCommand}\" added");
                        return;
                    }
                }

                if (message.Content.Contains("Remove Command", StringComparison.OrdinalIgnoreCase))
                {
                    var command = message.Content.Substring(message.Content.LastIndexOf("Remove Command ", StringComparison.OrdinalIgnoreCase) + 1);

                    if (!string.IsNullOrEmpty(command) && _dbService.RemoveCustomResponse(command))
                    {
                        await message.Channel.SendMessageAsync($"Command \"{command}\" removed");
                        return;
                    }
                }

                await message.Channel.SendMessageAsync("Sorry, I don't know that command. You can type \"+Architek Help\" to see what I can do.");
            }
            catch (Exception ex)
            {
                var log = new LogMessage(LogSeverity.Error, "", $"{ex}", ex);
                await LogAsync(log);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
