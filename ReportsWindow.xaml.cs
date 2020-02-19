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
using System.Reflection;
using System.Runtime.InteropServices;
using SQL = System.Data;


namespace WpfApp1
{
    //ReportsWindow allows the user to chose a report and filter by system using the two comboBoxes at the top
    //Based on this, a specific query will be generated that will generate the report

    // Interaction logic for ReportsWindow.xaml
    public partial class ReportsWindow : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView reportRow;       //local variable to store the row of data in the from a specific row in the Report DataGrid
        private bool fullHistoryChosen = false; //hides the FullHistory DataGrid by default 
        private bool includeCIM;
        private bool includeStratTasks;

        //Fills comboBox information, sets default report to Aging Items/All Systems, generates query to produce report
        public ReportsWindow(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillReportComboBox();
            FillSystemComboBox(arr[7]);
            FillStatusComboBox();
            SystemComboBox.SelectedIndex = 0;
            ReportComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;
            //Collapses all DataGrids until a system is chosen from the Combo Box
            Report.Visibility = Visibility.Collapsed;
        }

        //*******************************************************************
        // DESCRIPTION: Parses the string containing the user's systems, delimited by '/',
        //                  and fills the System combobox with these various systems.
        //              This will become important as the system chosen here drives the results of the query on this page.
        //*******************************************************************

