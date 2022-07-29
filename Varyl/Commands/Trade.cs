using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Varyl {
	[Group("trade")]
	public class Trade : BaseCommandModule {
		List<Trades> trades = new List<Trades>();
		
		[Command("confirm")]
		public async Task confirmTrade(CommandContext ctx) {
			foreach (var trade in trades.Where(trade => ctx.User == trade.User1 || ctx.User == trade.User2)) {
				if (DateTime.Now.Ticks > trade.Expire.Ticks) {
					trades.Remove(trade);
					return;
				}
				
				if (ctx.User == trade.User1) {
					if (trade.User1Inventory.Length <= 0) {
						return;
					}
					trade.User1Accepted = true;
				}
				else {
					if (trade.User2Inventory.Length <= 0) {
						return;
					}
					trade.User2Accepted = true;
				}

				return;
			}

			await ctx.Channel.SendMessageAsync("You have no ongoing trade.");
		}
		
		[Command("request")]
		public async Task requestTrade(CommandContext ctx, DiscordUser user) {
			if (ctx.User == user) {
				await ctx.Channel.SendMessageAsync("You can not send a trade request to yourself.");
				return;
			}

			if (user.IsBot) {
				await ctx.Channel.SendMessageAsync("You can not send a trade request to a bot.");
				return;
			}
			
			await ctx.Channel.SendMessageAsync(
				$"{user.Mention} do you wish to accept {ctx.User.Mention}'s trade offer?\nY/N");
			var answer = await ctx.Channel.GetNextMessageAsync(user, TimeSpan.FromSeconds(15));
			if (answer.Result.Content.ToLower() == "y") {
				var msg = await ctx.Channel.SendMessageAsync($"{ctx.User.Username}'s Inventory:\n\n{user.Username}'s Inventory:");
				trades.Add(new Trades() {
					Expire = DateTime.Now.AddSeconds(60),
					User1 = ctx.User,
					User2 = user,
					tradeMessage = msg
				});
			}
		}
	}
}