using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using MMDTools;
using Newtonsoft.Json;
using SQLitePCL;
using Varyl.BattleSystem;
using Varyl.Containers;
using VarylExtensions;
using static Varyl.BaseCommands;
using static VarylExtensions.Extensions;
using BindingFlags = System.Reflection.BindingFlags;
using Color = System.Drawing.Color;

namespace Varyl {

	[Group("inventory"), Aliases("inv")]
	public class InventoryCommands : BaseCommandModule {
		[Command("use")]
		public async Task UseItemAsync(CommandContext ctx, string ItemToUse) {
			var character = IsUsingCharacter(ctx.User.Id);
			if (character != null) {
				Player player = await Player.Load((long) character);
				player.FillInventory();
				var gameItem = player.Inventory.First(item => item.Type == ItemType.Consumable && item.Name.StartsWith(ItemToUse));
				if (gameItem != null) gameItem.Action();
			}
		}
	}

	// ReSharper disable once ClassNeverInstantiated.Global
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Group("owner"), Aliases("o"), RequireOwner]
	public class OwnerCommands : BaseCommandModule {
		[Command("shutdown")]
		public async Task ShutdownAsync(CommandContext ctx) {
			await ctx.RespondAsync("Shutting down.");
			Environment.Exit(938812094);
		}
		
		[Command("kick_bot")]
		public async Task KickBotAsync(CommandContext ctx) {
			await ctx.Guild.LeaveAsync();
		}
		
		[Command("get_hooks")]
		public async Task GetHooksAsync(CommandContext ctx) {
			var hooks = await ctx.Guild.GetWebhooksAsync();
			var s = string.Empty;
			for (var index = 0; index < hooks.Count; index++) {
				s += $"{index + 1}: {hooks[index].Name}\n";
			}

			await ctx.Channel.SendMessageAsync(s.Length > 0 ? s : "None");
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
		
		[Command("webhook")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public async Task SetupCommand(CommandContext ctx) {
			try {
				if (ctx.Member.Permissions != Permissions.Administrator && ctx.User.Id != 168407391317000192) 
					return;
				await ctx.Channel.CreateWebhookAsync("Varyl");
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}
		
	}
	
	public class BaseCommands : BaseCommandModule {
		const string ArrowRight = "→";
		[Command("image")]
		public async Task setImage(CommandContext cmd, string uri = "") {
			string s = string.Empty;
			var characterId = IsUsingCharacter(cmd.User.Id);
			if (characterId == null) return;
			var c = cmd.Message.Attachments;
			if (c.Count > 0) {
				foreach (var attachment in c) {
					s = attachment.Url;
				}
			}
			else {
				
				if (string.IsNullOrEmpty(uri)) {
					await cmd.RespondAsync("Neither a file, nor a uri was found.");
					return;
				}

				s = uri;
			}
			Player player = await Player.Load((long) characterId);
			player.ProfileUri = s;
			player.Update();
		}
		
		[Command("walk")]
		public async Task walk(CommandContext context, string direction) {
			var characterId = IsUsingCharacter(context.User.Id);
			if (characterId != null) {
				Player user = await Player.Load((long) characterId);
				user.LoadPosition();
				direction = direction.ToLowerInvariant();
				switch (direction) {
					case "left":
						user.Position.X -= 1;
						break;
					case "up":
						user.Position.Y += 1;
						break;
					case "right":
						user.Position.X += 1;
						break;
					case "down":
						user.Position.Y -= 1;
						break;
					default:
						return;
				}
				

				await context.RespondAsync("You have walked 10 meter");
				user.SavePosition();
			}
		}

		public static long? IsUsingCharacter(ulong id)
		{
			if (_ocCharacters.ContainsKey(id))
				return _ocCharacters[id];


			return null;
		}
		

		[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
		private static Dictionary<ulong, long> _ocCharacters = new Dictionary<ulong, long>();

		[Command("status")]
		public async Task GetOcStatus(CommandContext context) {
			if (context.User.Id != 168407391317000192) return;
			var characterId = IsUsingCharacter(context.User.Id);
			if (characterId == null) return;
			Player player = await Player.Load((long) characterId);
			await context.Channel.SendMessageAsync($"{player.Name}\n\nHealth: {player.BaseHealth / player.Health * 100}%\nMana: {player.BaseMagic / player.Magic * 100}%\nA: {player.GetSocialOpinion()}");
		}

		// ReSharper disable once CognitiveComplexity
		public Task<Bitmap> CreateMap(Point point) {
			World world = new World();
			world.InitializeBiome();
			using Bitmap bitmap = new Bitmap(256, 256);
			for (var xIndex = -128; xIndex < 128; xIndex++) {
				for (var yIndex = -128; yIndex < 128; yIndex++) {
					var xn = xIndex / 2f - 0.5f - point.X / 2f;
					var yn = yIndex / 2f - 0.5f - point.Y / 2f;
					var noiseValue = world.GetNoise(xn, yn);
					noiseValue += 0.5f * world.GetNoise(xn * 2f, yn * 2f);
					noiseValue += 0.25f * world.GetNoise(xn * 4f, yn * 4f);
					noiseValue /= 1 + 0.5f + 0.25f;
					noiseValue += 0.4f;
					noiseValue = Math.Max(noiseValue, 0f);
					noiseValue = Math.Min(1f, noiseValue);
					if(noiseValue < .1f)
						bitmap.SetPixel(xIndex, yIndex, Color.Aqua);
					else if(noiseValue < 0.15f)
						bitmap.SetPixel(xIndex, yIndex, Color.SandyBrown);
					else if(noiseValue < 0.3)
						bitmap.SetPixel(xIndex, yIndex, Color.ForestGreen);
					else if(noiseValue < 0.5)
						bitmap.SetPixel(xIndex, yIndex, Color.LimeGreen);
					else if(noiseValue < 0.7)
						bitmap.SetPixel(xIndex, yIndex, Color.YellowGreen);
					else if(noiseValue < 0.9)
						bitmap.SetPixel(xIndex, yIndex, Color.Yellow);
					else bitmap.SetPixel(xIndex, yIndex, Color.GhostWhite);

				}
			}
			return Task.FromResult(bitmap);
		}

		[Command("list")]
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

		[Command("use")]
		public async Task UseCharacterAsync(CommandContext ctx, [RemainingText] string name) {
			if (name.Length < 2) return;
			DiscordMessage message;
			await Open(Connection);
			await using (var cmd = Connection.CreateCommand()) {
				cmd.CommandText = "SELECT id, IsDead, DeadUntil from characters WHERE nick = @name AND Creator = @user";
				cmd.Parameters.AddWithValue("@name", name);
				cmd.Parameters.AddWithValue("@user", ctx.User.Id);
				await using var reader = cmd.ExecuteReader();
				if (reader.HasRows) {
					reader.Read();
					if (reader.GetInt64(1) == 1) {
						await ctx.Channel.SendMessageAsync(
							$"{name} is dead, you can revive them in {TimeSpan.FromTicks(reader.GetInt64(2) - DateTime.Now.Ticks).TotalHours:F1} hours");
						return;
					}

					if (_ocCharacters.ContainsKey(ctx.User.Id)) {
						_ocCharacters.Remove(ctx.User.Id);
					}

					var isSuccessful = _ocCharacters.TryAdd(ctx.User.Id, (long) reader.GetValue(0));
					if (isSuccessful)
						message = await ctx.Channel.SendMessageAsync($"Using {name}");
					else
						message = await ctx.Channel.SendMessageAsync(
							"There was a problem when selecting the character, please try again later.");
				}
				else message = await ctx.Channel.SendMessageAsync("No character by this name");
			}

			await ctx.Message.DeleteAsync();

			Timer(5000, async () => { await message.DeleteAsync(); });
		}

		[Command("purge"), Description("Deletes a certain amount of messages from the last 100 messages.")]
		public async Task PurgeCommand(CommandContext context,
			[Description("The user whose messages will be deleted.")]
			DiscordMember member,
			[Description("The maximum amount of message to delete. DEFAULT 5")]
			int max = 5) {
			if (context.Member.Permissions != Permissions.Administrator &&
			    context.User.Id != 168407391317000192) {
				await context.Channel.SendMessageAsync("[Exception] Permission.User");
				return;
			}

			try {
				var messagesAsync = await context.Channel.GetMessagesAsync();
				var messageCount = 0;
				foreach (var item in messagesAsync) {
					if (item.Author.Id == member.Id) {
						await item.DeleteAsync();
						await Task.Delay(1100);
						messageCount++;
					}

					if (messageCount >= max) break;
				}
			}
			catch (Exception error) {
				Console.WriteLine(error.Message);
			}
		}



		[Command("say")]
		[SuppressMessage("ReSharper", "CognitiveComplexity")]
		public async Task SayCommand(CommandContext ctx, [RemainingText] string messageContent) {
			var characterId = IsUsingCharacter(ctx.User.Id);
			if (characterId == null) return;

			await Task.Delay(500);
			await ctx.Message.DeleteAsync();
			var hooksAsync = await ctx.Guild.GetWebhooksAsync();
			if (hooksAsync.Count > 0) {
				DiscordWebhook hook = null;
				var enumerator = hooksAsync.GetEnumerator();
				while (enumerator.MoveNext()) {
					if (enumerator.Current != null && enumerator.Current.Name != "Varyl") continue;
					hook = enumerator.Current;
					break;
				}
				enumerator.Dispose();
				if (hook == null) return;
				if (hook.ChannelId != ctx.Channel.Id)
					await hook.ModifyAsync("Varyl", default, ctx.Channel.Id);

				try {
					Player player = await Player.Load((long) characterId);
					var builder = new DiscordWebhookBuilder {
						Username = player.Name,
						Content = messageContent
					};

					if (!string.IsNullOrEmpty(player.ProfileUri)) {
						builder.AvatarUrl = player.ProfileUri;
					}

					await hook.ExecuteAsync(builder);
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		[Command("profile")]
		[SuppressMessage("ReSharper", "CognitiveComplexity")]
		public async Task ProfileCmd(CommandContext context) {
			try {
				var characterId = IsUsingCharacter(context.User.Id);

				if (characterId != null) {
					var embed = new DiscordEmbedBuilder();
					Player player = await Player.Load((long) characterId);
					embed.WithTitle("Profile");
					embed.AddField("Name", player.Name);
					if(!string.IsNullOrEmpty(player.ProfileUri))
						embed.WithThumbnail(player.ProfileUri);
					var messageBuilder = new DiscordMessageBuilder();
					messageBuilder.WithEmbed(embed);
					await context.RespondAsync(messageBuilder);
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				throw;
			}
			
		}
	}
}