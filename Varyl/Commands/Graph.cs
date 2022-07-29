using System;

namespace Varyl {
	public class Graph {
		public bool[] noiseCircleMask(float width, float height) {
			if(width < 1) throw new Exception("Width must be bigger than 1.");
			if(height < 1) throw new Exception("Height must be bigger than 1.");
			width = width * 2 + 1;
			float midWidth = Math.Max(1, width / 2 + 0.5f);

			height = height * 2 + 1;
			float midHeight = Math.Max(1, height / 2 + 0.5f);
			return new bool[(int) (width*height)];
		}
	}
}