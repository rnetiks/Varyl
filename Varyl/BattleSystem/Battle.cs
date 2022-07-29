using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using static Varyl.RandomExtension;

namespace Varyl.BattleSystem {
	public class Battle {
		private short Round = 1;
		private IEnemy[] enemyTeam;
		Player[] playerTeam;
		private bool _matchRunning = true;
		private const int MAX_ROUNDS = 30;
		
		public Battle(Player[] players, IEnemy[] enemies) {
			playerTeam = players;
			enemyTeam = enemies;
		}

		public async Task StartMatch(CommandContext context) {
			while (_matchRunning) {
				_matchRunning = await NextRound(context);
			}

			await using var ms = new MemoryStream();
			var content = Encoding.ASCII.GetBytes(MessageLog);
			ms.Write(content, 0, content.Length);
			ms.Position = 0;
			DiscordMessageBuilder builder = new DiscordMessageBuilder();
			builder.WithFile("Backlog.txt", ms);
			await context.Channel.SendMessageAsync(builder);
		}

		private string MessageLog { get; set; }

		private async Task<bool> NextRound(CommandContext context) {
			if (enemyTeam.All(e => e.Health <= 0)) {
				MessageLog += "You won!";
				return await Task.FromResult(false);
			}

			if (playerTeam.All(e => e.Health <= 0)) {
				MessageLog += "You lost... the party leader will lose 10% of their total level";
				return await Task.FromResult(false);
			}
			
			if (currentRound > MAX_ROUNDS) {
				MessageLog += "Took over 30 rounds, this is a draw and neither party wins anything";
				return await Task.FromResult(false);
			}
			
			foreach (var player in playerTeam) {
				try {
					var sortedEnemies = enemyTeam.Where(e => e.Health > 0).ToArray();
					if (sortedEnemies.Length == 0)
						break;
					var selectedEnemy = sortedEnemies[NextInteger(enemyTeam.Length)];
					var afflictedDamage = await player.Attack(selectedEnemy);
					MessageLog += $"[Lv.{player.Level} {player.Name} ({player.Health} hp)] has dealt {afflictedDamage} damage towards {selectedEnemy.Name} ({selectedEnemy.Health})\n";
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}

			}
			
			foreach (var enemy in enemyTeam) {
				if (enemy.Health <= 0) continue;
				var sortedPlayerList = playerTeam.Where(e => e.Health > 0).ToArray();
				if (sortedPlayerList.Length == 0) 
					break;
				var player = sortedPlayerList[NextInteger(sortedPlayerList.Length)];
				var afflictedDamage = await enemy.Attack(player);
				MessageLog += $"[Lv.{enemy.Level} {enemy.Name} ({enemy.Health})] attacked {player.Name} for {afflictedDamage} damage.\n";
			}
			

			
			currentRound++;
			return await Task.FromResult(true);
		}

		public int currentRound { get; set; }
	}
}