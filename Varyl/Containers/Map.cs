using System;
using System.Collections.Generic;
using Emzi0767;

namespace Varyl.Containers {
	public class Map {
		public Map(long x, long y) {
			_noise = new FastNoiseLite();
		}
		SecureRandom _random = new SecureRandom();
		private FastNoiseLite _noise;
		public void GetBiome() {
			BiomeMap = new List<float>();
			_noise.SetSeed(476477079);
			BiomeMap = FillNoise();
		}

		public void GetDeadzone() {
			DeadzoneMap = new List<float>();
			_noise.SetSeed(1857109943);
			DeadzoneMap = FillNoise();
		}

		public void Height() {
			HeightMap = new List<float>();
			_noise.SetSeed(1868756635);
			HeightMap = FillNoise();
		}

		public void Moisture() {
			MoistureMap = new List<float>();
			_noise.SetSeed(610523660);
			MoistureMap = FillNoise();

		}

		private List<float> FillNoise() {
			var s = new List<float>();
			for (int x = 0; x < Mapsize; x++) {
				for (int y = 0; y < Mapsize; y++) {
					s.Add(_noise.GetNoise(x, y));
				}
			}

			return s;
		}

		private const int Mapsize = 25;
		private int X, Y;
		public List<float> DeadzoneMap { get; private set; }	= new List<float>();
		public List<float> HeightMap { get; private set; }		= new List<float>();
		public List<float> BiomeMap { get; private set; } 		= new List<float>();
		public List<float> MoistureMap { get; private set; } 	= new List<float>();
	}
}