using static FastNoise;

namespace Varyl {
	public class World {
		private const int Seed = 1333;
		public void InitializeHeightmap(int seed = Seed) {
		
		}
		FastNoise noise = new FastNoise();
		
		public void InitializeBiome(int seed = Seed) {
			noise.SetFrequency(0.02f);
			noise.SetSeed(seed);
			noise.SetFractalType(FractalType.FBM);
			noise.SetFractalOctaves(5);
			noise.SetFractalLacunarity(2.0f);
			noise.SetFractalGain(0.5f);
		}

		public float GetNoise(float x, float y) => noise.GetPerlin(x, y);
		public float GetFractal(float x, float y) => noise.GetPerlinFractal(x, y);
	}
}