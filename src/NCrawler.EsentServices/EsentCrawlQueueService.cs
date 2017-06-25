using System;
using System.IO;
using System.Text;
using Microsoft.Isam.Esent.Interop;

using NCrawler.EsentServices.Utils;
using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.EsentServices
{
	public class EsentCrawlQueueService : CrawlerQueueServiceBase
	{
		#region Readonly & Static Fields

		private readonly string m_DatabaseFileName;
		private readonly EsentInstance m_EsentInstance;

		#endregion

		#region Fields

		private JET_COLUMNDEF dataColumn;
		private JET_COLUMNDEF queueCountColumn;

        #endregion

        #region Constructors


        public EsentCrawlQueueService(string basePath, Uri baseUri, bool resume)
		{
            this.m_DatabaseFileName = Path.GetFullPath(
                Path.Combine(basePath, "NCrawlQueue{0}\\Queue.edb".FormatWith(baseUri.GetHashCode())));

			if (!resume && File.Exists(this.m_DatabaseFileName))
			{
				ClearQueue();
			}

            this.m_EsentInstance = new EsentInstance(this.m_DatabaseFileName, (session, dbid) =>
				{
					EsentTableDefinitions.CreateGlobalsTable(session, dbid);
					EsentTableDefinitions.CreateQueueTable(session, dbid);
				});

            // Get columns
            this.m_EsentInstance.Cursor((session, dbid) =>
				{
					Api.JetGetColumnInfo(session, dbid, EsentTableDefinitions.GlobalsTableName,
						EsentTableDefinitions.GlobalsCountColumnName,
						out this.queueCountColumn);
					Api.JetGetColumnInfo(session, dbid, EsentTableDefinitions.QueueTableName,
						EsentTableDefinitions.QueueTableDataColumnName,
						out this.dataColumn);
				});
        }

        public EsentCrawlQueueService(Uri baseUri, bool resume)
            : this(Directory.GetCurrentDirectory(), baseUri, resume)
        {
        }

        #endregion

        #region Instance Methods

        protected override void Cleanup()
		{
            this.m_EsentInstance.Dispose();
			base.Cleanup();
		}

		protected override long GetCount()
		{
			return this.m_EsentInstance.Table(EsentTableDefinitions.GlobalsTableName,
				(session, dbid, table) =>
					{
						var tmp = Api.RetrieveColumnAsInt32(session, table, this.queueCountColumn.columnid);
						if (tmp.HasValue)
						{
							return (long) tmp.Value;
						}

						return 0;
					});
		}

		protected override CrawlerQueueEntry PopImpl()
		{
			return this.m_EsentInstance.Cursor((session, dbid) =>
				{
					using (var transaction = new Transaction(session))
					{
						using (var table = new Table(session, dbid, EsentTableDefinitions.QueueTableName, OpenTableGrbit.None))
						{
							if (Api.TryMoveFirst(session, table))
							{
								var data = Api.RetrieveColumnAsString(session, table, this.dataColumn.columnid, Encoding.Unicode);
								Api.JetDelete(session, table);

								using (var table2 = new Table(session, dbid, EsentTableDefinitions.GlobalsTableName, OpenTableGrbit.None))
								{
									Api.EscrowUpdate(session, table2, this.queueCountColumn.columnid, -1);
								}

								transaction.Commit(CommitTransactionGrbit.None);
								return data.FromJson<CrawlerQueueEntry>();
							}
						}

						transaction.Rollback();
						return null;
					}
				});
		}

		protected override void PushImpl(CrawlerQueueEntry crawlerQueueEntry)
		{
            this.m_EsentInstance.Cursor((session, dbid) =>
				{
					using (var transaction = new Transaction(session))
					{
						using (var table = new Table(session, dbid, EsentTableDefinitions.QueueTableName, OpenTableGrbit.None))
						{
							using (var update = new Update(session, table, JET_prep.Insert))
							{
								Api.SetColumn(session, table, this.dataColumn.columnid, crawlerQueueEntry.ToJson(), Encoding.Unicode);
								update.Save();
							}
						}

						using (var table = new Table(session, dbid, EsentTableDefinitions.GlobalsTableName, OpenTableGrbit.None))
						{
							Api.EscrowUpdate(session, table, this.queueCountColumn.columnid, 1);
						}

						transaction.Commit(CommitTransactionGrbit.None);
					}
				});
		}

		private void ClearQueue()
		{
			File.Delete(this.m_DatabaseFileName);
		}

		#endregion
	}
}