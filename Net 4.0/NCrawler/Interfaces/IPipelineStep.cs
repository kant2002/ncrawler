using System.Threading.Tasks;

namespace NCrawler.Interfaces
{
	public interface IPipelineStep
	{
        Task ProcessAsync(Crawler crawler, PropertyBag propertyBag);
	}
}