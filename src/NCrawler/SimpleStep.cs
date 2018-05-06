using System;
using System.Threading.Tasks;
using NCrawler.Interfaces;

namespace NCrawler
{
    /// <summary>
    /// Implementation of pipeline steps which allow steps definition using simple functions
    /// </summary>
    public class SimpleStep : IPipelineStep
    {
        private Func<ICrawler, PropertyBag, Task> worker;

        public SimpleStep(Action worker)
            : this((crawler, properties) => worker())
        {
        }

        public SimpleStep(Action<ICrawler> worker)
            : this((crawler, properties) => worker(crawler))
        {
        }

        public SimpleStep(Action<ICrawler, PropertyBag> worker)
            : this((crawler, properties) =>
            {
                worker(crawler, properties);
                return Task.CompletedTask;
            })
        {
        }

        public SimpleStep(Func<Task> worker)
            : this((crawler, properties) => worker())
        {
        }

        public SimpleStep(Func<ICrawler, Task> worker)
            : this((crawler, properties) => worker(crawler))
        {
        }

        public SimpleStep(Func<ICrawler, PropertyBag, Task> worker) => this.worker = worker ?? throw new ArgumentNullException(nameof(worker));

        public Task ProcessAsync(ICrawler crawler, PropertyBag propertyBag)
        {
            return this.worker(crawler, propertyBag);
        }
    }
}
