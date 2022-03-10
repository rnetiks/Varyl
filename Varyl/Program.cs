using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace Varyl
{
    class Program
    {
        static void Main() => BotAsync().GetAwaiter().GetResult();

        static DSharpPlus.DiscordClient discord = null;
        
        static async Task BotAsync() {
            discord = new DSharpPlus.DiscordClient(new DSharpPlus.DiscordConfiguration
            {
                Token = "ODc5NDY5MDYwOTM1NTM2Njcx.YSQLYw.TbCh5-rDLWf60yFLaJ0cbwyY4NQ",
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
            commands.RegisterCommands<Commands>();
            await discord.ConnectAsync();
            
            //await Task.Delay(-1);
            ulong discordChannel = 0;
            ulong discordGuild = 0; 
            for (;;) {
                string cmd = Console.ReadLine();
                if (cmd != null && cmd.StartsWith(";guild")) {
                    discordGuild = ulong.Parse(cmd.Split()[1]);
                    var _guild = await discord.GetGuildAsync(discordGuild);
                    if (_guild != null) Console.Title = _guild.Name;
                }

                if (cmd != null && cmd.StartsWith(";channel") && discordGuild != 0) {
                    discordChannel = ulong.Parse(cmd.Split()[1]);
                    var _guild = await discord.GetGuildAsync(discordGuild);
                    var _channel = _guild.GetChannel(discordChannel);
                    if (_channel != null) Console.Title = $"{Console.Title} - {_channel.Name}";
                }

                if (cmd != null && cmd.Trim().ToLowerInvariant() == ";quit") {
                    return;
                }
                
                if (cmd == null || !cmd.StartsWith(";say")) continue;
                if (discordChannel == 0 || discordGuild == 0) continue;
                var guild = await discord.GetGuildAsync(discordGuild);
                var channel = guild.GetChannel(discordChannel);
                var message = cmd.Substring(4);
                await channel.SendMessageAsync(message);
            }
        }
    }
}
