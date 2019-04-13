using System;
using System.Drawing;
using System.Windows.Forms;

using Skybound.Gecko;

namespace NCrawler.GeckoProcessor
{
	public partial class GeckoBrowserForm : Form
	{
		#region Readonly & Static Fields

		private readonly GeckoWebBrowser m_GeckoWebBrowser = new GeckoWebBrowser();
		private readonly string m_Url;
		private static bool s_IsXulrunnerInitialized;

		#endregion

		#region Constructors

		static GeckoBrowserForm()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
		}

		public GeckoBrowserForm(string xulRunnerPath, string url)
		{
			InitializeXulRunner(xulRunnerPath);
            this.m_Url = url;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(-20000, -20000);
            this.Size = new Size(1, 1);
            this.Done = false;
            this.InitializeComponent();
		}

		#endregion

		#region Instance Properties

		public string DocumentDomHtml { get; private set; }

		public Boolean Done { get; set; }

		#endregion

		#region Instance Methods

		protected override void OnLoad(EventArgs e)
		{
            this.m_GeckoWebBrowser.Parent = this;
            this.m_GeckoWebBrowser.Dock = DockStyle.Fill;
            this.m_GeckoWebBrowser.DocumentCompleted += (s, ee) =>
				{
                    this.DocumentDomHtml = this.m_GeckoWebBrowser.Document.DocumentElement.InnerHtml;
					if (this.m_Url.Equals(this.m_GeckoWebBrowser.Document.Url.ToString(), StringComparison.OrdinalIgnoreCase))
					{
                        this.Done = true;
					}
				};

            this.m_GeckoWebBrowser.Navigate(this.m_Url);
		}

		#endregion

		#region Class Methods

		private static void InitializeXulRunner(string path)
		{
			if (s_IsXulrunnerInitialized)
			{
				return;
			}

			s_IsXulrunnerInitialized = true;
			Xpcom.Initialize(path);
		}

		#endregion
	}
}