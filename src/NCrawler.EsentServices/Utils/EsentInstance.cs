using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Windows7;

using NCrawler.Extensions;
using NCrawler.Utils;

namespace NCrawler.EsentServices.Utils
{
	public class EsentInstance : DisposableBase
	{
		#region Readonly & Static Fields

		private readonly Action<Session, JET_DBID> m_CreateTable;
		private readonly Stack<Cursor> m_Cursors = new Stack<Cursor>();
		private readonly string m_DatabaseFileName;

		#endregion

		#region Constructors

		public EsentInstance(string databaseFileName, Action<Session, JET_DBID> createTable)
		{
            this.m_DatabaseFileName = databaseFileName;
            this.m_CreateTable = createTable;

			AspectF.Define.
				//Retry(TimeSpan.Zero, 1, null).
				Do(this.InitInstance);

			try
			{
				if (!File.Exists(this.m_DatabaseFileName))
				{
                    this.CreateDatabase();
				}
			}
			catch (Exception)
			{
                // We have failed to initialize for some reason. Terminate
                // the instance.
                this.Instance.Term();
				throw;
			}
		}

		private void InitInstance()
		{
			var directory = Path.GetDirectoryName(this.m_DatabaseFileName);
            this.Instance = new Instance(Guid.NewGuid().ToString());
            this.Instance.Parameters.TempDirectory = Path.Combine(directory, "temp");
            this.Instance.Parameters.SystemDirectory = Path.Combine(directory, "system");
            this.Instance.Parameters.LogFileDirectory = Path.Combine(directory, "logs");
            this.Instance.Parameters.AlternateDatabaseRecoveryDirectory = directory;
            this.Instance.Parameters.CreatePathIfNotExist = true;
            this.Instance.Parameters.EnableIndexChecking = false;
            this.Instance.Parameters.CircularLog = true;
            this.Instance.Parameters.CheckpointDepthMax = 64 * 1024 * 1024;
            this.Instance.Parameters.LogFileSize = 1024; // 1MB logs
            this.Instance.Parameters.LogBuffers = 1024; // buffers = 1/2 of logfile
            this.Instance.Parameters.MaxTemporaryTables = 0;
            this.Instance.Parameters.MaxVerPages = 1024;
            this.Instance.Parameters.NoInformationEvent = true;
            this.Instance.Parameters.WaypointLatency = 1;
            this.Instance.Parameters.MaxSessions = 256;
            this.Instance.Parameters.MaxOpenTables = 256;
            this.Instance.Parameters.EventSource = "NCrawler";

			var grbit = EsentVersion.SupportsWindows7Features
				? Windows7Grbits.ReplayIgnoreLostLogs
				: InitGrbit.None;
			try
			{
                this.Instance.Init(grbit);
			}
			catch
			{
				Directory.Delete(directory, true);
				throw;
			}
		}

		#endregion

		#region Instance Properties

		public Instance Instance { get; set; }

		#endregion

		#region Instance Methods

		public T Cursor<T>(Func<Session, JET_DBID, T> action)
		{
			Cursor cursor;
			lock (this.m_Cursors)
			{
				cursor = this.m_Cursors.Count > 0 ? this.m_Cursors.Pop() : new Cursor(this.Instance, this.m_DatabaseFileName);
			}

			try
			{
				return action(cursor.Session, cursor.Dbid);
			}
			finally
			{
				lock (this.m_Cursors)
				{
                    this.m_Cursors.Push(cursor);
				}
			}
		}

		public void Cursor(Action<Session, JET_DBID> action)
		{
            this.Cursor((session, dbid) =>
				{
					action(session, dbid);
					return (object) null;
				});
		}

		public T Table<T>(string tableName, Func<Session, JET_DBID, Table, T> action)
		{
			return this.Cursor((session, dbid) =>
				{
					using (var table = new Table(session, dbid, tableName, OpenTableGrbit.None))
					{
						return action(session, dbid, table);
					}
				});
		}

		protected override void Cleanup()
		{
            this.m_Cursors.ForEach(cursor => cursor.Dispose());
            //Instance.Dispose();
            this.Instance.Term();
		}

		private void CreateDatabase()
		{
			using (var session = new Session(this.Instance))
			{
                Api.JetCreateDatabase(session, this.m_DatabaseFileName, string.Empty, out var dbid, CreateDatabaseGrbit.None);
                try
				{
					using (var transaction = new Transaction(session))
					{
                        this.m_CreateTable(session, dbid);
						transaction.Commit(CommitTransactionGrbit.None);
						Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
						Api.JetDetachDatabase(session, this.m_DatabaseFileName);
					}
				}
				catch (Exception)
				{
					// Delete the partially constructed database
					Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
					Api.JetDetachDatabase(session, this.m_DatabaseFileName);
					File.Delete(this.m_DatabaseFileName);
					throw;
				}
			}
		}

		#endregion
	}
}