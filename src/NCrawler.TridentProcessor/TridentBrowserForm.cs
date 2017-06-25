using System;
using System.Drawing;
using System.Windows.Forms;

using mshtml;

namespace NCrawler.IEProcessor
{
	public partial class TridentBrowserForm : Form
	{
		#region Readonly & Static Fields

		private readonly string m_Url;

		#endregion

		#region Constructors

		static TridentBrowserForm()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
		}

		public TridentBrowserForm(string url)
		{
            this.m_Url = url;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(-20000, -20000);
            this.Size = new Size(1, 1);
			InitializeComponent();
		}

		#endregion

		#region Instance Properties

		public string DocumentDomHtml { get; private set; }

		#endregion

		#region Instance Methods

		protected override void OnLoad(EventArgs e)
		{
            this.IEWebBrowser.DocumentCompleted += (s, ee) =>
				{
					if (this.IEWebBrowser.Document == null)
					{
						return;
					}

					var htmlDocument = this.IEWebBrowser.Document.DomDocument as IHTMLDocument2;
					if (htmlDocument == null)
					{
						return;
					}

					if (htmlDocument.body != null && htmlDocument.body.parentElement != null)
					{
                        this.DocumentDomHtml = htmlDocument.body.parentElement.outerHTML;
						Close();
					}
				};

            this.IEWebBrowser.Navigate(this.m_Url);
		}

		#endregion
	}
}