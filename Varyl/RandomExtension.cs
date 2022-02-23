using System;

namespace Varyl {
	public static class RandomExtension
	{
		public static float NextFloat(this Random random, float min, float max)
		{
			double val = (random.NextDouble() * (max - min) + min);
			return (float)val;
		}
        
		public static float NextFloat(float min, float max)
		{
			Random random = new Random();
			double val = (random.NextDouble() * (max - min) + min);
			return (float)val;
		}

		public static int NextInteger() {
			Random random = new Random();
			return random.Next();
		}
		public static int NextInteger(int max) {
			Random random = new Random();
			return random.Next(max);
		}
	}
}