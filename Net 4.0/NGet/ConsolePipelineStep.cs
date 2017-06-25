using System;
using System.Threading.Tasks;
using NCrawler;
using NCrawler.Interfaces;

namespace NGet
{
	public class ConsolePipelineStep : IPipelineStep
	{
		#region IPipelineStep Members

		public Task ProcessAsync(Crawler crawler, PropertyBag propertyBag)
		{
			Console.Out.WriteLine(propertyBag.Step.Uri);
            return Task.CompletedTask;
		}

		#endregion
	}
}