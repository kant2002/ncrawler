using System;
using System.Threading.Tasks;
using NCrawler;
using NCrawler.Interfaces;

namespace NGet
{
	public class ConsolePipelineStep : IPipelineStep
	{
		public async Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag)
		{
			await Console.Out.WriteLineAsync(propertyBag.Step.Uri.ToString()).ConfigureAwait(false);
		}
	}
}