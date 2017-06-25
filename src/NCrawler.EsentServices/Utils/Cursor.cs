using Microsoft.Isam.Esent.Interop;

using NCrawler.Utils;

namespace NCrawler.EsentServices.Utils
{
	public class Cursor : DisposableBase
	{
		#region Readonly & Static Fields

		public readonly JET_DBID Dbid;
		public readonly Session Session;
		private readonly string m_DatabaseFileName;

		#endregion

		#region Constructors

		public Cursor(Instance instance, string databaseFileName)
		{
            this.m_DatabaseFileName = databaseFileName;
            this.Session = new Session(instance);
			Api.JetAttachDatabase(this.Session, databaseFileName, AttachDatabaseGrbit.None);
			Api.JetOpenDatabase(this.Session, databaseFileName, null, out this.Dbid, OpenDatabaseGrbit.None);
		}

		#endregion

		#region Instance Methods

		protected override void Cleanup()
		{
			Api.JetCloseDatabase(this.Session, this.Dbid, CloseDatabaseGrbit.None);
			Api.JetDetachDatabase(this.Session, this.m_DatabaseFileName);
            this.Session.Dispose();
		}

		#endregion
	}
}