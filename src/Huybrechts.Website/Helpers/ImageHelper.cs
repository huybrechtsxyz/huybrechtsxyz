using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Security;

namespace Huybrechts.Website.Helpers
{
	public class ImageHelper
	{
		private string _rootPath = "";

		public ImageHelper(string rootPath)
		{
			_rootPath = rootPath;
		}

		public string UnknownUserType => "image/png";

		public string UnknownUserData => ConvertToBase64(_rootPath+ "/img/unknownuser.png", "image/png");

		public string UnknownUserData32 => ConvertToBase64(_rootPath + "/img/unknownuser32.png", "image/png");

		public string UnknownUserData64 => ConvertToBase64(_rootPath + "/img/unknownuser64.png", "image/png");

		public string ConvertToBase64(string fileName, string imageType)
		{
			return string.Format("data:" + imageType + ";base64,{0}", Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName)));
		}

		public string ConvertToBase64(MemoryStream stream, string imageType)
		{
			return string.Format("data:" + imageType + ";base64,{0}", Convert.ToBase64String(stream.ToArray()));	
		}

		public MemoryStream GetThumbnailImage(MemoryStream stream, int width, int height)
		{
			return stream;
        }
	}
}
