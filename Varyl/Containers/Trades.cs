using System;
using DSharpPlus.Entities;

namespace Varyl {
	internal class Trades {
		public DiscordUser User1, User2;
		public DateTime Expire;
		public bool User1Accepted, User2Accepted;
		public DiscordMessage tradeMessage;
		public object[] User1Inventory { get; set; }
		public object[] User2Inventory { get; set; }
	}
}