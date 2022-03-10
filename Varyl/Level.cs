using System;

namespace Varyl {
	public static class Level {
		private static double requiredXP(double level) {
			return 50 * level + 1 * Math.Pow(level + 1, 1.5) + 600;
		}

		private static string progressBuilder(int n, int max = 10) {
			string s = "[";
			for (int i = 0; i < n; i++) {
				s += "■";
			}

			for (int i = 0; i < max - n; i++) {
				s += "□";
			}

			return s + "]";
		}
		
        public static string LevelBuilder (double count)
        {
            try
            {
                var tempLevel = 1;
                var xp = count;

                while (xp >= requiredXP(tempLevel))
                {
                    xp -= requiredXP(tempLevel);
                    tempLevel++;
                }

                return $"{tempLevel}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }
	}
}