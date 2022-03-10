using System;

namespace Varyl.BattleSystem.Buffs {
	public class Buff {
		public virtual void OnPlayerAttack(Varyl.BattleSystem.Player player, IEnemy enemy){}
		public virtual void OnEnemyAttack(IEnemy enemy, Player player){}
		public virtual void OnPlayerEscape(){}
		public virtual void OnNewRound(){}

		public virtual void OnWalk(ref Player player, long x, long y) {
			Console.Write($"OnWalk(long x={x.ToString()}, long y={y.ToString()})");
		}
	}
}