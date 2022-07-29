using System;
using System.IO;
using System.Text;

namespace Varyl {
	public class Logger {
		public static void Create(string message, bool inout = true) {
			var now = DateTime.Now;
			var msg = message;
			msg = inout ? $"<< {msg}" : $">> {msg}";
			File.AppendAllText($"{(now.Year + now.Month + now.Day).ToString()}.log", $"{msg}\n");
		}
	}
}