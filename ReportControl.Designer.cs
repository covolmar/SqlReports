namespace Rittmeyer.WinKP.SqlReports
{
	partial class ReportControl
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && (components != null) )
			{
				components.Dispose( );
			}
			base.Dispose( disposing );
		}

		#region Vom Komponenten-Designer generierter Code

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent( )
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportControl));
			this.pnlReportDetails = new Rittmeyer.Windows.Forms.RAG_PanelBase();
			this.cmbStation = new Rittmeyer.Windows.Forms.RAG_ComboBox();
			this.cmbReportVorlage = new Rittmeyer.Windows.Forms.RAG_ComboBox();
			this.labelStation = new Rittmeyer.Windows.Forms.RAG_LabelBase();
			this.labelReportVorlage = new Rittmeyer.Windows.Forms.RAG_LabelBase();
			this.btnLoadReport = new Rittmeyer.Windows.Forms.RAG_ButtonBase();
			this.reportViewerControl = new Microsoft.Reporting.WinForms.ReportViewer();
			this.pnlReportDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlReportDetails
			// 
			this.pnlReportDetails.Controls.Add(this.cmbStation);
			this.pnlReportDetails.Controls.Add(this.cmbReportVorlage);
			this.pnlReportDetails.Controls.Add(this.labelStation);
			this.pnlReportDetails.Controls.Add(this.labelReportVorlage);
			this.pnlReportDetails.Controls.Add(this.btnLoadReport);
			resources.ApplyResources(this.pnlReportDetails, "pnlReportDetails");
			this.pnlReportDetails.Name = "pnlReportDetails";
			// 
			// cmbStation
			// 
			this.cmbStation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbStation.FormattingEnabled = true;
			this.cmbStation.HasChangeDetection = true;
			resources.ApplyResources(this.cmbStation, "cmbStation");
			this.cmbStation.Name = "cmbStation";
			this.cmbStation.Sorted = true;
			this.cmbStation.ValidateOnEnter = false;
			this.cmbStation.Wert = null;
			this.cmbStation.SelectionChangeCommitted += new System.EventHandler(this.cmbStation_SelectionChangeCommitted);
			// 
			// cmbReportVorlage
			// 
			this.cmbReportVorlage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbReportVorlage.FormattingEnabled = true;
			this.cmbReportVorlage.HasChangeDetection = true;
			resources.ApplyResources(this.cmbReportVorlage, "cmbReportVorlage");
			this.cmbReportVorlage.Name = "cmbReportVorlage";
			this.cmbReportVorlage.ValidateOnEnter = false;
			this.cmbReportVorlage.Wert = null;
			this.cmbReportVorlage.SelectionChangeCommitted += new System.EventHandler(this.cmbReportVorlage_SelectionChangeCommitted);
			// 
			// labelStation
			// 
			resources.ApplyResources(this.labelStation, "labelStation");
			this.labelStation.Name = "labelStation";
			// 
			// labelReportVorlage
			// 
			resources.ApplyResources(this.labelReportVorlage, "labelReportVorlage");
			this.labelReportVorlage.Name = "labelReportVorlage";
			// 
			// btnLoadReport
			// 
			resources.ApplyResources(this.btnLoadReport, "btnLoadReport");
			this.btnLoadReport.Name = "btnLoadReport";
			this.btnLoadReport.UseVisualStyleBackColor = true;
			this.btnLoadReport.Click += new System.EventHandler(this.BtnLoadReport_Click);
			// 
			// reportViewerControl
			// 
			this.reportViewerControl.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.reportViewerControl, "reportViewerControl");
			this.reportViewerControl.Name = "reportViewerControl";
			this.reportViewerControl.ServerReport.BearerToken = null;
			// 
			// ReportControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.reportViewerControl);
			this.Controls.Add(this.pnlReportDetails);
			this.Name = "ReportControl";
			this.pnlReportDetails.ResumeLayout(false);
			this.pnlReportDetails.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Windows.Forms.RAG_PanelBase pnlReportDetails;
		private Windows.Forms.RAG_ButtonBase btnLoadReport;
		private Windows.Forms.RAG_LabelBase labelReportVorlage;
		private Windows.Forms.RAG_LabelBase labelStation;
		private Windows.Forms.RAG_ComboBox cmbStation;
		private Windows.Forms.RAG_ComboBox cmbReportVorlage;
		private Microsoft.Reporting.WinForms.ReportViewer reportViewerControl;
	}
}
