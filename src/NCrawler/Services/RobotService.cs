using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NCrawler.Extensions;
using NCrawler.Interfaces;

namespace NCrawler.Services
{
	/// <summary>
	/// 	Taken from Searcharoo 7, and modifed
	/// </summary>
	public class RobotService : IRobot
	{
		#region Readonly & Static Fields

		private readonly Uri m_StartPageUri;
		private readonly IWebDownloader m_WebDownloader;

		#endregion

		#region Fields

		private string[] m_DenyUrls = new string[0];

		private bool m_Initialized;

		#endregion

		#region Constructors

		public RobotService(Uri startPageUri, IWebDownloader webDownloader)
		{
            this.m_StartPageUri = startPageUri;
            this.m_WebDownloader = webDownloader;
		}

		#endregion

		#region Instance Methods

		/// <summary>
		/// 	Does the parsed robots.txt file allow this Uri to be spidered for this user-agent?
		/// </summary>
		/// <remarks>
		/// 	This method does all its "matching" in uppercase - it expects the _DenyUrl 
		/// 	elements to be ToUpper() and it calls ToUpper on the passed-in Uri...
		/// </remarks>
		public async Task<bool> Allowed(Uri uri)
		{
			if (!this.m_Initialized)
			{
				await Initialize();
                this.m_Initialized = true;
			}

			if (this.m_DenyUrls.Length == 0)
			{
				return true;
			}

			var url = uri.AbsolutePath.ToUpperInvariant();
			if (this.m_DenyUrls.
				Where(denyUrlFragment => url.Length >= denyUrlFragment.Length).
				Any(denyUrlFragment => url.Substring(0, denyUrlFragment.Length) == denyUrlFragment))
			{
				return false;
			}

			return !url.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase);
		}

		private async Task Initialize()
		{
			try
			{
				var robotsUri = new Uri("http://{0}/robots.txt".FormatWith(this.m_StartPageUri.Host));
				var robots = await this.m_WebDownloader.DownloadAsync(new CrawlStep(robotsUri, 0), null, DownloadMethod.GET);

				if (robots == null || robots.StatusCode != HttpStatusCode.OK)
				{
					return;
				}

				string fileContents;
				using (var stream = new StreamReader(robots.GetResponse(), Encoding.ASCII))
				{
					fileContents = stream.ReadToEnd();
				}

				var fileLines = fileContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

				var rulesApply = false;
				var rules = new List<string>();
				foreach (var line in fileLines)
				{
					var ri = new RobotInstruction(line);
					if (!ri.Instruction.IsNullOrEmpty())
					{
						switch (ri.Instruction[0])
						{
							case '#': //then comment - ignore
								break;
							case 'u': // User-Agent
								if ((ri.UrlOrAgent.IndexOf("*") >= 0) || (ri.UrlOrAgent.IndexOf(this.m_WebDownloader.UserAgent) >= 0))
								{
									// these rules apply
									rulesApply = true;
								}
								else
								{
									rulesApply = false;
								}
								break;
							case 'd': // Disallow
								if (rulesApply)
								{
									rules.Add(ri.UrlOrAgent.ToUpperInvariant());
								}
								break;
							case 'a': // Allow
								break;
							default:
								// empty/unknown/error
								break;
						}
					}
				}

                this.m_DenyUrls = rules.ToArray();
			}
			catch (Exception)
			{
			}
		}

		#endregion

		#region IRobot Members

		public async Task<bool> IsAllowed(string userAgent, Uri uri)
		{
			return await Allowed(uri);
		}

		#endregion

		#region Nested type: RobotInstruction

		/// <summary>
		/// 	Use this class to read/parse the robots.txt file
		/// </summary>
		/// <remarks>
		/// 	Types of data coming into this class
		/// 	User-agent: * ==> _Instruction='User-agent', _Url='*'
		/// 	Disallow: /cgi-bin/ ==> _Instruction='Disallow', _Url='/cgi-bin/'
		/// 	Disallow: /tmp/ ==> _Instruction='Disallow', _Url='/tmp/'
		/// 	Disallow: /~joe/ ==> _Instruction='Disallow', _Url='/~joe/'
		/// </remarks>
		private class RobotInstruction
		{
			#region Constructors

			/// <summary>
			/// 	Constructor requires a line, hopefully in the format [instuction]:[url]
			/// </summary>
			public RobotInstruction(string line)
			{
                this.UrlOrAgent = string.Empty;
				var instructionLine = line;
				var commentPosition = instructionLine.IndexOf('#');
				if (commentPosition == 0)
				{
                    this.Instruction = "#";
				}

				if (commentPosition >= 0)
				{
					// comment somewhere on the line, trim it off
					instructionLine = instructionLine.Substring(0, commentPosition);
				}

				if (instructionLine.Length > 0)
				{
					// wasn't just a comment line (which should have been filtered out before this anyway
					var lineArray = instructionLine.Split(':');
                    this.Instruction = lineArray[0].Trim().ToUpperInvariant();
					if (lineArray.Length > 1)
					{
                        this.UrlOrAgent = lineArray[1].Trim();
					}
				}
			}

			#endregion

			#region Instance Properties

			/// <summary>
			/// 	Upper-case part of robots.txt line, before the colon (:)
			/// </summary>
			public string Instruction { get; private set; }

			/// <summary>
			/// 	Upper-case part of robots.txt line, after the colon (:)
			/// </summary>
			public string UrlOrAgent { get; private set; }

			#endregion
		}

		#endregion
	}
}