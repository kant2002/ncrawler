using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NCrawler.Interfaces;
using NCrawler.Test.Helpers;

namespace NCrawler.EntityFramework.Tests
{
    class TestEfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new FakeLoggerService()).As<ILog>().InstancePerDependency();
            builder.Register(c => new FakeDownloader()).As<IWebDownloader>().InstancePerDependency();
        }

        public static void SetupEfServicesStorage()
        {
            NCrawlerModule.Setup(new EfServicesModule(false), new TestEfModule());
        }
    }
}
