using System;

namespace Varyl {
	public static class MathExtension {
		public static double Distance(double x, double y) {
			return Math.Sqrt(x * x + y * y);
		}

		public static Point Position(double angle, float distance) {
			angle *= Math.PI / 180;
			return new Point((int) (distance * Math.Cos(angle)), (int) (distance * Math.Sin(angle)));
		}
	}
}