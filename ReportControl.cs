//////////////////////////////////////////////////////////////////////////////////
//	Copyright:	RITTMEYER AG,	Inwilerriedstrasse 57,	CH-6341  Baar			//
//////////////////////////////////////////////////////////////////////////////////
//																				//
// Projekt:	RIFLEX - Konfigurierprogramm										//
//																				//
// Filename:	ReportControl.cs					$Date:: 27.02.2018 12:12 $	//
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rittmeyer.WinKP.FrameWork;
using Rittmeyer.Business.Object;
using Rittmeyer.Error;
using System.Data.SqlClient;
using System.Xml;
using Microsoft.Reporting.WinForms;
using Rittmeyer.Data.Object;
using Rittmeyer.Data;
using Rittmeyer.Data.Sql;
using System.Text.RegularExpressions;
using System.IO;
using Rittmeyer.Text;
using Rittmeyer.Business;
using Rittmeyer.Configuration.Data;
using Rittmeyer.Services;

namespace Rittmeyer.WinKP.SqlReports
{
	//////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// SQL Report Viewer. Der Report Viewer (Local Mode) veranschaulicht anhand einer Report
	/// Definitionsdatei (RDL, Report Definition Language welche in Microsoft SQL
	/// Server Reporting Services eingesetzt wird) die aktuellen Daten in der WinKP
	/// SQL Datenbank. Reports können mit dem SQL Server Report Builder erstellt 
	/// werden.
	/// </summary>
	/// <remarks><list type="table">
	/// <listheader>Änderungen</listheader>
	/// <item><term>03.01.2017, COM</term>
	/// <description>	Erstellt</description></item>
	/// </list></remarks>
	//////////////////////////////////////////////////////////////////////////////////

	public partial class ReportControl: ArbeitsControl, IArbeitsControl
	{
		/// <summary>lokaler Reports-Pfad für Benutzer spezifische Vorlagen</summary>
		private string REPORT_PATH = RAG_Services.ReportsVorlagePfad;

		///// <summary>lokaler Reports-Pfad für Standard Vorlagen</summary>
		//private string STANDARD_REPORT_PATH = Configuration.Data.Configuration.StandardReportVorlagePfad;

		/// <summary>lokaler Reports-Pfad für Standard Vorlagen</summary>
		private string STANDARD_REPORT_PATH = Path.Combine( RAG_Services.GetKPIncludeDirectory( ), Const.REPORT_VORLAGE_FOLDER );

		/// <summary>WinKP DataLayer</summary>
		private CDataLayer datalayer;

		/// <summary>WinKP SqlDataAdapter</summary>
		private CSqlDataAdapter sqlDataAdapter;

		/// <summary>Alle vorhandene Report Templates</summary>
		private List<RAG_ReportTemplate> reportTemplates;

		/// <summary>Name der der SQL Spalte "StationId"</summary>
		private const string COLUMN_NAME_STATION_ID = "StationId";

		/// <summary> Aktuell ausgewählte ReportTemplate </summary>
		private RAG_ReportTemplate reportTemplate;


		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Interne RAG_ReportTemplate Klasse. Kapselt das FileInfo Objekt.
		/// </summary>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>16.06.2018, COM</term>
		/// <description>	Erstellt</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private class RAG_ReportTemplate
		{
			/// <summary> FileInfo des Report Templates </summary>
			private FileInfo fileInfo;

			//////////////////////////////////////////////////////////////////////////////////
			//// ReportFileInfo( ) 
			//////////////////////////////////////////////////////////////////////////////////
			/// <summary>
			/// Konstruktor
			/// </summary>
			/// <remarks><list type="table">
			/// <listheader>Änderungen</listheader>
			/// <item><term>16.06.2018, COM</term>
			/// <description>	Erstellt.</description></item>
			/// </list></remarks>
			//////////////////////////////////////////////////////////////////////////////////

			public RAG_ReportTemplate( FileInfo fileInfo )
			{
				this.FileInfo = fileInfo;
			}


			//////////////////////////////////////////////////////////////////////////////////
			//// Property Name
			//////////////////////////////////////////////////////////////////////////////////
			/// <summary>
			/// Beschreibung
			/// </summary>
			/// <remarks><list type="table">
			/// <listheader>Änderungen</listheader>
			/// <item><term>16.06.2018, COM</term>
			/// <description>	Erstellung</description></item>
			/// </list></remarks>
			//////////////////////////////////////////////////////////////////////////////////

			public string ReportFileName
			{
				get => this.FileInfo.Name;
			}

			//////////////////////////////////////////////////////////////////////////////////
			//// Property FileInfo
			//////////////////////////////////////////////////////////////////////////////////
			/// <summary>
			/// Beschreibung
			/// </summary>
			/// <remarks><list type="table">
			/// <listheader>Änderungen</listheader>
			/// <item><term>16.06.2018, COM</term>
			/// <description>	Erstellung</description></item>
			/// </list></remarks>
			//////////////////////////////////////////////////////////////////////////////////

			public FileInfo FileInfo
			{
				get => fileInfo;
				private set => fileInfo = value;
			}

			//////////////////////////////////////////////////////////////////////////////////
			//// ToString( )
			//////////////////////////////////////////////////////////////////////////////////
			/// <summary>
			/// Überschreibt die Object ToString() Mehtode. Wird für die Combobox Anzeige der
			/// Datei Namen verwendet. 
			/// verwendet. 
			/// </summary>
			/// <remarks><list type="table">
			/// <listheader>Änderungen</listheader>
			/// <item><term>16.06.2018, COM</term>
			/// <description>	Erstellung</description></item>
			/// </list></remarks>
			//////////////////////////////////////////////////////////////////////////////////

			public override string ToString( )
			{
				return ReportFileName;
			}

		}


