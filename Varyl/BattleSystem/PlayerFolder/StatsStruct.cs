namespace Varyl.BattleSystem {
	public partial class Player {
		public struct StatsStruct {
			public int Strength;
			public int Defense;
			public int Agility;
			public int Endurance;

			public StatsStruct(int strength, int defense, int agility, int endurance) {
				Strength = strength;
				Defense = defense;
				Agility = agility;
				Endurance = endurance;
			}
		}
	}
}