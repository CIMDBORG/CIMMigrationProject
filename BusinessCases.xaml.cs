using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    public partial class BusinessCases : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView BusinessCasesRow;       //local variable to store the row of data in the 'Business Cases' DataGrid
        private string reportQuery;

        /*Name: Michael Figueroa
        Function Name: BusinessCases
        Purpose: Constructor for the BusinessCases form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: Fills SystemComboBox, then calls FillStatusComboBoxWithAll, assigns both combo boxes to index 0 ("All"), then calls BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public BusinessCases(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillSystemComboBox();
            ReportHelper.FillStatusComboBoxWithAll(StatusComboBox);
            SystemComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
            //Collapses all DataGrids until a system is chosen from the Combo Box

        }

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds data to DataGrid
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: If system chosen is "All", no sys_impact in WHERE clause of query; else, query has sys_Impact = sys; ReportQuery is = query and used for excel export (this may no 
        longer be necessary), then standard SQL-DataTable procedures follow, and DataTable is used to bind to DataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - Would like this method simplified or eliminated all-together at some point as part of code cleanup
        Assistance Received: N/A
        */
        public void BindDataGrid(string sys)
        {
            string query;
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    if (ReportHelper.SystemChosen(SystemComboBox) == "All")
                    {
                        query = "SELECT ID, Priority_Number, Sys_Impact as [System], Category, Req_Name as Req, TFS_BC_HDFS_Num as BID_ID, " +
                                    "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy') AS Opened_Date, [Status], Title, " +
                                    "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as [Days] " +
                                    "FROM New_Issues WHERE (Category LIKE 'BC%') AND " + StatusString() +
                                    "ORDER BY Priority_Number ASC;";
                    }
                 
                    else
                    {
                         query = "SELECT ID, Priority_Number, Sys_Impact as [System], Category, Req_Name as Req, TFS_BC_HDFS_Num as BID_ID, " +
                                    "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy') AS Opened_Date, [Status], Title, " +
                                    "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as [Days] " +
                                    "FROM New_Issues WHERE (Sys_Impact LIKE '%" + sys + "%') AND (Category LIKE 'BC%') AND " + StatusString() +
                                    "ORDER BY Priority_Number ASC;";
                    }

                    reportQuery = query;

                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);

                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                    Report.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
        }

        /*Name: Michael Figueroa
        Function Name: StatusString
        Purpose: To set the [Status] condition in the WHERE clause of the BusinessCases query based on what option is chosen from the ComboBox
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: If "All Opened" chosen from StatusComboBox, then the issues that are not closed/implemented/dropped/deferred/or BC Approved are chosen; else, closed issues are 
        chosen
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string StatusString()
        {
            if (ReportHelper.StatusChosen(StatusComboBox) == "All Opened")
            {
                return "(New_Issues.[Status] NOT LIKE '%closed%' " +
                       "AND New_Issues.[Status] NOT LIKE '%Implemented%' " +
                       "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%BC Approved%') ";
            }
            else if (ReportHelper.StatusChosen(StatusComboBox) == "All Closed")
            {
                return "(New_Issues.[Status] LIKE '%closed%' " +
                       "OR New_Issues.[Status] LIKE '%Implemented%' " +
                       "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%' OR New_Issues.[Status] LIKE '%BC Approved%') ";
            }
            else
                return "(New_Issues.[Status] = '" + ReportHelper.StatusChosen(StatusComboBox) + "') ";
        }

       /*Name: Michael Figueroa
       Function Name: FillSystemComboBox
       Purpose: Fills SystemComboBox - this method may not be necessary as helper has the same method
       Parameters: None
       Return Value: None
       Local Variables: None
       Algorithm: None 
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20 - Will eliminate as part of code cleanup
       Assistance Received: N/A
       */
        private void FillSystemComboBox()
        {
            SystemComboBox.Items.Add("All");
            SystemComboBox.Items.Add("ABR");
            SystemComboBox.Items.Add("BAT");
            SystemComboBox.Items.Add("BFR");
            SystemComboBox.Items.Add("BIS");
            SystemComboBox.Items.Add("BRRS");
            SystemComboBox.Items.Add("BWS");
            SystemComboBox.Items.Add("CDC");
            SystemComboBox.Items.Add("CIM");
            SystemComboBox.Items.Add("CRIS");
            SystemComboBox.Items.Add("DOC");
            SystemComboBox.Items.Add("EBA");
            SystemComboBox.Items.Add("EBCM");
            SystemComboBox.Items.Add("eBilling");
            SystemComboBox.Items.Add("EDI");
            SystemComboBox.Items.Add("FBR");
            SystemComboBox.Items.Add("FCB");
            SystemComboBox.Items.Add("IB");
            SystemComboBox.Items.Add("IFA");
            SystemComboBox.Items.Add("MDC");
            SystemComboBox.Items.Add("ODBI");
            SystemComboBox.Items.Add("PMC");
            SystemComboBox.Items.Add("PS");
            SystemComboBox.Items.Add("SOX");
            SystemComboBox.Items.Add("Vendor");
        }

        /*Name: Michael Figueroa
       Function Name: SystemComboBox_SelectionChanged
       Purpose: Event handler for when a new System is chosen from combobox
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: Calls BindDataGrid to refresh datagrid 
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        /*Name: Michael Figueroa
        Function Name: StatusComboBox_SelectionChanged
        Purpose: Event handler for when a new Status is chosen from combobox
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls BindDataGrid to refresh datagrid 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        /*Name: Michael Figueroa
        Function Name: Export_Click
        Purpose: Excel export event handler (this method will no longer exist after the excel export method is moved to Helper class)
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable reports, DataTable historyTable
        Algorithm: reports and historyTable DataTables are filled, then the helper ToExcelClosedXML method completes the export.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - Mike would like to eliminate as part of code cleanup
        Assistance Received: N/A
        */

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(reportQuery, con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(reports);
                    }

                    DataTable sortedRecent = new DataTable();
                    ReportHelper.FillHistoryTable(sortedRecent, reportQuery);

                    Helper.ToExcelClosedXML(sortedRecent, reports);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }
        }

        /*Name: Michael Figueroa
        Function Name: EditButton_Click
        Purpose: Event handler for edit button click
        Parameters: Auto-generated
        Return Value: None
        Local Variables: DataRowView agingItemsRow
        Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - This method will be simplified by Mike at a later date
        Assistance Received: N/A
        */
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                BusinessCasesRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, BusinessCasesRow, IDList);
                editRecord.FormLabel.Text = "Business Cases";
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}