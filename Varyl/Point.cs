namespace Varyl {
	public struct Point<T> {
		public T X;
		public T Y;

		public Point(T x, T y) {
			X = x;
			Y = y;
		}
		
		public override string ToString() {
			return $"{X}, {Y}";
		}
	}
}