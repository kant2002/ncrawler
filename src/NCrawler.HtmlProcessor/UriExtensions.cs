using System;
using System.Collections.Generic;
using System.Text;

namespace NCrawler.HtmlProcessor
{
    /// <summary>
    /// Extension to the URI class.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Equivalent of Uri.GetLeftPart(UriPartial.Path
        /// </summary>
        /// <param name="uri">Uri for which get left path</param>
        /// <returns>Left part of path.</returns>
        public static string GetLeftPath(this Uri uri)
        {
            // return uri.GetLeftPart(UriPartial.Path);
            return uri.AbsolutePath;
        }
    }
}
