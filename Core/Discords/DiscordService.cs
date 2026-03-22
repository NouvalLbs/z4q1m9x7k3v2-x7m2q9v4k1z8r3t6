using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using SampSharp.GameMode.SAMP;
using ProjectSMP.Core.Discords.Models;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordService
    {
        private static DiscordSocketClient _client;
        private static InteractionService _interactions;
        private static Timer _responseTimer;
        private static bool _isRunning;

        private const string Token = "MTAxMzQyMzMxNzA0ODM3MzMxOA.Gl9mlc.wcQcfLiKTM0A5JEh8FzSlmKRhqBkCIsLfjuDQQ";
        private const ulong GuildId = 1037861564851695769;

        public static async Task InitializeAsync()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                AlwaysDownloadUsers = true
            };

            _client = new DiscordSocketClient(config);
            _interactions = new InteractionService(_client);

            _client.Log += Log;
            _client.Ready += OnReady;
            _client.InteractionCreated += HandleInteraction;

            await _interactions.AddModuleAsync<DiscordCommands>(null);

            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            _responseTimer = new Timer(3000, true);
            _responseTimer.Tick += ProcessResponses;

            _isRunning = true;
            Console.WriteLine("[Discord] Service initialized");
        }

        public static async Task ShutdownAsync()
        {
            _isRunning = false;
            _responseTimer?.Dispose();

            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                _client.Dispose();
            }

            Console.WriteLine("[Discord] Service stopped");
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine($"[Discord] {msg}");
            return Task.CompletedTask;
        }

        private static async Task OnReady()
        {
            try
            {
                await _interactions.RegisterCommandsToGuildAsync(GuildId);
                Console.WriteLine($"[Discord] Bot ready! Logged in as {_client.CurrentUser.Username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord] Error registering commands: {ex.Message}");
            }
        }

        private static async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(context, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord] Error handling interaction: {ex.Message}");

                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.RespondAsync($"Error: {ex.Message}", ephemeral: true);
            }
        }

        private static async void ProcessResponses(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            try
            {
                var responses = await DatabaseManager.QueryAsync<DiscordResponse>(
                    "SELECT * FROM discord_responses WHERE processed = 0 LIMIT 10");

                foreach (var response in responses)
                {
                    await SendResponse(response);

                    await DatabaseManager.ExecuteAsync(
                        "UPDATE discord_responses SET processed = 1 WHERE id = @Id",
                        new { response.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord] Error processing responses: {ex.Message}");
            }
        }

        private static async Task SendResponse(DiscordResponse response)
        {
            try
            {
                var user = await _client.GetUserAsync(response.UserId);
                if (user == null) return;

                if (!string.IsNullOrEmpty(response.EmbedData))
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithDescription(response.Message)
                        .WithColor(Discord.Color.Blue)
                        .WithCurrentTimestamp();

                    await user.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    await user.SendMessageAsync(response.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord] Error sending response to {response.UserId}: {ex.Message}");
            }
        }

        public static async Task SendDMAsync(ulong userId, string message, Embed embed = null)
        {
            try
            {
                var user = await _client.GetUserAsync(userId);
                if (user == null) return;

                if (embed != null)
                    await user.SendMessageAsync(message, embed: embed);
                else
                    await user.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord] Error sending DM: {ex.Message}");
            }
        }

        public static bool IsRunning => _isRunning;
    }
}