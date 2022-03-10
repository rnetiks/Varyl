using System;

namespace Varyl.BattleSystem.Buffs {
	public class StrengthBuff : Buff {
		public StrengthBuff(float strength) {
			Strength = strength;
		}

		private float Strength { get; }
	}
}