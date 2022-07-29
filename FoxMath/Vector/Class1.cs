using System;
using System.Diagnostics;
using System.IO;

namespace FoxMath.Vector {
	public class Vector2 {
		public float X { get; }
		public float Y { get; }

		public Vector2(float X, float Y) {
			this.X = X;
			this.Y = Y;
		}

		public Vector2(BinaryReader reader) {
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
		}
		
		public void Save(BinaryWriter writer) {
			writer.Write(X);
			writer.Write(Y);
		}
		
		public static Vector2 Vector(float x, float y) {
			return new Vector2(x, y);
		}
		
	}

	public class Vector3 {
		public float X;
		public float Y;
		public float Z;

		public Vector3(float X, float Y, float Z) {
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		static Vector3 Vector(float x, float y, float z) {
			return new Vector3(x, y, z);	
		}

		public Vector3(BinaryReader reader) {
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
		}

		public void Save(BinaryWriter writer) {
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
			var c = new Vector3(3, 3, 3) / new Vector3(2, 2, 2);
		}

		public static Vector3 operator +(Vector3 v) => v;
		public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);
		
		public static Vector3 operator +(Vector3 v, Vector3 c) {
			return new Vector3(v.X + c.X, v.Y + c.Y, v.Z + c.Z);
		}

		public static Vector3 operator -(Vector3 x, Vector3 y) {
			return new Vector3(x.X - y.X, x.Y - y.Y, x.Z - y.Z);
		}
		
		public static Vector3 operator *(Vector3 x, Vector3 y) {
			return new Vector3(x.X * y.X, x.Y * y.Y, x.Z * y.Z);
		}
		
		public static Vector3 operator /(Vector3 x, Vector3 y) {
			if (y.X == 0 || y.Y == 0 || y.Z == 0) {
				throw new DivideByZeroException();
			}
			return new Vector3(x.X / y.X, x.Y / y.Y, x.Z / y.Z);
		}
		
		public static Vector3 operator %(Vector3 x, Vector3 y) {
			return new Vector3(x.X % y.X, x.Y % y.Y, x.Z % y.Z);
		}

		public static bool operator ==( Vector3 x, Vector3 y) {
			return y != null && x != null && x.X == y.X && x.Y == y.Y && x.Z == y.Z;
		}
		
		public static bool operator !=(Vector3 x, Vector3 y) {
			return Equals(x, y);
		}
	}
}