using System;

using Autofac;

using NCrawler.Interfaces;

namespace NCrawler.IsolatedStorageServices
{
	public class IsolatedStorageModule : NCrawlerModule
	{
		#region Readonly & Static Fields

		private readonly bool m_Resume;

		#endregion

		#region Constructors

		public IsolatedStorageModule(bool resume)
		{
            this.m_Resume = resume;
		}

		#endregion

		#region Instance Methods

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.Register((c, p) =>
				{
					var crawlStart = p.TypedAs<Uri>();
					return new IsolatedStorageCrawlerHistoryService(crawlStart, this.m_Resume);
				}).As<ICrawlerHistory>().InstancePerDependency();

			builder.Register((c, p) =>
				{
					var crawlStart = p.TypedAs<Uri>();
					return new IsolatedStorageCrawlerQueueService(crawlStart, this.m_Resume);
				}).As<ICrawlerQueue>().InstancePerDependency();
		}

		#endregion

		#region Class Methods

		public static void Setup(bool resume)
		{
			Setup(new IsolatedStorageModule(resume));
		}

		#endregion
	}
}