        //Fills the ReportComboBox with the appropriate report options that the user has
        private void FillReportComboBox()
        {
            ReportComboBox.Items.Add("Aging Items");
            ReportComboBox.Items.Add("Application Priority Report");
        }

        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("All");
            StatusComboBox.Items.Add("Pending");
            StatusComboBox.Items.Add("Active");
            StatusComboBox.Items.Add("App Review");
            StatusComboBox.Items.Add("BC Submitted");
            StatusComboBox.Items.Add("BC Approved");
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Report.Visibility = Visibility.Visible;
                //Excel Export button
                Export.Visibility = Visibility.Visible;
                FullHistory.Visibility = Visibility.Collapsed;
                HistoryRecent.Visibility = Visibility.Visible;
                BindDataGrid();
            }

            //This automatically makes a selection for the user if they do not choose an option from the reports combobox menu in order to avoid an exception

            catch (NullReferenceException)
            {
                StatusComboBox.SelectedIndex = 0;
            }
        }

        //Controls the visibility settings for the datagrids and buttons as the combo box is changed
        private void ReportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Report.Visibility = Visibility.Visible;
                //Excel Export button
                Export.Visibility = Visibility.Visible;
                SetStratTasksVis();
                FullHistory.Visibility = Visibility.Collapsed; //Hides full history by default when a system is chosen
                HistoryRecent.Visibility = Visibility.Visible; //DataGrid pops up with most recent statuses for each item by default
                BindDataGrid();
                if(ReportComboBox.SelectedIndex == 0)
                {
                    HistoryRecent.Width = 600;
                    
                }
                else
                {
                    HistoryRecent.Width = 490;
                }
            }
            catch (Exception ex)
            {
                ReportComboBox.SelectedIndex = 0;
                BindDataGrid();
            }
        }

        //fills the system combobox based on the systems that the current user logged in works in i.e CRIS, eBilling, etc.
        //everyone is allowed access to reports with issues in CIM

        private void FillSystemComboBox(string systemString)
        {
            if (arr[6] == "User")
            {
                char delimiter = '/';
                string[] sys = systemString.Split(delimiter);
                int len = sys.Length;
                SystemComboBox.Items.Add("All");
                for (int x = 0; x < len; x++)
                {
                    SystemComboBox.Items.Add(sys[x]);
                }
                SystemComboBox.Items.Add("CIM");
            }
            else
            {
                Helper.FillSystemComboBox(SystemComboBox);
            }
        }

        //accessor method that returns a string that says what current system the user has chosen from the SystemComboBox
        private string SystemChosen()
        {
            return SystemComboBox.SelectedItem.ToString();
        }

        //accessor method that returns a string that says the current report that has been chosen by the user
        private string ReportChosen()
        {
            return ReportComboBox.SelectedItem.ToString();
        }

        private void SetStratTasksVis()
        {
            if(ReportComboBox.SelectedItem.ToString() == "Application Priority Report")
            {
                StratCheckBox.IsEnabled = true;
                PriorityCheckBox.IsEnabled = true;
            }
            else
            {
                StratCheckBox.IsEnabled = false;
                PriorityCheckBox.IsEnabled = false;
            }
        }

        private string StatusChosen()
        {
            if (StatusComboBox.SelectedItem == null)
            {
                StatusComboBox.SelectedIndex = 0;
            }

            return StatusComboBox.SelectedItem.ToString();
        }

        //This refreshes the page when the system menu is changed so the appropriate issues are filtered based on the system chosen
        //Hides full history by default when a system is chosen
        //DataGrid pops up with most recent statuses for each item by default
        //Collapses the button that allows you to return to the recent history by default

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Report.Visibility = Visibility.Visible;
                //Excel Export button
                Export.Visibility = Visibility.Visible;
                FullHistory.Visibility = Visibility.Collapsed;
                HistoryRecent.Visibility = Visibility.Visible;
                BindDataGrid();
            }

            //This automatically makes a selection for the user if they do not choose an option from the reports combobox menu in order to avoid an exception

            catch (NullReferenceException)
            {
                ReportComboBox.SelectedIndex = 0;
                BindDataGrid();
            }
        }

        private void SetCIM()
        {
            if(CIMCheckBox.IsChecked.Value)
            {
                includeCIM = true;
            }
            else
            {
                includeCIM = false;
            }
        }

        private void SetStratTasks()
        {
            if (StratCheckBox.IsChecked.Value)
            {
                includeStratTasks = true;
            }
            else
            {
                includeStratTasks  = false;
            }
        }

        private string AppendTasks()
        {
            if(includeStratTasks)
            {
                return "";
            }
            else
            {
                return " AND (Category != 'Strategic Task') ";
            }
        }
    
        //this generates the appropriate report query needed based on what options are chosen in the dropdown menus

        private string GenerateReportQuery()
        {
            string stringQuery;

            if (ReportChosen() == "Application Priority Report" && SystemChosen() != "All" && StatusChosen() != "All")
            {
                stringQuery = "SELECT New_Issues.ID AS ID, New_Issues.Sys_Impact AS [System], New_Issues.Priority_Number, New_Issues.Category, New_Issues.TFS_BC_HDFS_Num AS BID, " +
                        "FORMAT(New_Issues.Opened_Date, 'MM/dd/yyyy') AS Opened_Date, " +
                        "FORMAT(New_Issues.Completed_Date, 'MM/dd/yyyy') as Completed_Date, New_Issues.Title AS Title, New_Issues.Supporting_Details AS Details, " +
                        "FORMAT(New_Issues.Due_Date, 'MM/dd/yyyy') as Due_Date, New_issues.Req_Name, " +
                        "New_Issues.[Status], New_issues.Sys_Impact " +
                        "FROM New_Issues " +
                        "WHERE New_Issues.[Status] = '" + StatusChosen() + "' AND New_Issues.Sys_Impact LIKE '%" + SystemChosen() + "%' " + AppendTasks() + IncludeLowPriority() + " GROUP BY Priority_Number, Sys_Impact, ID, Category, TFS_BC_HDFS_Num, Opened_Date, " +
                        "Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;; ";
            }

            else if (ReportChosen() == "Application Priority Report" && SystemChosen() == "All" && StatusChosen() != "All")
            {
                stringQuery = "SELECT New_Issues.ID AS ID, New_Issues.Sys_Impact AS [System], New_Issues.Priority_Number, New_Issues.Category, New_Issues.TFS_BC_HDFS_Num AS BID, " +
                        "FORMAT(New_Issues.Opened_Date, 'MM/dd/yyyy') AS Opened_Date, " +
                        "FORMAT(New_Issues.Completed_Date, 'MM/dd/yyyy') as Completed_Date, New_Issues.Title AS Title, New_Issues.Supporting_Details AS Details, " +
                        "FORMAT(New_Issues.Due_Date, 'MM/dd/yyyy') as Due_Date, New_issues.Req_Name, " +
                        "New_Issues.[Status], New_issues.Sys_Impact " +
                        "FROM New_Issues " +
                        "WHERE " + ReportHelper.AllSystemsQuery(arr[7], includeCIM) + IncludeLowPriority() + "AND New_Issues.[Status] = '" + StatusChosen() + AppendTasks() + "' GROUP BY Priority_Number, Sys_Impact, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, " +
                        "Req_Name, [Status] ORDER BY Sys_Impact ASC, Priority_Number ASC;; ";
            }

            else if (ReportChosen() == "Application Priority Report" && SystemChosen() == "All" && StatusChosen() == "All")
            {
                stringQuery = "SELECT New_Issues.ID AS ID, New_Issues.Sys_Impact AS [System], New_Issues.Priority_Number, New_Issues.Category, New_Issues.TFS_BC_HDFS_Num AS BID, " +
                        "FORMAT(New_Issues.Opened_Date, 'MM/dd/yyyy') AS Opened_Date, " +
                        "FORMAT(New_Issues.Completed_Date, 'MM/dd/yyyy') as Completed_Date, New_Issues.Title AS Title, New_Issues.Supporting_Details AS Details, " +
                        "FORMAT(New_Issues.Due_Date, 'MM/dd/yyyy') as Due_Date, New_issues.Req_Name, " +
                        "New_Issues.[Status], New_issues.Sys_Impact " +
                        "FROM New_Issues WHERE " + ReportHelper.AllSystemsQuery(arr[7], includeCIM) + IncludeLowPriority() + " AND (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') " + AppendTasks() +
                       "ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }

            else if (ReportChosen() == "Application Priority Report" && SystemChosen() != "All" && StatusChosen() == "All")
            {
                stringQuery = "SELECT New_Issues.ID AS ID, New_Issues.Sys_Impact AS [System], New_Issues.Priority_Number, New_Issues.Category, New_Issues.TFS_BC_HDFS_Num AS BID, " +
                        "FORMAT(New_Issues.Opened_Date, 'MM/dd/yyyy') AS Opened_Date, " +
                        "FORMAT(New_Issues.Completed_Date, 'MM/dd/yyyy') as Completed_Date, New_Issues.Title AS Title, New_Issues.Supporting_Details AS Details, " +
                        "FORMAT(New_Issues.Due_Date, 'MM/dd/yyyy') as Due_Date, New_issues.Req_Name, " +
                        "New_Issues.[Status], New_issues.Sys_Impact " +
                        "FROM New_Issues " +
                        "WHERE New_Issues.Sys_Impact LIKE '%" + SystemChosen() + "%' " + IncludeLowPriority() + " AND (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%')" + AppendTasks() +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }

            else if (ReportChosen() == "Aging Items" && SystemChosen() != "All" && StatusChosen() != "All")
            {
                /*selects all items from New Issues where Category "BC/TI" and the last status update was more than 180 days ago, SELECT CAST(GETDATE() AS DATE)
                Impact "Billed Items" and the last status update was more than 22 days ago, and Impact "Not Billed Items" and 
                last status update was more than 8 days ago.*/

                stringQuery = "SELECT New_Issues.ID AS ID, Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Priority_Number, Category, TFS_BC_HDFS_Num as BID, Impact, " +
                        "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                        "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Opened_Date,  " +
                        "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                        "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                        "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 7)) " +
                        "OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " +
                        "AND New_Issues.[Status] = '" + StatusChosen() + "' AND Sys_Impact LIKE '%" + SystemChosen() + "%'" +
                         " GROUP BY Priority_Number, Sys_Impact, Assigned_To, Impact, Latest_Status_Update, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;"; 
            }

            else if (ReportChosen() == "Aging Items" && SystemChosen() != "All" && StatusChosen() == "All")
            {
                /*selects all items from New Issues where Category "BC/TI" and the last status update was more than 180 days ago, SELECT CAST(GETDATE() AS DATE)
                Impact "Billed Items" and the last status update was more than 22 days ago, and Impact "Not Billed Items" and 
                last status update was more than 8 days ago.*/

                stringQuery = "SELECT New_Issues.ID AS ID, Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Priority_Number, Category, TFS_BC_HDFS_Num as BID, Impact, " +
                        "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                        "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Opened_Date,  " +
                        "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                        "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                        "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 7)) " +
                        "OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " +
                        "AND (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') " +
                        "AND New_Issues.Sys_Impact = '" + SystemChosen() + "' " +
                         " GROUP BY Priority_Number, Sys_Impact, Assigned_To, Impact, Latest_Status_Update, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }
            else if (ReportChosen() == "Aging Items" && SystemChosen() == "All" && StatusChosen() == "All")
            {
                if (arr[6] == "User")
                {
                    stringQuery = "SELECT New_Issues.ID AS ID, Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Priority_Number, Category, TFS_BC_HDFS_Num as BID, Impact, " +
                       "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                       "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Opened_Date,  " +
                       "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                       "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                       "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 7)) " +
                       "OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) AND " +
                       ReportHelper.AllSystemsQuery(arr[7], includeCIM) + " AND (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') " + 
                        " GROUP BY Priority_Number, Sys_Impact, Assigned_To, Impact, Latest_Status_Update, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;";                       
                }
                else
                {
                    stringQuery = "SELECT New_Issues.ID AS ID, Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Priority_Number, Category, TFS_BC_HDFS_Num as BID, Impact, " +
                       "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update,  " +
                       "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Opened_Date,  (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                       "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                       "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                       "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 7)) " +
                       "OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) AND " +
                       "(New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') " + 
                         " GROUP BY Priority_Number, Sys_Impact, Assigned_To, Impact, Latest_Status_Update, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;";
                }
            }
            else
            {
                /*selects all items from New Issues where Category "BC/TI" and the last status update was more than 180 days ago, SELECT CAST(GETDATE() AS DATE)
                Impact "Billed Items" and the last status update was more than 22 days ago, and Impact "Not Billed Items" and 
                last status update was more than 8 days ago.*/

                stringQuery = "SELECT New_Issues.ID AS ID, Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Priority_Number, Category, TFS_BC_HDFS_Num as BID, Impact, (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE()))) as Open_Days, " +
                        "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update,  (SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Opened_Date,  " +
                        "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update " +
                        "FROM History " +
                        "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                        "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22)) OR (Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 7)) " +
                        "OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) AND " +
                        ReportHelper.AllSystemsQuery(arr[7], includeCIM) + " AND (New_Issues.[Status] = '" + StatusChosen() + "') " +
                         " GROUP BY Priority_Number, Sys_Impact, Assigned_To, Impact, Latest_Status_Update, ID, Category, TFS_BC_HDFS_Num, Opened_Date, Completed_Date, Title,Supporting_Details, Due_Date, Req_Name, [Status] " +
                        "ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }
            
            return stringQuery;
        }

        //this is a query that determines whether or not items with a priority over 300 are shown
        private string IncludeLowPriority()
        {
            if (PriorityCheckBox.IsChecked.Value.ToString() == "True")
            {
                return "";
            }
            else
            {
                return "AND (Priority_Number < 300)";
            }
        }


        //Fills the report datagrid with the appropriate information from SQL using data binding 

        private void FillReportTable(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(GenerateReportQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(table);
                        if (ReportChosen() != "Aging Items")
                        {
                            Report.Visibility = Visibility.Visible;
                            Report.ItemsSource = table.DefaultView;
                            AgingReport.Visibility = Visibility.Collapsed;
                        }

                        else
                        {
                            Report.Visibility = Visibility.Collapsed;
                            AgingReport.ItemsSource = table.DefaultView;
                            AgingReport.Visibility = Visibility.Visible;
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    if (ReportChosen() == "Aging Items")
                    {
                        MessageBox.Show("No Aging Items For " + SystemChosen());
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



    //Fills the right side of the table (the recent history datagrid)

            private DataTable FillRow(int taskNum)
            {
            string mostRecent = "SELECT TOP 1 TaskNum, CONVERT(date, EntryDate) AS EntryDate, New_StatusNote as LatestStatusNote, [Status] AS LatestStatus FROM History " +
                                "WHERE TaskNum = " + taskNum +
                                " ORDER BY EntryDate DESC;";
            DataTable historyRow = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand recentCmd = new SqlCommand(mostRecent, con);
                using (SqlDataAdapter sda = new SqlDataAdapter(recentCmd))
                {
                    sda.Fill(historyRow);
                }
            }
            return historyRow;
        }


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
                    using (SqlCommand IDCmd = new SqlCommand(GenerateReportQuery(), con))
                    {
                        using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                taskNum = reader2.GetInt32(0);
                                DataTable tabRecent = new DataTable();
                                tabRecent = FillRow(taskNum);
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
                        HistoryRecent.ItemsSource = recentHistory.DefaultView;
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

        //Binds both the report and history datatables with queries to display recent history for each item in the report
        //Also binds the table that shows the full history for each item, using the appropriate supporting methods
       

        public void BindDataGrid()
        {
            string query = GenerateReportQuery();

            //Fills the reports table (or the aging items table if aging items is chosen)
            DataTable reports = new DataTable();
            FillReportTable(reports);

            //Fills the recent history table to be displayed to user
            DataTable history = new DataTable();
            FillHistoryTable(history);
        } 
        
        //loads the data into the history grid so the user sees it in the front-end when they want
        private void BindHistoryGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    //Query that generates individual full status history, to be displayed if the user wishes
                    string query = "SELECT format(EntryDate, 'MM/dd/yyyy') AS EntryDateHistory, New_StatusNote AS NewStatus, [Status] AS History_Status, History.TaskNum AS TaskNum " +
                                   "FROM History WHERE TaskNum = " + TaskNum + " AND New_StatusNote != 'Aging' ORDER BY History.EntryDate DESC;";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
                    FullHistory.ItemsSource = dt.DefaultView;
                }


                catch(IndexOutOfRangeException)
                {
                    MessageBox.Show("No Aging Items to Update for this System");
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            //Only displays the full history table if the user clicks the Status History button
        }

        //Allows the user to access the past report statuses based on the task num that is clicked
        private void History_Click(object sender, RoutedEventArgs e)
        {
            reportRow = (DataRowView)((Button)e.Source).DataContext; //Retrieves the information for the row that the user clicks
            BindHistoryGrid(reportRow["ID"].ToString()); //binds full status history for row clicked
            if (fullHistoryChosen)
            {
                FullHistory.Visibility = Visibility.Collapsed;
                HistoryRecent.Visibility = Visibility.Visible;
                fullHistoryChosen = false;
            }

            else
            {
                HistoryRecent.Visibility = Visibility.Collapsed; //Collapses default view 
                FullHistory.Visibility = Visibility.Visible; //displays full status history DataGrid
                fullHistoryChosen = true;
            }
        }

        //This exports the report to Excel
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            //On Excel Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
            //rowClickedInfo = (DataRowView)((Button)e.Source).DataContext;
            //Generates an empty excel document 
           
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(GenerateReportQuery(), con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable reports = new DataTable();

                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(reports);
                    }

                    Helper.ToExcelClosedXML(reports);
                    
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

        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                reportRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDArray = Helper.FillIDList(GenerateReportQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //The following methods allow for the scrolling in both the history and report datagrids to be kept in sync
        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
        private void Lbx1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void Report_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer3 = GetDescendantByType(AgingReport, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            _listboxScrollViewer3.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void Aging_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(AgingReport, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        private void CIMCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetCIM();
            BindDataGrid();
        }

        private void StratCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetStratTasks();
            BindDataGrid();
        }

        private void PriorityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            BindDataGrid();
        }
    }
}
