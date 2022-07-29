using System;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using Microsoft.Data.Sqlite;
using Timer = System.Timers.Timer;
#define DEBUG
#pragma SolidTAT


namespace VarylExtensions {
	public class Extensions {
		public static readonly SqliteConnection Connection = new SqliteConnection("Data Source=database.db3");
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
		public static async Task Open(DbConnection connection) {
			if (connection.State == ConnectionState.Open) {
				return;
			}

			await connection.OpenAsync();
		}

		public static void Close(DbConnection connection) {
			if (connection.State == ConnectionState.Closed) {
				return;
			}

			connection.Close();
		}
		

	}
}