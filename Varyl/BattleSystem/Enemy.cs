using System.Threading.Tasks;

namespace Varyl.BattleSystem {
	public interface IEnemy {
		Task<int> Attack(Player player);
		Task Defend();
		Task UseSkill();
		Task Update();
		Task<int> damageEntity(int damage);

		string Name { get; }
		string Description { get; }
		
		int Strength { get; }
		int Defense { get; }
		int Health { get; set; }
		int BaseHealth { get; }
		ulong Level { get; }
	}
	
}