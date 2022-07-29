using System;
using System.IO;
using System.Threading.Tasks;

namespace Varyl.Containers {
	public class Item {
		// Product Id
		private short Id = 0;
		
		// Product Name
		private string Name;
		
		// Texture Path
		private string Texture = "default.png";

		private Item(short id, string name, string texture) {
			Id = id;
			Name = name;
			if(File.Exists(texture) && texture != null)
				Texture = texture;
		}

		public class Factory {
			public static Item Create(short id, string name, string texture) => new Item(id, name, texture);
			public static Item CreateEmpty() => new Item(0, string.Empty, null);
		}
	}
}