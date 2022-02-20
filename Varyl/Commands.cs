using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Data.Sqlite;

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
			if (connection.State == ConnectionState.Closed) return;
			await connection.CloseAsync();
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
				command.CommandText =
					"INSERT INTO `characters` (`character_name`, `by_user`, `nick`) SELECT @name, @user, @name WHERE NOT EXISTS(SELECT * FROM `characters` WHERE `character_name`= @name AND `by_user` = @user);"; // New Way
				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@user", context.User.Id);
				var reader = command.ExecuteReader();
				await context.RespondAsync(reader.RecordsAffected > 0
					? $"Created Character {name}"
					: "Character already exists.");
			}

			await Connection.CloseAsync();
		}

		public struct UserOcData {
			public long Id;
		}

		[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
		public static Dictionary<ulong, UserOcData> OcCharacter = new Dictionary<ulong, UserOcData>();

		[Command("stats")]
		public async Task StatsCommand(CommandContext ctx) {
			try {
				Connection.Open();
				await using (var command = Connection.CreateCommand()) {
					command.CommandText = ("SELECT COUNT(nick), SUM(messages) FROM characters WHERE by_user = @id");
					command.Parameters.AddWithValue("@id", ctx.User.Id);
					await using var reader = command.ExecuteReader();
					reader.Read();
					await ctx.RespondAsync(
						$"Total Characters: {reader.GetInt32(0)}\nTotal Messages: {reader.GetInt32(1)}");
				}

				await Close(Connection);
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				throw;
			}
		}

		[Command("list")]
		public async Task ListCommand(CommandContext ctx, DiscordUser discordUser = null, string search = "") {
			var user = discordUser != null ? discordUser.Id : ctx.User.Id;
			// List add characters by user
			Connection.Open();
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT character_name FROM characters WHERE by_user = @user";
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
				cmd.CommandText = "SELECT id from characters WHERE character_name = @name AND by_user = @user";
				cmd.Parameters.AddWithValue("@name", name);
				cmd.Parameters.AddWithValue("@user", ctx.User.Id);
				await using var reader = cmd.ExecuteReader();
				if (reader.HasRows) {
					reader.Read();
					if (OcCharacter.ContainsKey(ctx.User.Id)) {
						OcCharacter.Remove(ctx.User.Id);
					}

					var isSuccessful = OcCharacter.TryAdd(ctx.User.Id, new UserOcData {
						Id = (long) reader.GetValue(0)
					});
					if (isSuccessful)
						message = await ctx.Channel.SendMessageAsync($"Using {name}");
					else
						message = await ctx.Channel.SendMessageAsync(
							"There was a problem when selecting the character, please try again later.");
				}
				else message = await ctx.Channel.SendMessageAsync("No character by this name");
			}

			await ctx.Message.DeleteAsync();

			Timer(2000, async () => { await message.DeleteAsync(); });
		}

		[Command("purge"), Description("Deletes a certain amount of messages from the last 100 messages.")]
		public async Task PurgeCommand(CommandContext context,
			[Description("The user whose messages will be deleted.")]
			DiscordMember member,
			[Description("The maximum amount of message to delete. DEFAULT 5")]
			int max = 5) {
			if (context.Member.Permissions != DSharpPlus.Permissions.Administrator &&
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
						await Task.Delay(1000);
						messageCount++;
					}

					if (messageCount >= max) break;
				}
			}
			catch (Exception error) {
				Console.WriteLine(error.Message);
			}
		}

		[Command("createWebhook")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public async Task SetupCommand(CommandContext ctx) {
			try {
				await ctx.Channel.CreateWebhookAsync("Varyl");
				var message = await ctx.RespondAsync("Webhook Varyl created");
				Timer(2500, async () => { await message.DeleteAsync(); });
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		private static void Timer(int milliseconds, Action action) {
			var timer = new Timer(milliseconds) {
				AutoReset = false
			};
			timer.Elapsed += delegate
			{
				action.Invoke();
				timer.Stop();
				timer.Dispose();
			};
			timer.Start();
		}


		[Command("nick")]
		public async Task NickCommand(CommandContext ctx, [RemainingText] string text) {
			var characterId = Varyl.OcCharacter.IsUsingCharacter(ctx.User.Id);
			if (characterId == null) {
				await ctx.RespondAsync("No character selected, please first select a character using `<use`");
				return;
			}

			if (text.Length > 32) {
				await ctx.RespondAsync("Name must be less or equal to 32 characters in length");
				return;
			}

			if (text.Length < 1) {
				await ctx.RespondAsync("Name must but bigger or equal to 1 character in length");
				return;
			}

			await Connection.OpenAsync();
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "UPDATE `characters` SET `nick` = @name WHERE characters.id = @id";
				command.Parameters.AddWithValue("@name", text);
				command.Parameters.AddWithValue("@id", characterId);

				await using var result = command.ExecuteReader();
				if (result.RecordsAffected > 0) {
					await ctx.RespondAsync("Name successfully updated");
				}
				else await ctx.RespondAsync("Name update failed");
			}


			await Connection.CloseAsync();
		}

		[Command("say")]
		[SuppressMessage("ReSharper", "CognitiveComplexity")]
		public async Task SayCommand(CommandContext ctx, [RemainingText] string text) {
			long id = 0;
			var usesCharacter = false;

			if (OcCharacter.ContainsKey(ctx.User.Id)) {
				id = OcCharacter[ctx.User.Id].Id;
				usesCharacter = true;
			}

			if (!usesCharacter) {
				await ctx.RespondAsync("No character selected");
				return;
			}

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
					Connection.Open();

					await using var cmd = Connection.CreateCommand();
					cmd.CommandText =
						"SELECT characters.nick, profile_picture.uri FROM characters LEFT JOIN profile_picture ON characters.id = profile_picture.id WHERE characters.id = @id; UPDATE characters SET messages = messages + 1 WHERE characters.id = @id2";
					cmd.Parameters.AddWithValue("@id", id);
					cmd.Parameters.AddWithValue("@id2", id);


					await using var result = cmd.ExecuteReader();
					if (result.HasRows) {
						result.Read();
						var name = result.GetString(0);

						var builder = new DiscordWebhookBuilder {
							Username = name,
							Content = text
						};

						if (!result.IsDBNull(1)) {
							builder.AvatarUrl = result.GetString(1);
						}

						await hook.ExecuteAsync(builder);

						await Connection.CloseAsync();
					}
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		[Command("addfield")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public async Task AddFieldCommand(CommandContext ctx, string header, [RemainingText] string field) {
			var characterId = Varyl.OcCharacter.IsUsingCharacter(ctx.User.Id);
			if (characterId != null) {
				try {
					await Open(Connection);
					await using (var command = Connection.CreateCommand()) {
						command.CommandText =
							"INSERT INTO character_fields (character_id, field_type, field_content, header) VALUES (@character, 1, @content, @header)";
						command.Parameters.AddWithValue("@character", characterId);
						command.Parameters.AddWithValue("@content", field);
						command.Parameters.AddWithValue("@header", header);
						command.ExecuteNonQuery();
					}

					await ctx.RespondAsync("Field added");
					await Close(Connection);
				}
				catch (Exception e) {
					Console.WriteLine(e.Message);
					throw;
				}
			}
		}

		[Command("removefield")]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public async Task RemoveFieldCommand(CommandContext ctx, string field) {
			var characterId = Varyl.OcCharacter.IsUsingCharacter(ctx.User.Id);
			if (characterId != null) {
				await Open(Connection);
				await using (var command = Connection.CreateCommand()) {
					command.CommandText = "DELETE FROM character_fields WHERE character_id = @id AND header = @header;";
					command.Parameters.AddWithValue("@id", characterId);
					command.Parameters.AddWithValue("@header", field);
					command.ExecuteNonQuery();
				}

				await ctx.RespondAsync("Field deleted");
				await Close(Connection);
			}
		}

		[Command("profile")]
		[SuppressMessage("ReSharper", "CognitiveComplexity")]
		public async Task ProfileCmd(CommandContext context) {
			try {
				var characterId = Varyl.OcCharacter.IsUsingCharacter(context.User.Id);

				if (characterId != null) {
					var embed = new DiscordEmbedBuilder();
					await Open(Connection);
					var command = Connection.CreateCommand();
					command.CommandText =
						"SELECT characters.character_name, characters.nick, characters.messages, profile_picture.uri FROM characters LEFT JOIN profile_picture ON characters.id = profile_picture.id WHERE characters.id = @id;";
					command.Parameters.AddWithValue("@id", characterId);
					await using var result = command.ExecuteReader();
					
					if (result.HasRows) {
						result.Read();
						var name = result.GetString(0);
						var nick = result.GetString(1);
						var messages = result.GetInt32(2);
						embed.WithTitle("Profile");
						embed.AddField("Name", name);
						if (name != nick) {
							embed.AddField("Nickname", nick);
						}

						embed.AddField("Ranking", $"Lv.{Level.LevelBuilder(messages)}");
						if (!result.IsDBNull(3)) {
							embed.WithThumbnail(result.GetString(3));
						}

						var messageBuilder = new DiscordMessageBuilder();
						messageBuilder.WithEmbed(embed);
						command = Connection.CreateCommand();
						command.CommandText = "SELECT * FROM character_fields WHERE character_id = @id";
						command.Parameters.AddWithValue("@id", characterId);


						await using (var reader = command.ExecuteReader()) {
							if (reader.HasRows) {
								var memoryStream = new MemoryStream();
								while (reader.Read()) {
									var type = reader.GetInt32(1);
									if (type != 1) continue;
									var content = reader.GetString(2);
									var contentBytes =
										Encoding.UTF8.GetBytes(reader.GetString(3) + "\n" + content + "\n\n\n");
									memoryStream.Write(contentBytes, 0, contentBytes.Length);
								}

								if (memoryStream.Length > 0) {
									memoryStream.Position = 0;

									messageBuilder.WithFile("fields.txt", memoryStream);
								}
							}
						}

						await context.RespondAsync(messageBuilder);
					}

					await Close(Connection);
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				throw;
			}
		}

		[Command("Image"),
		 Description("Apply an image to the profile picture of a character, must include a uploaded image.")]
		public async Task ImageCommand(CommandContext ctx) {
			var attachments = ctx.Message.Attachments;

			var characterId = Varyl.OcCharacter.IsUsingCharacter(ctx.User.Id);
			if (attachments.Count > 0 && characterId != null) {
				var file = attachments[0].Url;
				await Open(Connection);
				var cmd = Connection.CreateCommand();
				cmd.CommandText =
					"CREATE TABLE IF NOT EXISTS profile_picture (id INTEGER, uri TEXT); DELETE FROM profile_picture WHERE id = @id; INSERT INTO profile_picture (id, uri) VALUES (@id, @uri)";
				cmd.Parameters.AddWithValue("@id", characterId);
				cmd.Parameters.AddWithValue("@uri", file);
				await cmd.ExecuteNonQueryAsync();
				await ctx.RespondAsync("Uploaded image");
				await Close(Connection);
			}
		}
	}
}