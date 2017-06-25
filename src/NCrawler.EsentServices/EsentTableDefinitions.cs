using System;
using System.Globalization;

using Microsoft.Isam.Esent.Interop;

namespace NCrawler.EsentServices
{
	public class EsentTableDefinitions
	{
		#region Constants

		public const string GlobalsCountColumnName = "Count";
		public const string GlobalsPrimaryKeyColumnName = "Id";
		public const string GlobalsTableName = "Globals";
		public const string HistoryTableName = "History";
		public const string HistoryTableUrlColumnName = "Url";
		public const string QueueTableDataColumnName = "Data";
		public const string QueueTableIdColumnName = "Id";
		public const string QueueTableName = "Queue";

		#endregion

		#region Class Methods

		/// <summary>
		/// 	Create the globals table.
		/// </summary>
		/// <param name = "session">The session to use.</param>
		/// <param name = "dbid">The database to create the table in.</param>
		public static void CreateGlobalsTable(Session session, JET_DBID dbid)
		{

            Api.JetCreateTable(session, dbid, GlobalsTableName, 1, 100, out var tableid);
            var defaultValue = BitConverter.GetBytes(0);

			Api.JetAddColumn(
				session,
				tableid,
				GlobalsPrimaryKeyColumnName,
				new JET_COLUMNDEF {coltyp = JET_coltyp.Long},
				null,
				0,
				out var primaryKeyColumnid);

			Api.JetAddColumn(
				session,
				tableid,
				GlobalsCountColumnName,
				new JET_COLUMNDEF {coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnEscrowUpdate},
				defaultValue,
				defaultValue.Length,
				out var countColumnid);

			var indexKey = string.Format(CultureInfo.InvariantCulture, "+{0}\0\0", GlobalsPrimaryKeyColumnName);
			var indexcreates = new[]
				{
					new JET_INDEXCREATE
						{
							cbKeyMost = SystemParameters.KeyMost,
							grbit = CreateIndexGrbit.IndexPrimary,
							szIndexName = "by_id",
							szKey = indexKey,
							cbKey = indexKey.Length,
							pidxUnicode = new JET_UNICODEINDEX
								{
									lcid = CultureInfo.CurrentCulture.LCID,
									dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
								},
						},
				};
			Api.JetCreateIndex2(session, tableid, indexcreates, indexcreates.Length);

			using (var update = new Update(session, tableid, JET_prep.Insert))
			{
				Api.SetColumn(session, tableid, countColumnid, 0);
				update.Save();
			}

			Api.JetCloseTable(session, tableid);
		}

		public static void CreateHistoryTable(Session session, JET_DBID dbid)
		{

            Api.JetCreateTable(session, dbid, HistoryTableName, 128, 100, out var tableid);
            Api.JetAddColumn(
				session,
				tableid,
				HistoryTableUrlColumnName,
				new JET_COLUMNDEF
					{
						coltyp = JET_coltyp.LongText,
						cp = JET_CP.Unicode,
						grbit = ColumndefGrbit.ColumnNotNULL
					},
				null,
				0,
				out var urlColumnid);

			var indexKey = string.Format(CultureInfo.InvariantCulture, "+{0}\0\0", HistoryTableUrlColumnName);
			var indexcreates = new[]
				{
					new JET_INDEXCREATE
						{
							cbKeyMost = SystemParameters.KeyMost,
							grbit = CreateIndexGrbit.IndexPrimary,
							szIndexName = "by_id",
							szKey = indexKey,
							cbKey = indexKey.Length,
							pidxUnicode = new JET_UNICODEINDEX
								{
									lcid = CultureInfo.CurrentCulture.LCID,
									dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
								},
						},
				};
			Api.JetCreateIndex2(session, tableid, indexcreates, indexcreates.Length);

			Api.JetCloseTable(session, tableid);
		}

		public static void CreateQueueTable(Session session, JET_DBID dbid)
		{

            Api.JetCreateTable(session, dbid, QueueTableName, 128, 100, out var tableid);
            Api.JetAddColumn(
				session,
				tableid,
				QueueTableIdColumnName,
				new JET_COLUMNDEF
					{
						coltyp = JET_coltyp.Long,
						cp = JET_CP.None,
						grbit = ColumndefGrbit.ColumnAutoincrement | ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnFixed
					},
				null,
				0,
				out var idColumnid);

			Api.JetAddColumn(
				session,
				tableid,
				QueueTableDataColumnName,
				new JET_COLUMNDEF {coltyp = JET_coltyp.LongText, grbit = ColumndefGrbit.None},
				null,
				0,
				out var dataColumnid);

			var indexKey = string.Format(CultureInfo.InvariantCulture, "+{0}\0\0", QueueTableIdColumnName);
			var indexcreates = new[]
				{
					new JET_INDEXCREATE
						{
							cbKeyMost = SystemParameters.KeyMost,
							grbit = CreateIndexGrbit.IndexPrimary,
							szIndexName = "by_id",
							szKey = indexKey,
							cbKey = indexKey.Length,
							pidxUnicode = new JET_UNICODEINDEX
								{
									lcid = CultureInfo.CurrentCulture.LCID,
									dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
								},
						},
				};
			Api.JetCreateIndex2(session, tableid, indexcreates, indexcreates.Length);

			Api.JetCloseTable(session, tableid);
		}

		#endregion
	}
}