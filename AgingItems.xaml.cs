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
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for AgingItems.xaml
    /// </summary>
    public partial class AgingItems : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView agingItemsRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery; //allows the query generated for the report to be used when exporting to excel

        //We select All systems with All statuses of all items that are aging on window load
        public AgingItems(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            SystemComboBox.SelectedIndex = 0;
            FillSystemComboBox();
            ReportHelper.FillStatusComboBoxWithAll (StatusComboBox);
            StatusComboBox.SelectedIndex = 0;
            BindDataGrid();
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

        //WHenever the system and status combo boxes change, the datagrid re-binds
        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //DataScroll.Visibility = Visibility.Visible;
                AgingReport.Visibility = Visibility.Visible;
                //Excel Export button
                Export.Visibility = Visibility.Visible;
                FullHistory.Visibility = Visibility.Collapsed; //Hides full history by default when a system is chosen
                BindDataGrid();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }

        //queries are customized based on what choices the user makes from the comboboxes
        public string agingQuery()
        {
            string query;
            string statusChosen = ReportHelper.StatusChosen(StatusComboBox);
             if ((SystemComboBox.SelectedIndex != 0) && ((ReportHelper.StatusChosen(StatusComboBox)) == "All Opened" || (ReportHelper.StatusChosen(StatusComboBox) == "All Closed")))
            {
                query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22))OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8)) " +
                           "OR ((Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 14)))) AND (Sys_Impact LIKE '%" + SystemChosen() + "%') " + StatusQuery() + " ORDER BY TaskNum ASC; ";
            }

            else if (SystemComboBox.SelectedIndex != 0 && ((ReportHelper.StatusChosen(StatusComboBox) != "All Opened") && (ReportHelper.StatusChosen(StatusComboBox) != "All Closed")))
            {
                query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 14)) OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " +
                           "AND New_Issues.[Status] = '" + ReportHelper.StatusChosen(StatusComboBox) + "' AND Sys_Impact LIKE '%" + SystemChosen() + "%' ORDER BY TaskNum ASC; ";
            }

            else if(SystemComboBox.SelectedIndex == 0 && (ReportHelper.StatusChosen(StatusComboBox) != "All Opened") && (ReportHelper.StatusChosen(StatusComboBox) != "All Closed"))
            {
                query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 14)) OR (Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " +
                           "AND New_Issues.[Status] = '" + ReportHelper.StatusChosen(StatusComboBox) + "' ORDER BY TaskNum ASC; ";
            }
            else 
            {
                query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 14)) OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " + StatusQuery() + " ORDER BY TaskNum ASC;";                           
            }
            return query;
        }

        //Sets status based on if All Opened or All Closed is chosen
        private string StatusQuery()
        {
            if(ReportHelper.StatusChosen(StatusComboBox) == "All Opened")
            {
                return "AND(New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' " +
                    "AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') ";
            }
            else
            {
                return "AND(New_Issues.[Status] LIKE '%closed%' OR New_Issues.[Status] LIKE '%implemented%' OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%' OR New_Issues.[Status] NOT LIKE '%Completed%') ";
            }
        }

        //Bind information to DataGrid
        public void BindDataGrid()
        {
          string query = agingQuery();

            reportQuery = query;
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    DataTable Reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(Reports);
                        AgingReport.Visibility = Visibility.Visible;
                        AgingReport.ItemsSource = Reports.DefaultView;
                    }
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

        //View issues in edit view
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                agingItemsRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);
                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, agingItemsRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //returns the system currently chosen in comboBox in the form of a string
        private string SystemChosen()
        {
            return SystemComboBox.SelectedItem.ToString();
        }

        //Fills history table with most recent status for each issue
        private void FillHistoryTable(DataTable recentHistory)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    //put this in different function so excel export can use it, which returns a datatable
                    DataColumn dc2 = new DataColumn("EntryDate");
                    DataColumn dc3 = new DataColumn("LatestStatusNote");
                    DataColumn dc4 = new DataColumn("LatestStatus");

                    recentHistory.Columns.Add(dc2);
                    recentHistory.Columns.Add(dc3);
                    recentHistory.Columns.Add(dc4);

                    int taskNum;
                    using (SqlCommand IDCmd = new SqlCommand(agingQuery(), con))
                    {
                        using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                taskNum = reader2.GetInt32(10);
                                DataTable tabRecent = new DataTable();
                                tabRecent = ReportHelper.FillRow(taskNum);
                                if (tabRecent.Rows.Count > 0)
                                {
                                    recentHistory.ImportRow(tabRecent.Rows[0]);
                                }
                                else
                                {
                                    recentHistory.Rows.Add(null, null, null);
                                }
                            }
                            reader2.Close();
                        }
                        IDCmd.Dispose();
                    }
                }

                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("error");
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

        //exports report to excel
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

                    DataTable historyTable = new DataTable();
                    FillHistoryTable(historyTable);
                    Helper.ToExcelClosedXML(historyTable, reports);
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
    }
}