using System.IO;

namespace MMD {
	public struct HeaderStruct {
		private byte[] _signature;
		private float _version;
		private byte _globalsCount;
		private byte[] _globals;
		private string _jpName;
		private string _engName;
		private string _jpComment;
		private string _engComment;

		public HeaderStruct(BinaryReader reader) {
			_signature = reader.ReadBytes(4);
			_version = reader.ReadSingle();
			_globalsCount = reader.ReadByte();
			_globals = new byte[_globalsCount];
			for (var i = 0; i < _globals.Length; i++) {
				_globals[i] = reader.ReadByte();
			}

			_jpName = reader.ReadString();
			_engName = reader.ReadString();
			_jpComment = reader.ReadString();
			_engComment = reader.ReadString();
		}

		public byte[] Signature {
			get => _signature;
			set => _signature = value;
		}

		public float Version {
			get => _version;
			set => _version = value;
		}

		public byte GlobalsCount {
			get => _globalsCount;
			set => _globalsCount = value;
		}

		public byte[] Globals {
			get => _globals;
			set => _globals = value;
		}

		public string JpName {
			get => _jpName;
			set => _jpName = value;
		}

		public string EngName {
			get => _engName;
			set => _engName = value;
		}

		public string JpComment {
			get => _jpComment;
			set => _jpComment = value;
		}

		public string EngComment {
			get => _engComment;
			set => _engComment = value;
		}
	}
}