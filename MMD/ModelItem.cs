using System;

namespace MMD {
	[Flags]
	public enum ModelItem {
		Headers = 1,
		Vertices = 2,
		Surfaces = 4,
		Textures = 8,
		Materials = 16,
		Bones = 32
	}
}