using System.Threading.Tasks;

namespace NCrawler.Interfaces
{
	public interface IPipelineStep
	{
        Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag);
	}
}