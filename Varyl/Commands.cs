using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Data.Sqlite;
using Varyl.BattleSystem;

namespace Varyl {
	
	// ReSharper disable once ClassNeverInstantiated.Global
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public class Commands : BaseCommandModule {
		public static readonly SqliteConnection Connection = new SqliteConnection("Data Source=database.db3");

		public static async Task Open(DbConnection connection) {
			if (connection.State == ConnectionState.Open) {
				return;
			}

			await connection.OpenAsync();
		}

		public static async Task Close(DbConnection connection) {
			if (connection.State == ConnectionState.Closed) {
				return;
			}
			
			await connection.CloseAsync();
		}

		[Command("image")]
		public async Task setImage(CommandContext cmd, string uri = "") {
			string s = string.Empty;
			var characterId = IsUsingCharacter(cmd.User.Id);
			if (characterId == null) return;
			var c = cmd.Message.Attachments;
			if (c.Count > 0) {
				using (var enumerator = c.GetEnumerator()) {
					enumerator.MoveNext();
					if (enumerator.Current != null) 
						s = enumerator.Current.Url;
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
				user.CachePosition();
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
				if (user.Buffs.Length > 0) {
					foreach (var buff in user.Buffs) {
						buff.OnWalk(ref user, user.Position.X, user.Position.Y);
					}
				}
				user.UpdatePosition();
			}
		}

		[Command("create")]
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

			await Connection.CloseAsync();
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
		public async Task ListCommand(CommandContext ctx, DiscordUser discordUser = null, string search = "") {
			var user = discordUser != null ? discordUser.Id : ctx.User.Id;
			// List add characters by user
			Connection.Open();
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT nick FROM Characters WHERE Creator = @user";
				if (search != string.Empty) {
					command.CommandText += " AND character_name LIKE @search";
					command.Parameters.AddWithValue("@search", search);
				}

				command.Parameters.AddWithValue("@user", user);
				await using var reader = command.ExecuteReader();
				if (!reader.HasRows) {
					await ctx.RespondAsync($"No characters by user {user}");
				}
				else {
					var characters = string.Empty;
					while (reader.Read()) {
						characters += $"{reader.GetString(0)}\n";
					}

					await ctx.RespondAsync(characters);
				}
			}

			Connection.Close();
		}

		[Command("use")]
		public async Task UseCommand(CommandContext ctx, [RemainingText] string name) {
			if (name.Length < 2) return;
			DiscordMessage message;
			Connection.Open();
			await using (var cmd = Connection.CreateCommand()) {
				cmd.CommandText = "SELECT id from characters WHERE nick = @name AND Creator = @user";
				cmd.Parameters.AddWithValue("@name", name);
				cmd.Parameters.AddWithValue("@user", ctx.User.Id);
				await using var reader = cmd.ExecuteReader();
				if (reader.HasRows) {
					reader.Read();
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

			VarylExtensions.Extensions.Timer(2000, async () => { await message.DeleteAsync(); });
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
				using (var hooks = hooksAsync.GetEnumerator()) {
					while (hooks.MoveNext()) {
						if (hooks.Current != null && hooks.Current.Name == "Varyl") {
							hook = hooks.Current;
						}
					}
				}

				if (hook == null) return;
				if (hook.ChannelId != ctx.Channel.Id)
					await hook.ModifyAsync("Varyl", default, ctx.Channel.Id);

				try {
					Player player = await Player.Load((long) characterId);
					var builder = new DiscordWebhookBuilder {
						Username = player.Name,
						Content = messageContent
					};

					if (string.IsNullOrEmpty(player.ProfileUri)) {
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