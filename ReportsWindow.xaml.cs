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
        private bool includeCIM; //bool that indicates whether to include CIM issues on report
        private bool includeStratTasks; //bool that indicates whether to include Strategic Tasks on report

        /*Name: Michael Figueroa
        Function Name: ReportsWindow
        Purpose: Constructor for ReportsWindow
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls FillReportComboBox, FillSystemComboBox, FillStatusComboBox, sets all three comboboxes to index 0,
        Collapses Report DataGrid, because aging items report is first report that pops up when form loads
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
            Report.Visibility = Visibility.Collapsed;
        }

        //*******************************************************************
        // DESCRIPTION: Parses the string containing the user's systems, delimited by '/',
        //                  and fills the System combobox with these various systems.
        //              This will become important as the system chosen here drives the results of the query on this page.
        //*******************************************************************

        /*Name: Michael Figueroa
        Function Name: FillReportComboBox
        Purpose: Fills the ReportComboBox with the appropriate report options that the user has
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillReportComboBox()
        {
            ReportComboBox.Items.Add("Aging Items");
            ReportComboBox.Items.Add("Application Priority Report");
        }

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBox
        Purpose: Fills the StatusComboBox with the appropriate status options
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: None - This is no longer needed, same method is in ReportHelper.cs
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("All");
            StatusComboBox.Items.Add("Pending");
            StatusComboBox.Items.Add("Active");
            StatusComboBox.Items.Add("App Review");
            StatusComboBox.Items.Add("BC Submitted");
            StatusComboBox.Items.Add("BC Approved");
        }

        /*Name: Michael Figueroa
        Function Name: StatusComboBox_SelectionChanged
        Purpose: Event handler for StatusComboBox selectionChanged
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: Calls BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
                ReportComboBox.SelectedIndex = 0;
                SystemComboBox.SelectedIndex = 0;
            }
        }

        /*Name: Michael Figueroa
        Function Name: ReportComboBox_SelectionChanged
        Purpose: Event handler for StatusComboBox selectionChanged;Controls the visibility settings for the datagrids
        and buttons as the combo box is changed
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: Calls BindDataGrid; if the report chosen is agingItems, then the HistoryRecent column width is set at 600;
        else, it is set to 490. 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
                StatusComboBox.SelectedIndex = 0;
                ReportComboBox.SelectedIndex = 0;
                SystemComboBox.SelectedIndex = 0;
                BindDataGrid();
            }
        }

        /*Name: Michael Figueroa
        Function Name: FillSystemComboBox
        Purpose: fills the system combobox based on the systems that the current user logged in works in i.e CRIS, eBilling, etc.
        Parameters: string systemString
        Return Value: N/A
        Local Variables: char delimiter, string[] sys, int len
        Parameters: None
        Algorithm: if arr[6] equals user, Fills SystemComboBox with the systems that user is assigned to in Systems column
        in New_Contacts table.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SystemChosen
        Purpose: accessor method that returns a string that says what current system the user has chosen from the SystemComboBox
        Return Value: string
        Local Variables: None
        Parameters: None
        Algorithm: None - This method may not even be necessary as it is already in ReportHelper.cs
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string SystemChosen()
        {
            return SystemComboBox.SelectedItem.ToString();
        }

        /*Name: Michael Figueroa
        Function Name: ReportChosen
        Purpose: accessor method that returns a string that says the current report that has been chosen by the user
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: if ReportComboBox.SelectedItem = null, its index is set to 0 in order to avoid a nullreferenceexception
        Version: 3.0.0.2
        Date modified: 3/18/20
        Assistance Received: N/A
        */
        private string ReportChosen()
        {
            if (ReportComboBox.SelectedItem == null)
            {
                ReportComboBox.SelectedIndex = 0;
            }
            return ReportComboBox.SelectedItem.ToString();
        }

        /*Name: Michael Figueroa
        Function Name: SetStratTasksVis
        Purpose: Sets visibility both StratCheckBox and PriorityCheckBox
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: If the report chosen is Application Priority Report, then the CheckBoxes are enabled; else, they are
        disabled. The reason is that the Aging Report does not utilize these checkboxes in any way
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: StatusChosen
        Purpose: Returns the current status chosen in the comboBox
        Return Value: string
        Local Variables: None
        Parameters: None
        Algorithm: if the selectedItem is null, selected index is set to 0; else, ToString value is returned
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string StatusChosen()
        {
            if (StatusComboBox.SelectedItem == null)
            {
                StatusComboBox.SelectedIndex = 0;
            }

            return StatusComboBox.SelectedItem.ToString();
        }

        /*Name: Michael Figueroa
        Function Name: SystemComboBox_SelectionChanged
        Purpose: Event handler for SystemComboBox SelectionChanged; refreshes the page.
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: Calls BindDataGrid in order to refresh datagrid; the catch part of the try-catch block avoids exception
        by setting ReportComboBox index to 0
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
                StatusComboBox.SelectedIndex = 0;
                ReportComboBox.SelectedIndex = 0;
                SystemComboBox.SelectedIndex = 0;
                BindDataGrid();
            }
        }

        /*Name: Michael Figueroa
        Function Name: SetCIM
        Purpose: Setter that determines whether CIM issues are included in the report or not
        Return Value: N/A
        Local Variables: None
        Parameters: None
        Algorithm: If the CIM Checkbox is checked, includeCIM is set to true; else it is false
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SetStratTasks
        Purpose: Setter that determines whether Strategic Tasks are included in the report or not
        Return Value: N/A
        Local Variables: None
        Paramters: None
        Algorithm: if StratCheckBox is checked, then strat tasks are included; else, they are not.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: AppendTasks
        Purpose: This is the part of the query in either report that determines whether Strategic Tasks are excluded or not
        Return Value: string N/A
        Local Variables: None
        Parameters: None
        Algorithm: if includeStratTasks is set to value of True, no strategic task condition is included in the query;
        else, Strategic Tasks are excluded
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: GenerateReportQuery
        Purpose: Returns the query that will be used to populate data in DataGrid
        Return Value: string stringQuery
        Local Variables: None
        Parameters: None
        Algorithm: if the report chosen is the application priority report and the comboboxes have values other than "All"
        chosen, then StatusChosen() and SystemChosen() are used to set Status and Sys_Impact conditions in the WHERE
        clause; when user choses All systems, then ReportHelper.AllSystemsQuery is used to set Sys_Impact condition in WHERE
        clause. When Status chosen is "All," then issues that have status that is not dropped, implemented, closed or deferred are
        queried. IncludeLowPriority and AppendTasks are also called to complete the query
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: IncludeLowPriority
        Purpose: This is the part of the query in either report that determines whether items with priority over 300
        are excluded or not
        Return Value: string
        Local Variables: None
        Parameters: None
        Algorithm: PriorityCheckBox is checked, then empty string is returned; else, string containing condition that 
        excludes issues with Priority_Number less than 300 is returned
        else, Strategic Tasks are excluded
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

       /*Name: Michael Figueroa
       Function Name: FillReportTable
       Purpose: Fills the report datagrid with the appropriate information from SQL using data binding
       Return Value: None
       Local Variables: SqlCommand cmd, SqlDataAdapter sda
       Parameters: DataTable table
       Algorithm: cmd and sda are used to fill DataTable table; after filled, if ReportChosen isn't aging items, Report
       datagrid is set to visible, data is binded to datagrid using table and ItemSource, and AgingReport is collapsed;
       else, Report is collapsed and AgingReport is Visible.
       If no aging items on report, exception is caught and user is notified.
       else, Strategic Tasks are excluded
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
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



        /*Name: Michael Figueroa
        Function Name: FillRow
        Purpose: Returns a DataTable consisting of one row with the most recent status for the issue
        Parameters: int taskNum
        Return Value: DataTable
        Local Variables: string mostRecent, historyRow 
        Algorithm: defines mostRecent with Query containing one row with most recent status for issue ID taskNum, 
        then SQL fills historyRow datatable which is then returned -this is a duplicate also contained in ReportHelper.cs, 
        can be deleted.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */    
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

        /*Name: Michael Figueroa
        Function Name: FillHistoryTable
        Purpose: Fills full history columns in datagrid; use for when someone wants to see mos recent status for each issue; this keeps the HistoryRecent and ManTasks dataGrids in sync
        Parameters: DataTable recentHistory
        Return Value: None
        Local Variables: int taskNum, DataTable tabRecent 
        Algorithm: Adds DataColumns to recentHistory table, then reads ManagerTasksQuery using reader, extracts each ID from each record in the query and assigns to taskNum, then calls FillRow with taskNum as a parameter; if the
        row count is 1, the row is added to recentHistory, else, nulls are added - Also in ReportHelper.cs, probably
        no longer needed in here
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

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds both the report and history datatables with queries to display recent history for each item in the report
        Also binds the table that shows the full history for each item, using the appropriate supporting methods
        Return Value: None
        Local Variables: int taskNum, DataTable tabRecent 
        Parameters: None
        Algorithm: Calls FillReportTable and FillHistoryTable
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid()
        {
            DataTable reports = new DataTable();
            FillReportTable(reports);

            DataTable history = new DataTable();
            FillHistoryTable(history);
        }

        /*Name: Michael Figueroa
        Function Name: BindHistoryGrid
        Purpose: loads the data into the history grid so the user sees it in the front-end when they want
        Return Value: None
        Local Variables: string query, SqlCommand command, DataTable dt, SqlDataAdapter sda 
        Parameters: string TaskNum
        Algorithm: Goes through typical Data binding methods using SqlCommand, SqlDataAdapter, and string query to fill
        DataTable dt and bind data to FullHistory
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void BindHistoryGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
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

        /*Name: Michael Figueroa
        Function Name: History_Click
        Purpose: Event handler for Toggle History button
        Return Value: None
        Local Variables: None 
        Parameters: Auto-Generated
        Algorithm: Calls BindHistoryGrid; everytime button is toggled, fullHistoryChosen value is toggled as well; this
        value determines the visibility of the FullHistory DataGrid. If fullHistoryChosen is true, the visibility of 
        FullHistory is collapsed, and fullHistoryChosen is toggled to false; else, FullHistory is expanded and 
        FullHistoryChosen is true
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: Export_Click
        Purpose: Excel export (this method will no longer exist after the excel export method is moved to Helper class
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable reports, DataTable historyTable
        Algorithm: reports and historyTable DataTables are filled, then the helper ToExcelClosedXML method completes the export.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: EditButton_Click
        Purpose: Event handler for edit button click
        Parameters: Auto-generated
        Return Value: None
        Local Variables: DataRowView agingItemsRow
        Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
       Function Name: GetDescendantByType
       Purpose: This method helps access the scrollview of a visual element - in this case, the visual element is a DataGrid, and the Type is a
       scrollviewer. This is needed so the History and ManTasks DataGrids are in sync.
       Parameters: Visual Element, Type type
       Return Value: Visual foundElement
       Local Variables: Visual visual, Visual foundElement
       Algorithm: if there is no Visual with name element, then null is returned; if element is the same Type as type, then the element is returned;
       credit user punker76 on Stack Overflow (https://stackoverflow.com/questions/10293236/accessing-the-scrollviewer-of-a-listbox-from-c-sharp)
       with method and for more details on algorithm.
       NOTE: This is also used in other windows such as ReportsWindow, so this may be better off in the helper.
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
        Function Name: lbx1_ScrollChanged
        Purpose: Method 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: _listboxScrollViewer1 and _listboxScrollViewer2
        Algorithm: ManTasks and HistoryRecent scrollviewers retrieved using GetDescendantByType; then vertical offset of _listboxScrollViewer2 is set to offset of _listboxScrollViewer1
        in order to keep DataGrids in sync when scrolling
        NOTE: This is also used in other windows such as ReportsWindow, so this may be better off in the helper.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Lbx1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: Report_ScrollChanged
        Purpose: Event handler for Report scrollChanged that keeps DataGrids in sync when scrolling 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: _listboxScrollViewer1 and _listboxScrollViewer2
        Algorithm: Report and HistoryRecent scrollviewers retrieved using GetDescendantByType; then vertical offset of _listboxScrollViewer2 is set to offset of _listboxScrollViewer1
        in order to keep DataGrids in sync when scrolling
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Report_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: HistoryRecent_ScrollChanged
        Purpose: Event handler for HistoryRecent scrollchanged that keeps DataGrids in sync when scrolling 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: _listboxScrollViewer1 and _listboxScrollViewer2
        Algorithm: Report and HistoryRecent scrollviewers retrieved using GetDescendantByType; then vertical offset of _listboxScrollViewer2 is set to offset of _listboxScrollViewer1
        in order to keep DataGrids in sync when scrolling
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(Report, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer3 = GetDescendantByType(AgingReport, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            _listboxScrollViewer3.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: Aging_ScrollChanged
        Purpose: Event handler for Aging scrollChanged that keeps DataGrids in sync when scrolling 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: _listboxScrollViewer1 and _listboxScrollViewer2
        Algorithm: Aging and HistoryRecent scrollviewers retrieved using GetDescendantByType; then vertical offset of _listboxScrollViewer2 is set to offset of _listboxScrollViewer1
        in order to keep DataGrids in sync when scrolling
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Aging_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer _listboxScrollViewer1 = GetDescendantByType(AgingReport, typeof(ScrollViewer)) as ScrollViewer;
            ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
            _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: CIMCheckBox_Click
        Purpose: Event handler for CIMCheckBox click; "refreshes" datagrid 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetCIM and BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CIMCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetCIM();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: StratCheckBox_Click
        Purpose: Event handler for StratCheckBox click; "refreshes" datagrid 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetStratTasks and BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void StratCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetStratTasks();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: StratCheckBox_Click
        Purpose: Event handler for PriorityCheckBox click; "refreshes" datagrid 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls indDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void PriorityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            BindDataGrid();
        }
    }
}