using Autofac;
using NCrawler.Interfaces;
using NCrawler.Test.Helpers;

namespace NCrawler.EsentServices.Tests
{
    class TestEsentModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new FakeLoggerService()).As<ILog>().InstancePerDependency();
            builder.Register(c => new FakeDownloader()).As<IWebDownloader>().InstancePerDependency();
        }

        public static void SetupEfServicesStorage()
        {
            NCrawlerModule.Setup(new EsentServicesModule(false), new TestEsentModule());
        }
    }
}
