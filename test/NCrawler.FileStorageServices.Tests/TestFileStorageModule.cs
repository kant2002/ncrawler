using System.IO;
using Autofac;
using NCrawler.Interfaces;
using NCrawler.Test.Helpers;

namespace NCrawler.FileStorageServices.Tests
{
    class TestFileStorageModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new FakeLoggerService()).As<ILog>().InstancePerDependency();
            builder.Register(c => new FakeDownloader()).As<IWebDownloader>().InstancePerDependency();
        }

        public static void SetupFileServicesStorage()
        {
            NCrawlerModule.Setup(new FileStorageModule(Directory.GetCurrentDirectory(), false), new TestFileStorageModule());
        }
    }
}
