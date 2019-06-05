﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Web.UI;
using System.Runtime.InteropServices;

using Abstractspoon.Tdl.PluginHelpers;

namespace HTMLReportExporter
{
	public partial class HtmlReportTemplateForm : Form
	{
		private String m_TypeId = String.Empty;
		private String m_PrefsKey = String.Empty;
		private Translator m_Trans = null;
		private TaskList m_Tasklist = null;
		private Preferences m_Prefs = null;
		private UIThemeToolbarRenderer m_TBRenderer = null;

		private HtmlReportTemplate m_Template = null;
		private HtmlReportTemplate m_PrevTemplate = null;
		private Timer m_ChangeTimer = null;
		private String m_TemplateFilePath = "HtmlReportTemplate.txt";
		private bool m_FirstPreview = true;

		// --------------------------------------------------------------

		private enum PageType
		{
			Header = 0,
			Title = 1,
			Tasks = 2,
			Footer = 3
		}

		// --------------------------------------------------------------

		public HtmlReportTemplateForm(String typeId, Translator trans, TaskList tasks, Preferences prefs, String key)
		{
			m_TypeId = typeId;
			m_Trans = trans;
			m_Tasklist = tasks;
			m_Prefs = prefs;
			m_PrefsKey = key;

			m_TemplateFilePath = prefs.GetProfileString(key, "LastOpenTemplate", "");

			m_Template = new HtmlReportTemplate();
			m_PrevTemplate = new HtmlReportTemplate();

			m_ChangeTimer = new Timer();
			m_ChangeTimer.Tick += new EventHandler(OnChangeTimer);
			m_ChangeTimer.Interval = 500;

			InitializeComponentEx();
		}

		public HtmlReportTemplate ReportTemplate
		{
			get { return m_Template; }
		}

		private void InitializeComponentEx()
		{
			InitializeComponent();

			int bannerHeight = RhinoLicensing.CreateBanner(m_TypeId, this, m_Trans, 0/*20*/);

			this.Height = (this.Height + bannerHeight);
			this.Content.Location = new Point(0, bannerHeight);
			this.Content.Height = (this.Content.Height - bannerHeight);

			var defFontName = m_Prefs.GetProfileString("Preferences", "HTMLFont", "Verdana");
			var defFontSize = m_Prefs.GetProfileInt("Preferences", "HtmlFontSize", 2);

			this.htmlReportHeaderControl.SetFont(defFontName, defFontSize);
			this.htmlReportTitleControl.SetFont(defFontName, defFontSize);
			this.htmlReportTasksControl.SetFont(defFontName, defFontSize);
			this.htmlReportFooterControl.SetFont(defFontName, defFontSize);

			this.htmlReportHeaderControl.Translator = m_Trans;
			this.htmlReportTitleControl.Translator = m_Trans;
			this.htmlReportTasksControl.Translator = m_Trans;
			this.htmlReportFooterControl.Translator = m_Trans;

			if (!m_Template.Load(m_TemplateFilePath))
			{
#if DEBUG
				this.m_Template.Header.Text = "Header";
				this.m_Template.Header.BackColor = Color.LightBlue;
				this.m_Template.Title.Text = "Title";
				this.m_Template.Task.Text = "$(Title)";
				this.m_Template.Footer.Text = "Footer";
				this.m_Template.Footer.BackColor = Color.LightPink;

				this.m_TemplateFilePath = "Example.rwt";
#endif
			}

			this.tabControl.SelectedIndexChanged += new EventHandler(OnTabPageChange);
			this.browserPreview.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnPreviewLoaded);
			this.headerEnabledCheckbox.CheckedChanged += new EventHandler(OnHeaderEnableChanged);
			this.titleEnabledCheckbox.CheckedChanged += new EventHandler(OnTitleEnableChanged);
			this.footerEnabledCheckbox.CheckedChanged += new EventHandler(OnFooterEnableChanged);

			m_Trans.Translate(this);
			
			UpdateControls();
			InitialiseToolbar();

			m_ChangeTimer.Start();
		}

		private void InitialiseToolbar()
		{
			m_TBRenderer = new UIThemeToolbarRenderer();

			m_TBRenderer.SetUITheme(new UITheme());
			m_TBRenderer.EnableDrawRowDividers(true);
			//m_TBRenderer.EnableDrawBorders(this.Toolbar, false, false, false, true);

			this.Toolbar.Renderer = m_TBRenderer;

			// Place this at the end to ensure the toolbar has finished its resize
			Toolbars.FixupButtonSizes(this.Toolbar);
		}

