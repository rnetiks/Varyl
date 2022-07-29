using System;
using System.Collections.Generic;
using System.Linq;

namespace Varyl.Containers {
	public static class ItemFactory {
		private static readonly List<GameItem> StandardItems;

		static ItemFactory() {

		}

		public static GameItem CreateGameItem(int itemTypeId) {
			if (itemTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(itemTypeId));
			var standardItem = StandardItems.FirstOrDefault(item => item.ItemTypeId == itemTypeId);
			return standardItem?.Clone();
		}
	}
}