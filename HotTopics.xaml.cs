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
   
    public partial class HotTopics : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //holds login-based user data
        private DataRowView hotTopicsRow;           //holds the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery;                 //holds the SQL query


        /*Name: Dominick Carrubba (ppz7tzh) 
        Function Name: HotTopics
        Purpose: HotTopics Constructor to populate items in dropdown box for the data grid. 
        Parameters: string[] user_data 
        Return Value: N/A
        Local Variables: None
        Algorithm: Assigns global variables based on values passed by parameters in the constructor, calls methods
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public HotTopics(string[] user_data)        
        {
            InitializeComponent();
            arr = user_data;
            Helper.FillSystemComboBox(SystemComboBox);
            SystemComboBox.SelectedIndex = 0;
            BindDataGrid();
        }

        /*Name: Dominick Carrubba (ppz7tzh) 
        Function Name: BindDataGrid
        Purpose: HotTopics Constructor to set query to populate data in the DataGrid based on dropdown selection. 
        Parameters: string[] user_data 
        Return Value: query 
        Local Variables: query
        Algorithm: If selection from combobox is All, select all issues, if else populate based on critera selected in the combobox. Populate selected data in the Data Grid. 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid()
        {
            string query;
            if (SystemComboBox.SelectedItem.ToString() == "All")
            {
                 query = "SELECT New_Issues.Sys_Impact as [System], New_Issues.Assigned_To AS[Owner], New_Issues.TFS_BC_HDFS_Num as BID, New_Issues.Impact, New_Issues.Supporting_Details, " +
                            "New_Issues.Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                            "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, h1.TaskNum, New_Issues.ID as ID " +
                            "FROM New_Issues " +
                            "LEFT JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History GROUP BY TaskNum) h1 " +
                            "ON h1.TaskNum = New_Issues.ID " +
                            "WHERE Hot_Topic = 1 " +
                            "ORDER BY New_Issues.Priority_Number ASC;";
                reportQuery = query;
            }
            else
            {
                 query = "SELECT New_Issues.Sys_Impact as [System], New_Issues.Assigned_To AS[Owner], New_Issues.TFS_BC_HDFS_Num as BID, New_Issues.Impact, New_Issues.Supporting_Details, " +
                            "New_Issues.Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                            "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, h1.TaskNum, New_Issues.ID as ID " +
                            "FROM New_Issues " +
                            "LEFT JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History GROUP BY TaskNum) h1 " +
                            "ON h1.TaskNum = New_Issues.ID " +
                            "WHERE (Hot_Topic = 1) AND Sys_Impact = '" + ReportHelper.SystemChosen(SystemComboBox) + "' " +
                            "ORDER BY Priority_Number ASC;";
                reportQuery = query;
            }
           
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
                        HotTopicsReport.Visibility = Visibility.Visible;
                        HotTopicsReport.ItemsSource = Reports.DefaultView;
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


        /*Name: Dominick Carrubba (ppz7tzh) 
        Function Name: FillHistoryTable 
        Purpose: Fills history table with most recent status for each issue
        Parameters: DataTable recentHistory 
        Return Value: string plannedDate
        Local Variables: EntryDate - Date of entry , LatestStatusNote - most recent status text , LatestStatus - date of latest status
        Algorithm: Open the connection to fetch statuses for issue, populate most recent status details.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillHistoryTable(DataTable recentHistory)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    DataColumn dc2 = new DataColumn("EntryDate");
                    DataColumn dc3 = new DataColumn("LatestStatusNote");
                    DataColumn dc4 = new DataColumn("LatestStatus");

                    recentHistory.Columns.Add(dc2);
                    recentHistory.Columns.Add(dc3);
                    recentHistory.Columns.Add(dc4);
                                        int taskNum;
                    using (SqlCommand IDCmd = new SqlCommand(reportQuery, con))
                    {
                        using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                for (int i = 0; i < reader2.FieldCount; i++)
                                {
                                    if (reader2.GetName(i) == "ID")
                                    {
                                        taskNum = reader2.GetInt32(i);

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
                                }
                            }
                            reader2.Close();
                        }
                        IDCmd.Dispose();
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


        /*Name: Dominick Carrubba (ppz7tzh) 
       Function Name: EditButton_Click
       Purpose: Event handler for the Edit Form View
       Parameters: object sender, RoutedEventArgs e
       Return Value: N/A
       Local Variables: editRecord, hotTopicsRow, MessageBox
       Algorithm: If the Edit button is clicked, then the selected row from the table is populated in the Edit Form
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                hotTopicsRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, hotTopicsRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        /*Name: Dominick Carrubba (ppz7tzh) 
       Function Name: Export_Click
       Purpose: Event Handler - Generates an Excel document populated with the data populated in the DataGrid 
       Parameters: object sender, RoutedEventArgs e
       Return Value: None
       Local Variables: Microsoft.Office.Interop.Excel.Application excel ( excel document ),  Microsoft.Office.Interop.Excel.Workbook wb = null (Empty worksheet within docuement) 
       Algorithm: When event is triggered, pull the query generated in BindDataGrid to populate the data in the generated excel worksheet. 
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            //Generates an empty excel document 
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;

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

                    DataTable history = new DataTable();
                    FillHistoryTable(history);
                    Helper.ToExcelClosedXML(history, reports);
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

        /*Name: Dominick Carrubba (ppz7tzh) 
       Function Name: SystemComboBox_SelectionChange
       Purpose: Relay information in the data grid based on the changed selection in the combo box
       Parameters: object sender, SelectionChangedEventArgs e
       Return Value: none
       Local Variables: none
       Algorithm: When the Combo Box selection is changed -> relay change in DataGrid based on selection in ComboBox
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }
    }
}