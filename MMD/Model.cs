using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace MMD {
	public class Factory {
		public static string LoadPmx(string currentModelUrl) {
			return new Varyl
		}
	}
	public class Model {

		/*
		 * OH MAH GOSH
		 * .PMX FILE CHECKING IN DISCORD?
		 * YES!
		 * AND EVEN SOME CHECKS THAT ARE NOT THERE BY DEFAULT!
		 */

		private readonly Exception _fileEmptyException = new Exception("File empty.");

		private HeaderStruct _header;
		public HeaderStruct Header => _header;

		public bool F(ModelItem select, ModelItem input) => (select & input) == select;
		public void Filter(ModelItem item) {
			var s = string.Empty;
			if (F(ModelItem.Headers, item)) {
				
			}
		}


		MemoryStream CreateErrorLog() {
			return new MemoryStream(Encoding.ASCII.GetBytes($"Varyl .PMX[{Header.Version.ToString(CultureInfo.InvariantCulture)}] file check"));
		}

		void ReadHeader(BinaryReader stream) {
			_header = new HeaderStruct(stream);
		}
		
		public Model(string path) {
			if (!File.Exists(path)) {
				throw new FileNotFoundException();
			}
			
			FileStream fileStream = new FileStream(path, FileMode.Open);

			if (fileStream.Length <= 0) {
				throw _fileEmptyException;
			}
			
			Load(fileStream);
		}

		public Model(Stream stream) {
			if (stream.Length <= 0) {
				throw _fileEmptyException;
			}
			Load(stream);
		}

		private void Load(Stream stream) {
			var reader = new BinaryReader(stream, Encoding.UTF8);
			ReadHeader(reader);
		}
	}
}