		//////////////////////////////////////////////////////////////////////////////////
		//// ReportControl( ) 
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Konstruktor
		/// </summary>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>27.02.2018, COM</term>
		/// <description>	Erstellt.</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public ReportControl( )
		{
			InitializeComponent( );
			this.reportTemplates = new List<RAG_ReportTemplate>( );
		}

		
		//////////////////////////////////////////////////////////////////////////////////
		//// Property BusinessObject
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public ICBusinessObject BusinessObject
		{
			get
			{
				return this.BusinessObject;
			}
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// Property SaveButtonEnabled
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Muss immer False zurück geben, da dieses Control nicht gespeichert werden kann.
		/// </summary>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>31.07.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public override bool SaveButtonEnabled
		{
			get { return false; }
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// CancelChanges( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public override bool CancelChanges( IErrorHandler errorHandler )
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// DisplayData( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public bool DisplayData( IErrorHandler errorHandler )
		{
			LoadCmbReportTemplates( errorHandler );
			LoadCmbStation( errorHandler );

			return true;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// Help( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public override void Help( IErrorHandler errorHandler )
		{
			base.Help( errorHandler );
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// InitializeControl( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <param name="businessObject"></param>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public override bool InitializeControl( ICBusinessObject businessObject, IErrorHandler errorHandler )
		{
			bool bOk = true;

			this.datalayer = (CDataLayer)CDataLayer.GetDataLayer( );
			if( datalayer != null )
			{
				this.sqlDataAdapter = datalayer.SqlDataAdapter;
			}

			if( this.datalayer == null || this.sqlDataAdapter == null )
				bOk = false;

			return bOk;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// SaveChanges( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Beschreibung
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		public bool SaveChanges( IErrorHandler errorHandler )
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// BtnLoadReport_Click( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Load Report Button Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private void BtnLoadReport_Click( object sender, EventArgs e )
		{
			IErrorHandler errorHandler = ErrorHandler.GetInstance( );

			if( !LoadReport( errorHandler ) )
			{
				errorHandler.SetError( this, "ReportControl: general error loading report", ErrorStates.Error );
				errorHandler.ShowError( );
			}
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// LoadReport( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Lädt den ausgewählten Report von der SQL Datenbank und stellt diesen im 
		/// Report Viewer dar. 
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>05.03.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>22.03.2018, COM</term>
		/// <description>Anpassung auf mehrere DataSets pro Report</description></item>
		/// <item><term>27.03.2018, COM</term>
		/// <description>falls nach Station gefiltert werden soll und es fehlt die 
		/// StationId Spalte in einem verwendetem DataSet, dann Report 
		/// Erstellung gleich abbrechen. </description></item>
		/// <item><term>04.06.2018, COM</term>
		/// <description>datalayer vor dem Laden immer neu initialisieren</description></item>
		/// <item><term>14.07.2018, COM</term>
		/// <description>Falls von einem vorhandem DataSet im Report keine SQL Daten geladen 
		/// werden konnten, dann Report Erstellung sofort abbrechen. </description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private bool LoadReport( IErrorHandler errorHandler )
		{
			bool bOk = true;
			int progress = 0;
			Cursor = Cursors.WaitCursor;

			StatusLeiste.WriteToStatusLeiste( Texte.GetText( this, "ReportControl: Lade Report..." ), StatusLeiste.LEFT, StatusLeiste.NO_ICON );

			// reinitalize always datalayer befor loading Reports from Sql. 
			InitializeControl( null, errorHandler );

			XmlDocument doc = new XmlDocument( );
			IList<DataSet> dataSets = new List<DataSet>();
			progress += 10;
			StatusLeiste.WriteProgress( progress );

			try
			{
				this.reportTemplate = null;
				if( cmbReportVorlage.SelectedItem != null && this.cmbReportVorlage.SelectedItem.GetType() == typeof(RAG_ReportTemplate) )
				{
					//Get selected Template
					this.reportTemplate = (RAG_ReportTemplate)cmbReportVorlage.SelectedItem;

					if( bOk = ( this.reportTemplate != null ) )
					{
						//Parse and Load report Template
						if( bOk &= !String.IsNullOrEmpty( this.reportTemplate.FileInfo.FullName ) )
						{
							doc.Load( this.reportTemplate.FileInfo.FullName );
							IDictionary<string, string> dataSetsInfo = GetDataSetsInfoFromXml( doc );
							progress += 10;
							StatusLeiste.WriteProgress( progress );

							if( bOk &= dataSetsInfo.Count > 0 )
							{
								int getDataProgress = 60 / dataSetsInfo.Count;

								foreach( KeyValuePair<string, string> entry in dataSetsInfo )
								{
									StatusLeiste.WriteToStatusLeiste( Texte.GetText( this, "ReportControl: Laden von DataSets", new object[] { entry.Key } ),
										StatusLeiste.LEFT, StatusLeiste.NO_ICON );

									DataSet dataSet;
									bOk &= GetData( entry.Key, entry.Value, out dataSet, errorHandler );
									if( bOk && dataSet != null && dataSet.Tables.Count > 0 )
									{
										dataSets.Add( dataSet );

										// Set 60% of total progress to fetch data from db!
										progress += getDataProgress;
										StatusLeiste.WriteProgress( progress );
									}
									else
									{
										break;
									}
								}

								// only create report dataSet and start rendering report if all required dataSet have been loaded successfully!
								if( bOk && dataSets.Count == dataSetsInfo.Count )
								{
									DataSet reportDataSet;
									if( this.cmbStation.SelectedItem != null )
									{
										IBO_Station stationToFilter = this.cmbStation.SelectedItem as IBO_Station;
										bOk = CreateReportDataSet( dataSets, dataSetsInfo, out reportDataSet, errorHandler, stationToFilter );
									}
									else
										bOk = CreateReportDataSet( dataSets, dataSetsInfo, out reportDataSet, errorHandler );

									progress += 10;
									StatusLeiste.WriteProgress( progress );
									if( bOk && reportDataSet.Tables.Count > 0 )
										RenderReportData( this.reportTemplate.FileInfo.FullName, dataSetsInfo, reportDataSet );
								}
							}
						}
					}
				}
				else
				{
					//no report selected, inform User!
					string text = Texte.GetText( this, "ReportControl: no report selected" );
					MessageBox.Show( text, "Reports", MessageBoxButtons.OK );
					bOk = false;
				}
			}
			catch( Exception ex )
			{
				bOk = false;
				errorHandler.SetError( this, "ReportControl: I/O Error Reports", new object[] { ex.Message }, ErrorStates.Error );
				errorHandler.ShowError( );
				Cursor = Cursors.Default;
			}

			StatusLeiste.WriteProgress( 100 );
			StatusLeiste.ClearStatusLeiste( 0 );
			Cursor = Cursors.Default;

			return bOk;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// CreateReportDataSet( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Erstellt aus mehreren DataSet Objekten ein einziges DataSet Objekt, welches
		/// alle für den Report benötigten DataTables enthält.
		/// </summary>
		/// <param name="dataSets"></param>
		/// <param name="dataSetsInfo"></param>
		/// <param name="outDataSet"></param>
		/// <param name="errorHandler"></param>
		/// <param name="stationToFilter">Wenn Station != null, denn nach dieser Station
		/// filtern. </param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>03.01.2017, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>16.04.2018, COM</term>
		/// <description> Filter angepasst. Wenn nach Station gefiltert wird, muss
		/// mindestens ein dataSet nach Station gefiltert sein. Sonst kein Report
		/// darstellen. </description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private bool CreateReportDataSet( IList<DataSet> dataSets, IDictionary<string, string> dataSetsInfo, out DataSet outDataSet,
			IErrorHandler errorHandler, IBO_Station stationToFilter = null )
		{
			bool bOk = true;
			bool filter = false;
			outDataSet = new DataSet( );

			/* get all dataSetTables in the different datasets and create
			 1 new dataset, which includes all datasetTables for the report. */
			foreach( DataSet dataSet in dataSets )
			{
				if( dataSet.Tables.Count > 0 && dataSet.Tables[ 0 ] != null )
				{
					DataTable originalData = dataSet.Tables[ 0 ];
					dataSet.Tables.Remove( originalData );

					if( stationToFilter != null )
					{
						DataTable filteredTableOnStation;
						filter |= QueryOnStation( originalData, out filteredTableOnStation, stationToFilter, errorHandler );
						if( filteredTableOnStation != null && filteredTableOnStation.Rows.Count > 0 )
							outDataSet.Tables.Add( filteredTableOnStation );
						else
							outDataSet.Tables.Add( originalData );
					}
					else
					{
						outDataSet.Tables.Add( originalData );
					}
				}
			}
			if( stationToFilter != null )
				bOk &= filter; // at least one dataSet has to be filterd. 

			return bOk;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// QueryOnStation( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Filtert das übergebene DataTable nach der ausgewählten Station. 
		/// </summary>
		/// <param name="originalData"></param>
		/// <param name="filteredTableOnStation"></param>
		/// <param name="errorHandler"></param>
		/// <param name="stationToFilter">Station, nach welcher die Daten gefiltert werden.</param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>05.03.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>21.03.2018, COM</term>
		/// <description>check if column stationId exist</description></item>
		/// <item><term>16.04.2018, COM</term>
		/// <description> out parameter hinzugefügt.</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private bool QueryOnStation( DataTable originalData, out DataTable filteredTableOnStation, IBO_Station stationToFilter, IErrorHandler errorHandler )
		{
			IEnumerable<DataRow> result;
			filteredTableOnStation = new DataTable( );
			filteredTableOnStation = originalData.Clone( );

			DataColumnCollection columns = originalData.Columns;
			if( columns.Contains( COLUMN_NAME_STATION_ID ) && stationToFilter != null )
			{
				result = from myRow in originalData.AsEnumerable( )
						 where myRow.Field<int>( COLUMN_NAME_STATION_ID ) == stationToFilter.Id
						 select myRow;

				if( result.Count<DataRow>( ) > 0 )
				{
					foreach( DataRow row in result )
					{
						filteredTableOnStation.ImportRow( row );
					}
				}
				return true;
			}
			else
			{
				//errorHandler.SetError( this, "ReportControl: Fehlende Spalte StationId", new object[] { originalData.TableName }, ErrorStates.Warning );
				//errorHandler.ShowError( );
				return false;
				
			}
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// GetDataSetsInfoFromXml( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Parst die DataSet Namen und die entprechenden Queries aus dem Report XML
		/// </summary>
		/// <param name="doc">Report XML</param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>22.03.2018, COM</term>
		/// <description> refactor für mehrere DataSets im Report </description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private IDictionary<string, string> GetDataSetsInfoFromXml( XmlDocument doc )
		{
			IDictionary<string, string> dataSets = new Dictionary<string, string>( );

			XmlElement root = doc.DocumentElement;
			if( root.HasChildNodes )
			{
				XmlElement dataSetsElement = root[ "DataSets" ];
				if( !dataSetsElement.IsEmpty )
				{
					XmlNodeList dataSetNodes = dataSetsElement.GetElementsByTagName( "DataSet" );
					foreach( XmlNode dataSetNode in dataSetNodes )
					{
						XmlAttributeCollection attributes = dataSetNode.Attributes;

						string dataSetName = attributes[ "Name" ].Value;
						dataSetName.Trim( );

						dataSets.Add( dataSetName, GetQueryFromReportXML( dataSetNode ) );
					}
				}
			}
			return dataSets;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// GetQueryFromReportXML( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Parst die auszuführende Query im Report XML. 
		/// </summary>
		/// <param name="dataSetNode">Report XML</param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private string GetQueryFromReportXML( XmlNode dataSetNode )
		{
			StringBuilder buffer = new StringBuilder( );
			StringBuilder queryBuilder = new StringBuilder( );
			string query = string.Empty;

			XmlNode queryNode = dataSetNode[ "Query" ];
			if( queryNode != null && queryNode.HasChildNodes )
			{
				XmlNode commandText = queryNode[ "CommandText" ];
				if( commandText != null )
				{
					string queryString = commandText.FirstChild.Value;
					string[] words = queryString.Split( ' ', '\n', '\r' );
					foreach( string word in words )
					{
						if( String.IsNullOrEmpty( word ) )
							continue;
						word.Trim( );
						foreach( char c in word )
						{
							if( Char.IsLetterOrDigit( c ) || Char.IsPunctuation( c ) || Char.IsSymbol( c ) )
							{
								buffer.Append( c );
							}
						}
						queryBuilder.Append( buffer.ToString( ) );
						queryBuilder.Append( " " );
						buffer.Clear( );
					}
					query = queryBuilder.ToString( );
					query.Trim( );
				}
			}
			return query;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// RenderReportData( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Rendert den Report im Report Viewer Control. 
		/// </summary>
		/// <param name="reportFile"></param>
		/// <param name="dataSetsInfo"></param>
		/// <param name="reportData"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private void RenderReportData( string reportFile, IDictionary<string, string> dataSetsInfo, DataSet reportData )
		{
			this.reportViewerControl.Reset( );
			this.reportViewerControl.LocalReport.DataSources.Clear( );

			this.reportViewerControl.LocalReport.ReportPath = reportFile;
			this.reportViewerControl.ProcessingMode = ProcessingMode.Local;
			this.reportViewerControl.LocalReport.EnableHyperlinks = true;

			foreach( KeyValuePair<string, string> entry in dataSetsInfo )
			{
				ReportDataSource rds = new ReportDataSource( );
				rds.Name = entry.Key;
				rds.Value = reportData.Tables[ entry.Key ];

				this.reportViewerControl.LocalReport.DataSources.Add( rds );
			}

			this.reportViewerControl.LocalReport.Refresh( );
			this.reportViewerControl.RefreshReport( );

			this.reportViewerControl.LocalReport.ReleaseSandboxAppDomain( );
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// GetData( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Lädt die für den Report benötigten Daten aus der SQL Datenbank. 
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="query"></param>
		/// <param name="reportData"></param>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private bool GetData( string tableName, string query, out DataSet reportData, IErrorHandler errorHandler )
		{
			bool bOk = true;
			reportData = null;
			
			IDbCommand cmd = new SqlCommand( query );
			bOk = sqlDataAdapter.Select( cmd, out reportData, errorHandler, tableName );

			if( !bOk )
			{
				errorHandler.SetError( this, "ReportControl: Daten konnten nicht geladen werden", new object[] { tableName }, ErrorStates.Error );
				errorHandler.ShowError( );
			}

			return bOk;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// LoadCmbReportTemplates( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Füllt die Report Combobox mit den verfügbaren Reports auf dem Dateisystem. 
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>28.02.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>17.04.2018, COM</term>
		/// <description> Reports vom KP_Include laden. </description></item>
		/// <item><term>16.06.2018, COM</term>
		/// <description>Anpassung für Differenzierung User und Standard Reports. </description></item>
		/// </list></remarks>
		//////////////////////////////////////////////////////////////////////////////////

		private void LoadCmbReportTemplates( IErrorHandler errorHandler )
		{
			try
			{
				if( Directory.Exists( REPORT_PATH ) && Directory.GetFiles(REPORT_PATH).Length > 0 )
				{
					var folder = new DirectoryInfo( REPORT_PATH );
					if( folder.Exists && folder.GetFileSystemInfos( ).Length > 0 )
					{
						//User Reports
						string[] files = System.IO.Directory.GetFiles( REPORT_PATH );

						foreach( String file in files )
						{
							FileInfo fileInfo = new FileInfo( file );
							RAG_ReportTemplate reportTemplate = new RAG_ReportTemplate( fileInfo );
							reportTemplates.Add( reportTemplate );
						}

						if( this.cmbReportVorlage.Items.Count > 0 )
							this.cmbReportVorlage.Items.Clear( );

						if( REPORT_PATH == STANDARD_REPORT_PATH )
							this.cmbReportVorlage.Items.Add( "---------- STANDARD REPORTS ----------" );
						else
							this.cmbReportVorlage.Items.Add( "---------- USER REPORTS ----------" );

						foreach( RAG_ReportTemplate reportTemplate in reportTemplates )
						{
							if( reportTemplate.FileInfo.Extension.Equals(".rdl"))
								this.cmbReportVorlage.Items.Add( reportTemplate );
						}

						// If User has selected an user specific report path, add standard reports anyway..
						if( REPORT_PATH != STANDARD_REPORT_PATH )
						{
							//add seperator dummy object
							this.cmbReportVorlage.Items.Add( String.Empty );
							this.cmbReportVorlage.Items.Add( "---------- STANDARD REPORTS ----------" );
							int startIndexOfStandardReports = reportTemplates.Count;

							//Standard Reports
							files = System.IO.Directory.GetFiles( STANDARD_REPORT_PATH );

							foreach( String file in files )
							{
								FileInfo fileInfo = new FileInfo( file );
								RAG_ReportTemplate reportTemplate = new RAG_ReportTemplate( fileInfo );
								reportTemplates.Add( reportTemplate );
							}

							for( int i = startIndexOfStandardReports; i < reportTemplates.Count; i++ )
							{
								if( reportTemplates[ i ].FileInfo.Extension.Equals( ".rdl" ) )
									this.cmbReportVorlage.Items.Add( reportTemplates[ i ] );
							}
						}
					}
				}
				else
				{
					errorHandler.SetError( this, "ReportControl: Report Folder is Missing", ErrorStates.Info );
					errorHandler.ShowError( );
				}
			}
			catch( Exception ex)
			{
				errorHandler.SetError( this, "ReportControl: I/O Error Reports", new object[] { ex.Message }, ErrorStates.Error );
				errorHandler.ShowError( );
			}
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// LoadCmbStation( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Füllt die Station Combobox mit allen Riflex Stationen.  
		/// </summary>
		/// <param name="errorHandler"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>05.03.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// <item><term>13.03.2018, POA</term>
		/// <description>	Nur RIFLEX Stationen</description></item>
		/// <item><term>20.03.2018, COM</term>
		/// <description>reverse loop, liste wird während Iteration geändert.</description></item>
		/// </list></remarks>
		/////////////////////////////////////////////////////////////////////////////////

		private void LoadCmbStation( IErrorHandler errorHandler )
		{
			ICBusinessLayer businessLayer = CBusinessLayer.GetBusinessLayer( );
			IList<IBO_Station> stationen = businessLayer.GetStationen( errorHandler );

			for( int i = stationen.Count - 1; i >= 0; i-- )
			{
				if( !stationen[ i ].IsRiflexM1 )
					stationen.RemoveAt( i );
			}

			if( stationen.Count > 0 )
			{
				this.cmbStation.Items.Clear( );
				this.cmbStation.Items.Add( "---------- ALL STATIONS ----------" );

				foreach( IBO_Station station in stationen )
				{
					this.cmbStation.Items.Add( station );
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// cmbReportVorlage_SelectionChangeCommitted( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SelectionChangeCommitted - Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>16.06.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		/////////////////////////////////////////////////////////////////////////////////

		private void cmbReportVorlage_SelectionChangeCommitted( object sender, EventArgs e )
		{
			if( this.cmbReportVorlage.SelectedItem.GetType( ) != typeof( RAG_ReportTemplate ) )
				this.cmbReportVorlage.SelectedItem = null;
		}


		//////////////////////////////////////////////////////////////////////////////////
		//// cmbStation_SelectionChangeCommitted( )
		//////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SelectionChangeCommitted - Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks><list type="table">
		/// <listheader>Änderungen</listheader>
		/// <item><term>16.06.2018, COM</term>
		/// <description>	Erstellung</description></item>
		/// </list></remarks>
		/////////////////////////////////////////////////////////////////////////////////
		
		private void cmbStation_SelectionChangeCommitted( object sender, EventArgs e )
		{
			if( this.cmbStation.SelectedItem.GetType( ) != typeof( BO_Station ) )
				this.cmbStation.SelectedItem = null;
		}
	}

}
