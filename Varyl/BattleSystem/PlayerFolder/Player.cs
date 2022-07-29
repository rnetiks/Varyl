using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Varyl.BattleSystem.Buffs;
using Varyl.Containers;
using FoxMath.Point;
using static VarylExtensions.Extensions;

// ReSharper disable once CheckNamespace
namespace Varyl.BattleSystem {
	public partial class Player {
		/// <summary>
		/// The id of the character, only used by the <see cref="Player"/> class.
		/// </summary>
		private long _id;
		
		/// <summary>
		/// The level of the character.
		/// </summary>
		public long Level { get; private set; }
		
		/// <summary>
		/// The experience points of the character.
		/// </summary>
		public long Experience { get; private set; }
		
		/// <summary>
		/// The name of the character.
		/// </summary>
		public string Name { get; set; } 
		
		/// <summary>
		/// The base stat values of the character.
		/// </summary>
		public long BaseHealth, BaseMagic, BaseDefense, BaseStrength, BaseAgility;
		
		/// <summary>
		/// The current stat values of the character.
		/// </summary>
		public long Health, Magic, Defense, Strength, Agility;
		
		/// <summary>
		/// The inventory of the character.
		/// </summary>
		public GameItem[] Inventory = new GameItem[124];
		
		/// <summary>
		/// The buffs of the character.
		/// </summary>
		public Buff[] Buffs;
		
		/// <summary>
		/// The world position of the character;
		/// </summary>
		public Point<long> Position;

		/// <summary>
		/// Link to the profile image of the character
		/// </summary>
		public string ProfileUri;
		
		public string replacePvPString(string formula, Player player_one, Player player_two) {
			string __replacot(Player p, string formula) {
				formula = formula.Replace("player2.name", p.Name);
				formula = formula.Replace("player2.level", p.Level.ToString());
				formula = formula.Replace("player2.exp", p.Experience.ToString());
			
				formula = formula.Replace("player2.bcon", p.BaseHealth.ToString());
				formula = formula.Replace("player2.bmag", p.BaseMagic.ToString());
				formula = formula.Replace("player2.bdef", p.BaseDefense.ToString());
				formula = formula.Replace("player2.bstr", p.BaseStrength.ToString());
				formula = formula.Replace("player2.bagi", p.BaseAgility.ToString());
			
				formula = formula.Replace("player2.con", p.Health.ToString());
				formula = formula.Replace("player2.mag", p.Magic.ToString());
				formula = formula.Replace("player2.def", p.Defense.ToString());
				formula = formula.Replace("player2.str", p.Strength.ToString());
				formula = formula.Replace("player2.agi", p.Agility.ToString());

				return formula;
			}
			formula = __replacot(player_one, formula);
			formula = __replacot(player_two, formula);
			
			return formula;
		}
		public string LevelMessage(short health, short magic, short defense, short strength, short agility) {
			var message = "";
			message += health > 0
				? $"Health: {BaseHealth.ToString()} → {(BaseHealth + health).ToString()}"
				: $"Health: {BaseHealth.ToString()}";
			message += magic > 0
				? $"Magic: {BaseMagic.ToString()} → {(BaseMagic + magic).ToString()}"
				: $"Magic: {BaseMagic.ToString()}";
			message += defense > 0
				? $"Defense: {BaseDefense.ToString()} → {(BaseDefense + defense).ToString()}"
				: $"Defense: {BaseDefense.ToString()}";
			message += strength > 0
				? $"Strength: {BaseStrength.ToString()} → {(BaseStrength + strength).ToString()}"
				: $"Strength: {BaseStrength.ToString()}";
			message += agility > 0
				? $"Agility: {BaseAgility.ToString()} → {(BaseAgility + agility).ToString()}"
				: $"Agility: {BaseAgility.ToString()}";

			return message;
		}
		
