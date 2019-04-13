using System;
using System.IO;

using NGet.Utils;

namespace NGet
{
	public class Arguments
	{
		#region Fields

		public TextWriter DefaultOutput = Console.Out;
		public OptionSet m_LoggingAndInputFileArgumentOptionSet;
		public OptionSet m_StartupArgumentOptionSet;

		#endregion

		#region Constructors

		public Arguments()
		{
            this.m_StartupArgumentOptionSet = new OptionSet
				{
					{"V|version", "display the version of Wget and exit.", v => this.ShowVersionInformation()},
					{"h|help|?", "print this help.", v => this.ShowUsageFull()},
					{"b|background", "go to background after startup.", v => this.SetBackGroundFlag()},
				};
            this.m_LoggingAndInputFileArgumentOptionSet = new OptionSet
				{
					{"o|output-file=", "log messages to {FILE}.", v => this.SetOutputOutputToFileOption()},
					{"a|append-output=", "append messages to {FILE}.", v => this.SetOutputAppendToFileOption()},
					{"d|debug", "print lots of debugging information.", v => this.SetShowDebugInformation()},
					{"q|quiet", "quiet (no output).", v => this.SetQuietOption()},
					{"v|verbose", "be verbose (this is the default).", v => this.SetVerboseOption()},
					{"nv|no-verbose", "turn off verboseness, without being quiet.", v => this.SetNoVerboseOption()},
					{"i|input-file=", "download URLs found in {FILE}.", file => this.SetInputFileOption(file)},
					{"F|force-html", "treat input file as HTML.", v => this.SetInputFileIsHtmlOption()},
					{"B|base=", "prepends {URL} to relative links in -F -i file.", v => this.SetPrependUrlOption()},
				};
		}

		#endregion

		#region Instance Properties

		public bool Verbose { get; private set; }

		#endregion

		#region Instance Methods

		public void ShowUsageFull()
		{
            this.DefaultOutput.WriteLine("nget 1.0.0.0, a non-interactive network retriever.");
            this.DefaultOutput.WriteLine("Usage: nget [OPTION]... [URL]...");
            this.DefaultOutput.WriteLine();
            this.DefaultOutput.WriteLine("Mandatory arguments to long options are mandatory for short options too.");
            this.DefaultOutput.WriteLine();
            this.DefaultOutput.WriteLine("Startup:");
            this.m_StartupArgumentOptionSet.WriteOptionDescriptions(this.DefaultOutput);
            this.DefaultOutput.WriteLine();

            this.DefaultOutput.WriteLine("Logging and input file:");
            this.m_LoggingAndInputFileArgumentOptionSet.WriteOptionDescriptions(this.DefaultOutput);
		}

		public void ShowUsageShort()
		{
            this.DefaultOutput.WriteLine("nget: missing URL");
            this.DefaultOutput.WriteLine("Usage: nget [OPTION]... [URL]...");
            this.DefaultOutput.WriteLine();
            this.DefaultOutput.WriteLine("Try 'nget --help' for more options.");
		}

		private void SetBackGroundFlag()
		{
		}

		private void SetInputFileIsHtmlOption()
		{
		}

		private void SetInputFileOption(string value)
		{
		}

		private void SetNoVerboseOption()
		{
		}

		private void SetOutputAppendToFileOption()
		{
		}

		private void SetOutputOutputToFileOption()
		{
		}

		private void SetPrependUrlOption()
		{
		}

		private void SetQuietOption()
		{
		}

		private void SetShowDebugInformation()
		{
		}

		private void SetVerboseOption()
		{
            this.Verbose = true;
		}

		private void ShowVersionInformation()
		{
		}

		#endregion
	}
}