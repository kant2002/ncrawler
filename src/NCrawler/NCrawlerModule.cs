using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core.Lifetime;

using NCrawler.Extensions;
using NCrawler.Interfaces;
using NCrawler.Services;

namespace NCrawler
{
	public class NCrawlerModule : Autofac.Module
    {
		#region Constructors

		static NCrawlerModule()
		{
			Setup();
		}

		#endregion

		#region Instance Methods

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => new WebDownloaderV2()).As<IWebDownloader>().SingleInstance().ExternallyOwned();
			builder.Register(c => new InMemoryCrawlerHistoryService()).As<ICrawlerHistory>().InstancePerDependency();
			builder.Register(c => new InMemoryCrawlerQueueService()).As<ICrawlerQueue>().InstancePerDependency();
            builder.Register(c => new SystemTraceLoggerService()).As<ILog>().InstancePerDependency();
			builder.Register(c => new NativeTaskRunnerService()).As<ITaskRunner>().InstancePerDependency();
			builder.Register((c, p) => new RobotService(p.TypedAs<Uri>(), c.Resolve<IWebDownloader>())).As<IRobot>().InstancePerDependency();
			builder.Register((c, p) => new CrawlerRulesService(p.TypedAs<Crawler>(), c.Resolve<IRobot>(p), p.TypedAs<Uri>())).As<ICrawlerRules>().InstancePerDependency();
		}

		#endregion

		#region Class Properties

		public static IContainer Container { get; private set; }

		#endregion

		#region Class Methods

		public static void Register(Action<ContainerBuilder> registerCallback)
		{
			var builder = new ContainerBuilder();
			Container.ComponentRegistry.
				Registrations.
				Where(c => !c.Activator.LimitType.GetTypeInfo().IsAssignableFrom(typeof(LifetimeScope).GetTypeInfo())).
				ForEach(c => builder.RegisterComponent(c));
			registerCallback(builder);
			Container = builder.Build();
		}

		public static void Setup()
		{
			Setup(new NCrawlerModule());
		}

		public static void Setup(params Autofac.Module[] modules)
		{
			var builder = new ContainerBuilder();
			modules.ForEach(module => builder.RegisterModule(module));
			Container = builder.Build();
		}

		#endregion
	}
}