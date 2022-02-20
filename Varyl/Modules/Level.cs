using System;
using DSharpPlus.CommandsNext;

namespace Varyl.Modules {
	public class Level : BaseCommandModule {
		private double getRequiredExperience(int currentLevel) {
			return 5 * (Math.Pow(currentLevel, 2)) + (50 * currentLevel) + 100;
			//Literally a fucking copy of mee6 level curve
		}
	}
}