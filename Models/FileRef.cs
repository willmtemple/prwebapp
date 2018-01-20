using System;
using System.IO;

namespace PeerReviewWeb.Models
{
	public class FileRef
	{
		public Guid ID { get; set; }
		public ApplicationUser Owner { get; set; }

		public string Name { get; set; }

		public string ContentType { get; set; }

		public FileStream Open(string basePath, FileMode mode)
		{
			var ids = ID.ToString();
			var _path = Path.Combine(basePath, ids.Substring(0, 2), ids);
			var _dir = Path.GetDirectoryName(_path);

			if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);

			return File.Open(_path, mode);
		}
	}
}