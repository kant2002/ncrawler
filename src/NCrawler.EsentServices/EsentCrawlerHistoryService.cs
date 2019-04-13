using System;
using System.IO;
using System.Text;

using Microsoft.Isam.Esent.Interop;

using NCrawler.EsentServices.Utils;
using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.EsentServices
{
	public class EsentCrawlerHistoryService : HistoryServiceBase
	{
		private readonly bool m_Resume;

		#region Readonly & Static Fields

		private readonly string m_DatabaseFileName;
		private readonly EsentInstance m_EsentInstance;

		#endregion

		#region Fields

		private JET_COLUMNDEF historyCountColumn;
		private JET_COLUMNDEF historyUrlColumn;

		#endregion

		#region Constructors

		public EsentCrawlerHistoryService(string basePath, Uri baseUri, bool resume)
		{
            this.m_Resume = resume;
            this.m_DatabaseFileName = Path.GetFullPath(
                Path.Combine(basePath, "NCrawlHist{0}\\Hist.edb".FormatWith(baseUri.GetHashCode())));

			if (!resume)
			{
                this.ClearHistory();
			}

            this.m_EsentInstance = new EsentInstance(this.m_DatabaseFileName, (session, dbid) =>
				{
					EsentTableDefinitions.CreateGlobalsTable(session, dbid);
					EsentTableDefinitions.CreateHistoryTable(session, dbid);
				});

            // Get columns
            this.m_EsentInstance.Cursor((session, dbid) =>
				{
					Api.JetGetColumnInfo(session, dbid, EsentTableDefinitions.GlobalsTableName,
						EsentTableDefinitions.GlobalsCountColumnName, out this.historyCountColumn);
					Api.JetGetColumnInfo(session, dbid, EsentTableDefinitions.HistoryTableName,
						EsentTableDefinitions.HistoryTableUrlColumnName, out this.historyUrlColumn);
				});
        }

        public EsentCrawlerHistoryService(Uri baseUri, bool resume)
            : this(Directory.GetCurrentDirectory(), baseUri, resume)
        {

        }

        #endregion

        #region Instance Methods

        protected override void Add(string key)
		{
            this.m_EsentInstance.Cursor((session, dbid) =>
				{
					using (var transaction = new Transaction(session))
					{
						using (var table = new Table(session, dbid, EsentTableDefinitions.HistoryTableName, OpenTableGrbit.None))
						{
							using (var update = new Update(session, table, JET_prep.Insert))
							{
								Api.SetColumn(session, table, this.historyUrlColumn.columnid, key, Encoding.Unicode);
								update.Save();
							}
						}

						using (var table = new Table(session, dbid, EsentTableDefinitions.GlobalsTableName, OpenTableGrbit.None))
						{
							Api.EscrowUpdate(session, table, this.historyCountColumn.columnid, 1);
						}

						transaction.Commit(CommitTransactionGrbit.None);
					}
				});
		}

		protected override void Cleanup()
		{
			if (!this.m_Resume)
			{
                this.ClearHistory();
			}

            this.m_EsentInstance.Dispose();
			base.Cleanup();
		}

		protected override bool Exists(string key)
		{
			return this.m_EsentInstance.Table(EsentTableDefinitions.HistoryTableName,
				(session, dbid, table) =>
					{
						Api.JetSetCurrentIndex(session, table, "by_id");
						Api.MakeKey(session, table, key, Encoding.Unicode, MakeKeyGrbit.NewKey);
						return Api.TrySeek(session, table, SeekGrbit.SeekEQ);
					});
		}

		protected override long GetRegisteredCount()
		{
			return this.m_EsentInstance.Table(EsentTableDefinitions.GlobalsTableName,
				(session, dbid, table) =>
					{
						var tmp = Api.RetrieveColumnAsInt32(session, table, this.historyCountColumn.columnid);
						if (tmp.HasValue)
						{
							return (long) tmp.Value;
						}

						return 0;
					});
		}

		private void ClearHistory()
		{
            try
            {
                if (File.Exists(this.m_DatabaseFileName))
                {
                    File.Delete(this.m_DatabaseFileName);
                }
            }
            catch
            {
            }
		}

		#endregion
	}
}