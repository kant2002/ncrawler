using System;

namespace NCrawler.Extensions
{
	public static class UrlExtensions
	{
		private static readonly string s_HttpScheme = "http://";
		private static readonly string s_HttpsScheme = "https://";

		public static string NormalizeUrl(this string url, string baseUrl)
		{
			if (url.IsNullOrEmpty())
			{
				return baseUrl;
			}

			if (url.IndexOf("..") != -1 ||
				url.StartsWith("/") ||
				!url.StartsWith(s_HttpScheme, StringComparison.OrdinalIgnoreCase) ||
				!url.StartsWith(s_HttpsScheme, StringComparison.OrdinalIgnoreCase))
			{
				url = new Uri(new Uri(baseUrl), url).AbsoluteUri;
			}

			if (Uri.IsWellFormedUriString(url, UriKind.Relative))
			{
				if (!baseUrl.IsNullOrEmpty())
				{
					var absoluteBaseUrl = new Uri(baseUrl, UriKind.Absolute);
					return new Uri(absoluteBaseUrl, url).ToString();
				}

				return new Uri(url, UriKind.Relative).ToString();
			}

			if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
			{
				// Only handle same schema as base uri
				var baseUri = new Uri(baseUrl);
				var uri = new Uri(url);
				bool schemaMatch;

				// Special case for http/https
				if (baseUri.Scheme.IsIn("http", "https"))
				{
					schemaMatch = string.Compare("http", uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare("https", uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0;
				}
				else
				{
					schemaMatch = string.Compare(baseUri.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0;
				}

				if (schemaMatch)
				{
					return new Uri(url, UriKind.Absolute).ToString();
				}
			}

			return null;
		}
	}
}