		private void UpdateControls()
		{
			this.htmlReportHeaderControl.InnerHtml = m_Template.Header.Text;
			this.htmlReportHeaderControl.BodyBackColor = m_Template.Header.BackColor;
			this.headerEnabledCheckbox.Checked = m_Template.Header.Enabled;
			this.headerDividerCheckbox.Checked = m_Template.Header.WantDivider;
			this.headerHeightCombobox.Text = m_Template.Header.PixelHeightText;

			this.htmlReportTitleControl.InnerHtml = m_Template.Title.Text;
			this.titleEnabledCheckbox.Checked = m_Template.Title.Enabled;
			this.titleSeparatePageCheckbox.Checked = m_Template.Title.SeparatePage;

			this.htmlReportTasksControl.InnerHtml = m_Template.Task.Text;

			this.htmlReportFooterControl.InnerHtml = m_Template.Footer.Text;
			this.htmlReportFooterControl.BodyBackColor = m_Template.Footer.BackColor;
			this.footerEnabledCheckbox.Checked = m_Template.Footer.Enabled;
			this.footerDividerCheckbox.Checked = m_Template.Footer.WantDivider;
			this.footerHeightCombobox.Text = m_Template.Footer.PixelHeightText;

			// Refresh enable states
			// Note: 'Task' control always enabled
			OnHeaderEnableChanged(this.headerEnabledCheckbox, new EventArgs());
			OnTitleEnableChanged(this.titleEnabledCheckbox, new EventArgs());
			OnFooterEnableChanged(this.footerEnabledCheckbox, new EventArgs());
			
			RefreshPreview();
		}

		private void OnHeaderEnableChanged(object sender, EventArgs args)
		{
			EnableControls(headerPage, sender, m_Template.Header.BackColor);
		}

		private void OnTitleEnableChanged(object sender, EventArgs args)
		{
			EnableControls(titlePage, sender, Color.Empty);
		}

		private void OnFooterEnableChanged(object sender, EventArgs args)
		{
			EnableControls(footerPage, sender, m_Template.Footer.BackColor);
		}

		private void EnableControls(TabPage page, object checkbox, Color enabledBackColor)
		{
			if ((checkbox != null) && (checkbox is CheckBox))
			{
				bool enabled = (checkbox as CheckBox).Checked;

				foreach (System.Windows.Forms.Control control in page.Controls)
				{
					if (control != checkbox)
					{
						control.Enabled = enabled;

						// restore background colour
						if (enabled && (enabledBackColor != Color.Empty) && (control is HtmlReportHeaderFooterControl))
						{
							(control as HtmlReportHeaderFooterControl).BodyBackColor = enabledBackColor;
						}
					}
				}
			}
		}

