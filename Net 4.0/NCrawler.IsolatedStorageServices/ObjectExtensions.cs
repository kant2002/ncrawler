using System.IO;
using System.Runtime.Serialization;

namespace NCrawler.IsolatedStorageServices
{
	internal static class ObjectExtensions
	{
		public static byte[] ToBinary<T>(this T o) where T : class, new()
		{
			var dc = new DataContractSerializer(typeof(T));
			using (var ms = new MemoryStream())
			{
				dc.WriteObject(ms, o);
				return ms.ToArray();
			}
		}

		public static T FromBinary<T>(this byte[] byteArray) where T : class, new()
		{
			var dc = new DataContractSerializer(typeof(T));
			using (var ms = new MemoryStream())
			{
				ms.Write(byteArray, 0, byteArray.Length);
				ms.Seek(0, SeekOrigin.Begin);
				return dc.ReadObject(ms) as T;
			}
		}
	}
}
