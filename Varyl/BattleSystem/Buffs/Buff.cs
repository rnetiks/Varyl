namespace Varyl.BattleSystem.Buffs {
	public class Buff {
		public virtual void OnPlayerAttack(Varyl.BattleSystem.Player player, IEnemy enemy){}
		public virtual void OnEnemyAttack(IEnemy enemy, Player player){}
		public virtual void OnPlayerEscape(){}
		public virtual void OnNewRound(){}
		public virtual void OnWalk(long x, long y){}
	}
}