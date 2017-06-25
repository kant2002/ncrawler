namespace NCrawler.IFilterProcessor
{
	public class ExcelIFilterProcessor : IFilterProcessor
	{
		#region Constructors

		public ExcelIFilterProcessor()
			: base("application/excel", "xls")
		{
            this.m_MimeTypeExtensionMapping.Add("application/vnd.ms-excel", "xsl");
            this.m_MimeTypeExtensionMapping.Add("application/x-excel", "xsl");
            this.m_MimeTypeExtensionMapping.Add("application/x-msexcel", "xsl");
		}

		#endregion
	}
}