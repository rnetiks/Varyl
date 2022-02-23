using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Varyl.BattleSystem.Buffs;
using static Varyl.Commands;

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
		public InventorySlot[] Inventory = new InventorySlot[124];
		
		/// <summary>
		/// The buffs of the character.
		/// </summary>
		public Buff[] Buffs;
		
		/// <summary>
		/// The world position of the character;
		/// </summary>
		public Point Position;

		/// <summary>
		/// Creates a <see cref="Player"/> object from actual user data
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public static async Task<Player> Load(ulong character) {
			await Open(Connection);
			Player player;
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT id, baseHealth, baseMagic, baseDefense, baseStrength, baseAgility, health, magic, defense, strength, agility, level, xp, nick FROM characters WHERE id = @id";
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
					Name = reader.GetString(13)
				};
			}
			
			await Close(Connection);
			return await Task.FromResult(player);
		}

		public async void Update() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText =
					"UPDATE characters SET baseHealth = @baseHealth, baseMagic = @baseMagic, baseDefense = @baseDefense, baseStrength = @baseStrength, baseAgility = @baseAgility, health = @health, magic = @magic, defense = @defense, strength = @strength, agility = @agility, level = @level, xp = @xp WHERE id = @id";
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
				command.Parameters.AddWithValue("@id", _id);
				command.ExecuteNonQuery();
			}

			await Close(Connection);
		}
		
		/// <summary>
		/// Fills in the Position on the <see cref="Player"/> object.
		/// </summary>
		public async void GetPosition() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT PositionX, PositionY FROM characters WHERE id = @id";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				reader.Read();
				Position = new Point(reader.GetInt64(0), reader.GetInt64(1));
			}
			await Close(Connection);
		}
		/// <summary>
		/// Updates the position of the character within the database.
		/// </summary>
		public async void UpdatePosition() {
			await Open(Connection);
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "UPDATE characters SET PositionX = @xpos, PositionY = @ypos WHERE id = @id";
				command.Parameters.AddWithValue("@xpos", Position.X);
				command.Parameters.AddWithValue("@ypos", Position.Y);
				command.Parameters.AddWithValue("@id", _id);
				command.ExecuteNonQuery();
			}

			await Close(Connection);
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
				}
			}

			await Close(Connection);
		}
		
		/// <summary>
		/// Fills in the <see cref="Inventory"/> of the character.
		/// </summary>
		public async void FillInventory() {
			await Open(Connection);
			List<InventorySlot> slots;
			await using (var command = Connection.CreateCommand()) {
				command.CommandText = "SELECT item, quantity, type FROM inventory WHERE parent = @id LIMIT 124";
				command.Parameters.AddWithValue("@id", _id);
				await using var reader = command.ExecuteReader();
				slots = new List<InventorySlot>();
				while (reader.Read()) {
					InventorySlot slot = new InventorySlot {
						Item = reader.GetInt32(0), 
						Quantity = reader.GetInt32(1), 
						Type = (ItemType) reader.GetInt32(2)
					};
					slots.Add(slot);
				}
			}

			Inventory = slots.ToArray();
			await Close(Connection);
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

		private Player() { }


		/// <summary>
		/// Attacks an enemy and returns the damage dealt
		/// </summary>
		/// <param name="enemy">An <see cref="IEnemy"/> object</param>
		/// <returns>The amount of damage done to the enemy</returns>
		public async Task<int> Attack(IEnemy enemy) {
			return await enemy.damageEntity((int) ((Strength - enemy.Defense / 2) * RandomExtension.NextFloat(0.7f, 1.13f)));
		}

		public Task<int> DamageEntity(int damage) {
			Health = Math.Max(0, Health - damage);
			return Task.FromResult(damage);
		}
		
		public static bool AllDefeated(Player[] players) => players.All(player => player.Health <= 0);
	}

	[SuppressMessage("ReSharper", "NotAccessedField.Global")]
	public class InventorySlot {
		public int Item;
		public int Quantity;
		public ItemType Type;
	}
	
	[Flags]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public enum ItemType {
		Consumable = 1,
		Head = 2,
		Chest = 4,
		Leg = 8,
		Weapon = 16
	}
}