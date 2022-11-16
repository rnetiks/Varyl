using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using DSharpPlus;
using static VarylExtensions.Extensions;
using Color = System.Drawing.Color;

namespace Varyl {

	// ReSharper disable once ClassNeverInstantiated.Global
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Group("owner"), Aliases("o"), RequireOwner]
	public class OwnerCommands : BaseCommandModule {
		[Command("kick")]
		public async Task KickBotAsync(CommandContext ctx, string args) {
			switch (args) {
				case "bot":
					await ctx.Guild.LeaveAsync();
					break;
			}
		}
		
		[Command("get")]
		public async Task GetHooksAsync(CommandContext ctx, string args) {
			switch (args) {
				case "hooks":
					var hooks = await ctx.Guild.GetWebhooksAsync();
					var s = string.Empty;
					for (var index = 0; index < hooks.Count; index++) {
						s += $"[{(index + 1).ToString()}] {hooks[index].Name}\n";
					}

					await ctx.Channel.SendMessageAsync(s.Length > 0 ? s : "There are no Webhooks configured. If you are the owner of the guild, please set up a Webhook in the guild settings first.");
					break;
			}
		}
	}
	
	[Group("create"), Aliases("c")]
	public class CreateCommands : BaseCommandModule {
		
		[Command("character")]
		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public async Task CreateCommand(CommandContext context, [RemainingText] string name) {
			if (name.Length < 2 || name.Length > 32) {
				await context.RespondAsync("Name needs to be between 2 to 32 characters");
				return;
			}

			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "INSERT INTO `characters` (nick, Creator, profile_uri) SELECT @name, @user, @n WHERE NOT EXISTS(SELECT * FROM Characters WHERE nick = @name AND Creator = @user);"; // New Way
				//command.CommandText = "INSERT INTO Characters (nick, Creator) SELECT @name, @user FROM dual WHERE NOT EXISTS (SELECT * FROM Characters WHERE nick = @name AND Creator = @user) LIMIT 1;";
				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@user", context.User.Id);
				command.Parameters.AddWithValue("@n", string.Empty);
				try { 				
					var reader = command.ExecuteReader();
					await context.RespondAsync(reader.RecordsAffected > 0
						? $"Created Character {name}"
						: "Character already exists.");
					
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}

			}

			Close(Connection);
		}

	}
	
	public class BaseCommands : BaseCommandModule {
		public static long? IsUsingCharacter(ulong id)
		{
			if (_ocCharacters.ContainsKey(id))
				return _ocCharacters[id];
			return null;
		}
		
		private static Dictionary<ulong, long> _ocCharacters = new Dictionary<ulong, long>();

		[Command("list"), Aliases("ls")]
		public async Task ListCharactersAsync(CommandContext ctx, DiscordUser discordUser = null, string search = "") {
			var user = discordUser != null ? discordUser.Id : ctx.User.Id;
			// List add characters by user
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT nick, IsDead FROM Characters WHERE Creator = @user";

				command.Parameters.AddWithValue("@user", user);
				await using var reader = command.ExecuteReader();
				if (!reader.HasRows) {
					await ctx.RespondAsync($"No characters by user {user}");
				}
				else {
					var characters = string.Empty;
					while (reader.Read()) {
						
						characters += $"- {reader.GetString(0)}";
						var isDead = reader.GetInt64(1);
						Console.WriteLine(isDead);
						if (isDead == 1) 
							characters += " [Dead]";
						characters += "\n";
					}

					await ctx.RespondAsync(characters);
				}
			}
			Close(Connection);
		}
	}
}