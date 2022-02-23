using System;
using System.Linq;
using System.Threading.Tasks;
using static Varyl.RandomExtension;

namespace Varyl.BattleSystem.Enemies {
	public class Bandit : IEnemy {
		public Bandit(in int enemyLevel) {
			BaseHealth = (int) (enemyLevel * 487.14 * NextFloat(1f, 1.4f));
			Health = BaseHealth;
			Strength = (int) (enemyLevel * 48.273 * NextFloat(1f, 1.4f));
			Level = (ulong) enemyLevel;
		}

		public async Task<int> Attack(Player player) {
			return await player.DamageEntity((int) (Strength * NextFloat(1f, 1.4f)));
		}
		
		

		public Task Defend() {
			Defense *= 2;
		}

		public Task UseSkill() {
			Console.WriteLine("Not yet implemented");
		}

		public Task<int> damageEntity(int damage) {
			Health = Math.Max(0, Health - damage);
			return Task.FromResult(damage);
		}

		public string Name { get; } = "Bandit";
		public string Description { get; }
		public int Strength { get; }
		public int Defense { get; set; }
		public int Health { get; set; }
		public int BaseHealth { get; }
		public ulong Level { get; }
	}
}