		private void OnPreviewLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (m_FirstPreview) // First time only
			{
				SetPreviewZoom(DPIScaling.Scale(40));
				m_FirstPreview = false;
			}
		}

		private void SetPreviewZoom(int percent)
		{
			int OLECMDID_OPTICAL_ZOOM = 63;
			int OLECMDEXECOPT_DONTPROMPTUSER = 2;

			if ((percent >= 10) && (percent <= 1000))
			{
				dynamic ax = this.browserPreview.ActiveXInstance;

				if (ax != null)
					ax.ExecWB(OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT_DONTPROMPTUSER, new VariantWrapper(percent), new VariantWrapper(0));
			}
		}

		private void OnTabPageChange(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				switch ((PageType)tabControl.SelectedIndex)
				{
					case PageType.Header: this.htmlReportHeaderControl.SetActive(); break;
					case PageType.Title:  this.htmlReportTitleControl.SetActive();  break;
					case PageType.Tasks:  this.htmlReportTasksControl.SetActive();  break;
					case PageType.Footer: this.htmlReportFooterControl.SetActive(); break;
				}
			}
		}
		
		private void OnChangeTimer(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				switch ((PageType)tabControl.SelectedIndex)
				{
					case PageType.Header:
						m_Template.Header.Text = this.htmlReportHeaderControl.InnerHtml ?? "";
						m_Template.Header.Enabled = headerEnabledCheckbox.Checked;
						m_Template.Header.WantDivider = headerDividerCheckbox.Checked;
						m_Template.Header.BackColor = this.htmlReportHeaderControl.BodyBackColor;
						m_Template.Header.PixelHeightText = this.headerHeightCombobox.Text;
						break;

					case PageType.Title:
						m_Template.Title.Text = this.htmlReportTitleControl.InnerHtml ?? "";
						m_Template.Title.Enabled = titleEnabledCheckbox.Checked;
						m_Template.Title.SeparatePage = this.titleSeparatePageCheckbox.Checked;
						break;

					case PageType.Tasks:
						m_Template.Task.Text = this.htmlReportTasksControl.InnerHtml ?? "";
						m_Template.Task.Enabled = true; // always
						break;

					case PageType.Footer:
						m_Template.Footer.Text = this.htmlReportFooterControl.InnerHtml ?? "";
						m_Template.Footer.Enabled = footerEnabledCheckbox.Checked;
						m_Template.Footer.WantDivider = footerDividerCheckbox.Checked;
						m_Template.Footer.BackColor = this.htmlReportFooterControl.BodyBackColor;
						m_Template.Footer.PixelHeightText = this.footerHeightCombobox.Text;
						break;
				}

				CheckRefreshPreview();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel)
			{
				CheckSaveTemplate(false);
				m_Prefs.GetProfileString(m_PrefsKey, "LastOpenTemplate", m_TemplateFilePath);
			}
		}

		private void CheckRefreshPreview()
		{
			if (!m_Template.Equals(m_PrevTemplate))
			{
				m_ChangeTimer.Stop();

				m_PrevTemplate.Copy(m_Template);
				RefreshPreview();

				m_ChangeTimer.Start();
			}
		}

		private void RefreshPreview()
		{
			try
			{
				String previewPath = Path.Combine(Path.GetTempPath(), "HtmlReporterPreview.html");

				using (var file = new StreamWriter(previewPath))
				{
					using (var html = new HtmlTextWriter(file))
					{
						var report = new HtmlReportBuilder(m_Tasklist, m_Template, m_Prefs);

						report.BuildReport(html);
					}
				}

				browserPreview.Navigate(new System.Uri(previewPath));
			}
			catch (Exception /*e*/)
			{
			}
		}

		private void OnNewReportTemplate(object sender, EventArgs e)
		{
			if (CheckSaveTemplate(true))
			{
				m_Template.Clear();
				m_TemplateFilePath = String.Empty;

				UpdateControls();
			}
		}

		private void OnOpenReportTemplate(object sender, EventArgs e)
		{
			var dlg = new OpenFileDialog
			{
				//InitialDirectory = LastBrowsedFolder,
				Title = m_Trans.Translate("Open Template File"),

				AutoUpgradeEnabled = true,
				CheckFileExists = true,
				CheckPathExists = true,

				Filter = FileFilter,
				FilterIndex = 0,
				RestoreDirectory = true,

				ShowReadOnly = false
			};

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				if (m_Template.Load(dlg.FileName))
				{
					m_TemplateFilePath = dlg.FileName;
					UpdateControls();
				}
			}
		}

		private String FileFilter
		{
			get { return String.Format("{0} (*.rwt)|*.rwt;", m_Trans.Translate("Report Writer Templates"));	}
		}

		private void OnSaveReportTemplate(object sender, EventArgs e)
		{
			CheckSaveTemplate(false);
		}

		private void OnHelp(object sender, EventArgs e)
		{
			// TODO
		}

		private bool CheckSaveTemplate(bool mustHaveContents)
		{
			if (mustHaveContents && !m_Template.HasContents())
				return true;

			if (m_Template.Save())
				return true;

			// Prompt for a file path
			var dlg = new SaveFileDialog
			{
				//InitialDirectory = LastBrowsedFolder,
				Title = m_Trans.Translate("Save Template File"),

				AutoUpgradeEnabled = true,
				CheckPathExists = true,

				Filter = FileFilter,
				FilterIndex = 0,
				RestoreDirectory = true,
			};
			
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				if (m_Template.Save(dlg.FileName))
				{
					m_TemplateFilePath = dlg.FileName;
					return true;
				}
			}

			return false;
		}
	}
}