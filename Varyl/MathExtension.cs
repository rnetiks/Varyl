using System;
using FoxMath;
using FoxMath.Point;

namespace Varyl {
	public static class MathExtension {
		public static double Distance(double x, double y) {
			return Math.Sqrt(x * x + y * y);
		}

		public static Point<long> Position(double angle, float distance) {
			angle *= Math.PI / 180;
			return new Point<long>((int) (distance * Math.Cos(angle)), (int) (distance * Math.Sin(angle)));
		}
	}
}