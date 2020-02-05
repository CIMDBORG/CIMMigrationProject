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

        //*******************************************************************
        // DESCRIPTION: Function that runs the Business Cases query and fills the data grid with the result table.
        //              First, the SELECT query is run to pull the data on the open items. 
        //              Then, a SQLDataAdapter is used to fill the datatable with these results. 
        //              See BusinessCases.xaml for more on data binding. Note that the names of the result columns
        //                  match the names of the binding columns. That is how the query result table is connected to the datagrid.
        //*******************************************************************
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

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        //*******************************************************************
        // DESCRIPTION: Runs when the user clicks the "Edit" button in one of the datagrid rows.
        //              On that button click, the data from that row of the datatable is pulled as a DataRowView object, named priorbySystemRow.
        //              An instance of the EditRecord form is then created, passing:
        //                      1) this page itself, which is so that the updates can be completed
        //                      2) login-based user data arr (string[] object)
        //                      3) prioritization-by-system data priorBySystemRow (DataRowView object)
        //              The user is then taken to the EditRecord form, where the data of that particular issue auto-populates the form.
        //*******************************************************************

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