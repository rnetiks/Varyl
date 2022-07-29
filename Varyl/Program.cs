using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Varyl.Containers;

namespace Varyl
{
    class Program
    {
        static void Main() => BotAsync().GetAwaiter().GetResult();

        static DSharpPlus.DiscordClient discord = null;
        
        static async Task BotAsync() {
            discord = new DSharpPlus.DiscordClient(new DSharpPlus.DiscordConfiguration
            {
                Token = Credentials.Create("cred.dpg"),
                TokenType = DSharpPlus.TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information
            });
            discord.UseInteractivity(new InteractivityConfiguration() {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "<" },
            });
            commands.RegisterCommands<BaseCommands>();
            commands.RegisterCommands<Trade>();
            commands.RegisterCommands<OwnerCommands>();
            commands.RegisterCommands<InventoryCommands>();
            commands.RegisterCommands<CreateCommands>();
            await discord.ConnectAsync();

            ulong discordChannel = 0;
            ulong discordGuild = 0; 
            while (true) { 
                var cmd = Console.ReadLine();
                DiscordGuild guild;
                DiscordChannel channel;

                if (cmd == null || !cmd.StartsWith(';')) continue;
                switch (cmd.Substring(1)) {
                    case "quit":
                        return;
                    case "guild":
                        discordGuild = ulong.Parse(cmd.Split()[1]);
                        guild = await discord.GetGuildAsync(discordGuild);
                        if (guild != null)
                            Console.Title = guild.Name;
                        break;
                    case "channel" when discordGuild != 0:
                        discordChannel = ulong.Parse(cmd.Split()[1]);
                        guild = await discord.GetGuildAsync(discordGuild);
                        channel = guild.GetChannel(discordChannel);
                        if (channel != null)
                            Console.Title = $"{Console.Title} - {channel.Name}";
                        break;
                    case "say" when discordGuild != 0 && discordChannel != 0:
                        guild = await discord.GetGuildAsync(discordGuild);
                        channel = guild.GetChannel(discordChannel);
                        var message = cmd.Substring(4);
                        await channel.SendMessageAsync(message);
                        break;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        }
    }
}