		/// <summary>
		/// Creates a <see cref="Player"/> object from actual user data
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public static async Task<Player> Load(long character) {
			await Open(Connection);
			Player player;
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT id, baseHealth, baseMagic, baseDefense, baseStrength, baseAgility, health, magic, defense, strength, agility, level, xp, nick, profile_uri FROM Characters WHERE id = @id";
				command.Parameters.AddWithValue("@id", character);
				await using var reader = command.ExecuteReader();
				reader.Read();
				player = new Player {
					_id = reader.GetInt64(0),
					BaseHealth = reader.GetInt64(1),
					BaseMagic = reader.GetInt64(2),
					BaseDefense = reader.GetInt64(3),
					BaseStrength = reader.GetInt64(4),
					BaseAgility = reader.GetInt64(5),
					Health = reader.GetInt64(6),
					Magic = reader.GetInt64(7),
					Defense = reader.GetInt64(8),
					Strength = reader.GetInt64(9),
					Agility = reader.GetInt64(10),
					Level = reader.GetInt64(11),
					Experience = reader.GetInt64(12),
					Name = reader.GetString(13),
					ProfileUri = reader.GetString(14)
				};
			}
			
			Close(Connection);
			return await Task.FromResult(player);
		}
		
		private Player() {}
		
		/// <summary>
		/// Updates the general information of the player
		/// </summary>
		public async void Update() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText =
					"UPDATE Characters SET baseHealth = @baseHealth, baseMagic = @baseMagic, baseDefense = @baseDefense, baseStrength = @baseStrength, baseAgility = @baseAgility, health = @health, magic = @magic, defense = @defense, strength = @strength, agility = @agility, level = @level, xp = @xp, profile_uri = @profileuri WHERE id = @id";
				command.Parameters.AddWithValue("@baseHealth", BaseHealth);
				command.Parameters.AddWithValue("@baseMagic", BaseMagic);
				command.Parameters.AddWithValue("@baseDefense", BaseDefense);
				command.Parameters.AddWithValue("@baseStrength", BaseStrength);
				command.Parameters.AddWithValue("@baseAgility", BaseAgility);
				command.Parameters.AddWithValue("@health", Health);
				command.Parameters.AddWithValue("@magic", Magic);
				command.Parameters.AddWithValue("@defense", Defense);
				command.Parameters.AddWithValue("@strength", Strength);
				command.Parameters.AddWithValue("@agility", Agility);
				command.Parameters.AddWithValue("@level", Level);
				command.Parameters.AddWithValue("@xp", Experience);
				command.Parameters.AddWithValue("@profileuri", ProfileUri);
				command.Parameters.AddWithValue("@id", _id);
				command.ExecuteNonQuery();
			}

			Close(Connection);
		}
		
		/// <summary>
		/// Fills in the Position on the <see cref="Player"/> object.
		/// </summary>
		public async void LoadPosition() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT PositionX, PositionY FROM Characters WHERE id = @id";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				reader.Read();
				Position = new Point<long>(reader.GetInt64(0), reader.GetInt64(1));
			}
			Close(Connection);
		}
		/// <summary>
		/// Updates the position of the character within the database.
		/// </summary>
		public async void SavePosition() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "UPDATE Characters SET PositionX = @xpos, PositionY = @ypos WHERE id = @id";
				command.Parameters.AddWithValue("@xpos", Position.X);
				command.Parameters.AddWithValue("@ypos", Position.Y);
				command.Parameters.AddWithValue("@id", _id);
				command.ExecuteNonQuery();
			}

			Close(Connection);
		}
		
		/// <summary>
		/// Fills in the <see cref="Buffs"/> a character has.
		/// </summary>
		public async void FillBuffs() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT buffContent FROM Buffs WHERE Parent = @id LIMIT 4";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				while (reader.Read()) {
					// TODO fill the Buff implementation out
					// Maybe json?
				}
			}

			Close(Connection);
		}
		
		/// <summary>
		/// Fills in the <see cref="Inventory"/> of the character.
		/// </summary>
		public async void FillInventory() {
			await Open(Connection);
			List<GameItem> slots;
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT Item, Quantity, Type FROM Inventory WHERE Parent = @id LIMIT 124";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				slots = new List<GameItem>(); 
				while (reader.Read()) {
					var slot = ItemFactory.CreateGameItem(reader.GetInt32(0));
					if (slot != null) slots.Add(slot);
				}
			}

			Inventory = slots.ToArray();
			Close(Connection);
		}
		
		/// <summary>
		/// Creates a manual NPC which will be seen as a Player
		/// </summary>
		/// <param name="baseHealth">The maximum amount of Health the character has.</param>
		/// <param name="baseMagic">The maximum amount of Magic the character has.</param>
		/// <param name="baseDefense">The maximum amount of Defense the character has.</param>
		/// <param name="baseStrength">The maximum amount of Strength the character has.</param>
		/// <param name="baseAgility">The maximum amount of Agility the character has.</param>
		/// <param name="level">The level of the character, the higher the level the more Experience it drops.</param>
		/// <param name="name">The name of the character.</param>
		public Player(long baseHealth, long baseMagic, long baseDefense, long baseStrength, long baseAgility, long level, string name) {
			BaseHealth = baseHealth;
			BaseMagic = baseMagic;
			BaseDefense = baseDefense;
			BaseStrength = baseStrength;
			BaseAgility = baseAgility;
			Level = level;
			Name = name;
		}


		/// <summary>
		/// Attacks an enemy and returns the damage dealt
		/// </summary>
		/// <param name="enemy">An <see cref="IEnemy"/> object</param>
		/// <returns>The amount of damage done to the enemy</returns>
		public async Task<int> Attack(IEnemy enemy) {
			return await enemy.damageEntity((int) ((Strength - enemy.Defense / 2) * RandomExtension.NextFloat(0.7f, 1.13f)));
		}

		/// <summary>
		/// Damages the player object
		/// </summary>
		/// <param name="damage"></param>
		/// <returns></returns>
		public Task<int> DamageEntity(int damage) {
			Health = Math.Max(0, Health - damage);
			return Task.FromResult(damage);
		}
		
		/// <summary>
		/// Checks if a team of players is defeated
		/// </summary>
		/// <param name="players"></param>
		/// <returns></returns>
		public static bool AllDefeated(Player[] players) => players.All(player => player.Health <= 0);

		// ReSharper disable once CyclomaticComplexity
		private static async Task<string> ConvertPointsToString(int points) {
			var t = points switch {
				_ when points > -500 && points < 1000 => "Neutral",
				_ when points < -500 => "Bad",
				_ when points < -1000 => "Criminal",
				_ when points < -2000 => "Wanted",
				_ when points < -4000 => "Monster",
				_ when points < -5000 => "Kill on sight",
				_ when points < -10000 => "International Enemy",
				_ when points >= 768000 => "n̷o̸ ̸i̵n̷f̶o̵r̴m̷a̴t̷i̵o̸n̸",
				_ when points > 384000 => "Fox's protection",
				_ when points > 192000 => "Undefined",
				_ when points > 96000 => "Fox Inc.",
				_ when points > 48000 => "Special grade hero",
				_ when points > 24000 => "Controller",
				_ when points > 12000 => "Special individual",
				_ when points > 6000 => "Hero",
				_ when points > 3000 => "Friendly",
				_ when points >= 1000 => "Good",
				_ => "Neutral"
			};
			return await Task.FromResult(t);
		}
		
		/// <summary>
		/// Gets the rank of what others think
		/// </summary>
		/// <returns></returns>
		public async Task<string> GetSocialOpinion() {
			await Open(Connection);
			int points;
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT soPoints FROM characters WHERE id = @id";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				reader.Read();
				points = reader.GetInt32(0);
			}

			Close(Connection);

			return await ConvertPointsToString(points);
		}
	}

	[Flags]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum ItemType {
		Consumable = 1, //Might be lewd
		Head = 2,
		Chest = 4,
		Leg = 8,
		Weapon = 16
	}
}