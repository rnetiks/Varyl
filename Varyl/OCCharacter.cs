using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Varyl {
	public class OcCharacter {
		public string Name, Nickname;
		public CharacterLevel Level;
		public Point Position;

		public OcCharacter(ulong userId) {
			var characterId = IsUsingCharacter(userId);
			if (characterId == null) return;
			Commands.Open(Commands.Connection).GetAwaiter().GetResult();

			using (var command = Commands.Connection.CreateCommand()) {
				command.CommandText = "SELECT character_name, nick, level, xp, xPosition, yPosition FROM characters WHERE id = @id";
				command.Parameters.AddWithValue("@id", characterId);
				using var reader = command.ExecuteReader();
				reader.Read();
				Name = reader.GetString(0);
				Nickname = reader.GetString(1);
				Level = new CharacterLevel(reader.GetInt32(2), (ulong)reader.GetValue(3));
				Position = new Point(reader.GetInt32(4), reader.GetInt32(5));
			}
			
			Commands.Close(Commands.Connection).GetAwaiter().GetResult();
		}

		public static long? IsUsingCharacter(ulong id)
		{
			if (Commands.OcCharacter.ContainsKey(id))
				return Commands.OcCharacter[id].Id;

			return null;
		}
	}
}