using System;

namespace NCrawler.LanguageDetection.Google
{
    [Serializable]
	public class LanguageDetector
	{
		#region Fields

		public LanguageDetectionResponseData responseData = new LanguageDetectionResponseData();
		public string responseDetails;
		public string responseStatus;

		#endregion
	}
}