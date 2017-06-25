using System;

namespace NCrawler.LanguageDetection.Google
{
    [Serializable]
	public class LanguageDetectionResponseData
	{
		#region Fields

		public string confidence;
		public string isReliable;
		public string language;

		#endregion
	}
}