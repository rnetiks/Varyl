using System;
using System.Timers;

namespace VarylExtensions {
	public class Extensions {
		public static void Timer(int milliseconds, Action action) {
			var timer = new Timer(milliseconds) {
				AutoReset = false
			};
			timer.Elapsed += delegate
			{
				action.Invoke();
				timer.Stop();
				timer.Dispose();
			};
			timer.Start();
		}
	}
}