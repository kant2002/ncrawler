using System.IO;
using System.Runtime.Serialization;

namespace NCrawler.IsolatedStorageServices
{
	internal static class StreamExtensions
	{
		public static TResult FromBinary<TResult>(this Stream s) where TResult : class, new()
		{
			DataContractSerializer dc = new DataContractSerializer(typeof(TResult));
			return (TResult)dc.ReadObject(s);
		}
	}
}
