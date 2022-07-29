using Varyl.BattleSystem;

namespace Varyl {
	public class GameItem {
		public int ItemTypeId { get; set; }
		public string Name { get; set; }
		public int Price { get; set; }
		public ItemType Type { get; }
		
		public bool IsConsumable { get; }
		public bool IsEquip { get; }

		public GameItem(int itemTypeId, string name, int price, ItemType type) {
			ItemTypeId = itemTypeId;
			Name = name;
			Price = price;
			Type = type;
		}

		public GameItem Clone() {
			return new GameItem(ItemTypeId, Name, Price, ItemType.Weapon);
		}

		public void Action() {
			
		}
	}
}