using System.IO;
using System.Reflection;

using Autofac;
using NCrawler.Interfaces;

using Module = Autofac.Module;

namespace NCrawler.Test.Helpers
{
	public class TestModule : Module
	{
		#region Instance Methods

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => new FakeLoggerService()).As<ILog>().InstancePerDependency();
			builder.Register(c => new FakeDownloader()).As<IWebDownloader>().InstancePerDependency();
		}

		#endregion

		#region Class Methods

		public static void SetupInMemoryStorage()
		{
			NCrawlerModule.Setup(new NCrawlerModule(), new TestModule());
		}

		public static void SetupSqLiteStorage()
		{
			//NCrawlerModule.Setup(new SqLiteServicesModule(false), new TestModule());
		}

		#endregion
	}
}