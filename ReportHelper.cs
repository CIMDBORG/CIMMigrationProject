using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Spreadsheet;  

namespace WpfApp1
{
    //This class contains methods that are used throughout the different report forms; specifically, queries
    class ReportHelper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config

        /*Name: Michael Figueroa
        Function Name: StatusChosen
        Purpose: Returns string value of StatusComboBox item that is currently chosen
        Parameters: ComboBox statusComboBox
        Return Value: string value of statusComboBox
        Local Variables: None
        Algorithm: if the statusComboBox selected item is not null, then ToString value is returned; else, index of
        combobox is set to 0 and the ToString value is returned
        NOTE: This may be a good method to use for all comboboxes in this project. Need to evaluate this further.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static string StatusChosen(ComboBox statusComboBox)
        {
            if (statusComboBox.SelectedItem != null)
            {
                return statusComboBox.SelectedItem.ToString();
            }
            else
            {
                statusComboBox.SelectedIndex = 0;
                return statusComboBox.SelectedIndex.ToString();
            }
        }

        /*Name: Michael Figueroa
        Function Name: AllSystemsQuery
        Purpose: This allows the user to query issues consisting of all their systems
        Parameters: string systems, bool includeCIM
        Return Value: string value sb.ToString()
        Local Variables: char delimiter, string[] sys, string stringQuery, StringBuilder sb
        Algorithm: string is split using delimiter, then stringQuery is instantiated (stringQuery is where the Sys_Impact
        condition is in the WHERE clause), then, for each system contained in array sys, if i == 0, no OR is appended, but
        the system contained in sys[0] is appended else, an OR is included along with value in sys[i].
        This algorithm avoids errors when running this query in sql. See Mike Fig for elaborate details.
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static string AllSystemsQuery(string systems, bool includeCIM)
        {
            char[] delimiter = new char[] { '/', ';', ',' };
            string[] sys = systems.Split(delimiter);
            string stringQuery = " (Sys_Impact = ";
            StringBuilder sb = new StringBuilder(stringQuery);
            for (int i = 0; i < sys.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append("'" + sys[i] + "' ");
                }
                else
                {
                    sb.Append("OR Sys_Impact = '" + sys[i] + "' ");
                }
            }
            if (includeCIM)
            {
                sb.Append("OR Sys_Impact = 'CIM') ");
            }
            else
            {
                sb.Append(") ");
            }

            return sb.ToString();
        }

        /*Name: Michael Figueroa
        Function Name: InsertIntoAging
        Purpose: Inserts a new record into the History table; this specifically inserts a status of "aging" for each record
        in the aging report. This is used specifically in AgingItems.xaml.cs
        Parameters: DataTable aging
        Return Value: None
        Local Variables: string insert
        Algorithm: aging.Rows[i].Field<int>("ID") retrieves the ID of the issue, that is inserted as the TaskNum; 
        insert query is executed by ExecuteNonQuery() and the record is added to the History table
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static void InsertIntoAging(DataTable aging)
        {
                using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                        for(int i = 0; i < aging.Rows.Count; i++)
                        {
                                string insert = "INSERT INTO History (TaskNum, New_StatusNote, Timestamp) VALUES (" + aging.Rows[i].Field<int>("ID") + ", 'Aging', GETDATE());";
                                SqlCommand insertCmd = new SqlCommand(insert, con);
                                insertCmd.ExecuteNonQuery();
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
        Function Name: FillAging
        Purpose: FillAging() fills the aging table when someone runs the aging report; this is used in AgingItems.xaml.cs
        Parameters: DataTable aging
        Return Value: None
        Local Variables: string insert
        Algorithm: aging.Rows[i].Field<int>("ID") retrieves the ID of the issue, that is inserted as the TaskNum; 
        insert query is executed by ExecuteNonQuery() and the record is added to the History table
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static DataTable FillAging()
        {
            DataTable aging = new DataTable();
            string query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE ((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 180)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 22))OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 8))) " +
                           "AND (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%') " +
                           "ORDER BY TaskNum ASC;";
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(aging);
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
            return aging;
        }

        /*Name: Michael Figueroa
        Function Name: OwnerAgingQuery
        Purpose: This gets the aging query for a specific user; this is used for the aging warning that you get in yellow
        on the menu screen when an issue needs to get updated
        Parameters: string owner
        Return Value: string query
        Local Variables: string query
        Algorithm: None
        Version: 3.0.0.1
        Date modified: January 2020
        Assistance Received: N/A
        */
        public static string OwnerAgingQuery(string owner)
        {
            string query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                           "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                           "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days, " +
                           "(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                           "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                           "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 178)) " +
                           "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 20)) OR(Impact LIKE '%Not Billed Items%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 6)) OR(Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 12))) " +
                           "AND(New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') AND (Assigned_To = '" + owner + "') " +
                           "ORDER BY TaskNum ASC;";
            return query;
        }
        /*Name: Michael Figueroa
        Function Name: FillAgingOwnerSpecific
        Purpose: this returns a datatable used so the user can edit their issues when clicking on AgingButton in
        UserMenu_Window.xaml.cs
        Parameters: string owner
        Return Value: DataTable aging
        Local Variables: DataTable aging, string query
        Algorithm: OwnerAgingQuery is called to assign value to string query which is used to execute SqlCommand cmd 
        which then fills aging using Sda.Fill(aging)
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */

        public static DataTable FillAgingOwnerSpecific(string owner)
        {
            DataTable aging = new DataTable();
            string query = OwnerAgingQuery(owner);
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(aging);
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
            return aging;
        }

        /*Name: Michael Figueroa
        Function Name: OwnerUpdatesReq
        Purpose: This is the query that determines if a user has any issues marked for and update
        Parameters: string owner
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static string OwnerUpdatesReq(string owner)
        {
            return "SELECT ID FROM New_Issues WHERE (Manager_Update_Bit = 1) AND (User_Update_Bit = 0) AND (Assigned_To = '" + owner + "') AND" +
                " (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%');";
        }

        /*Name: Michael Figueroa
        Function Name: FillUpdateRequired
        Purpose: this returns a datatable that contains the issues that managers have requested an update on
        Parameters: string owner
        Return Value: DataTable updateReq
        Local Variables: DataTable updateReq, string query
        Algorithm: OwnerUpdatesReq is called to assign value to string query which is used to execute SqlCommand cmd 
        which then fills updateReq using Sda.Fill(updateReq)
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static DataTable FillUpdateRequired(string owner)
        {
            DataTable updateReq = new DataTable();
            string query = OwnerUpdatesReq(owner);
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(updateReq);
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
            return updateReq;
        }

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBox
        Purpose: Adds values to ComboBox statusComboBox - this should be used instead of all FillStatusComboBox 
        methods in other forms
        Parameters: ComboBox statusComboBox 
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static void FillStatusComboBox(ComboBox statusComboBox)
        {
            statusComboBox.Items.Add("Pending");
            statusComboBox.Items.Add("Active");
            statusComboBox.Items.Add("App Review");
            statusComboBox.Items.Add("BC Submitted");
            statusComboBox.Items.Add("BC Approved");
        }

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBoxWithAll
        Purpose: Adds values to ComboBox statusComboBox, including the options for All Opened and All Closed
        Parameters: ComboBox statusComboBox 
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static void FillStatusComboBoxWithAll(ComboBox statusComboBox)
        {
            statusComboBox.Items.Add("All Opened");
            statusComboBox.Items.Add("All Closed");
            statusComboBox.Items.Add("Pending");
            statusComboBox.Items.Add("Active");
            statusComboBox.Items.Add("App Review");
            statusComboBox.Items.Add("BC Submitted");
            statusComboBox.Items.Add("BC Approved");
        }

        /*Name: Michael Figueroa
        Function Name: SystemChosen
        Purpose: Returns the current selectedItem in systemComboBox
        Parameters: ComboBox systemComboBox 
        Return Value: String
        Local Variables: None
        Algorithm: if the selected item is not "All', then the ToString value is returned; else, "All" is returned
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static string SystemChosen(ComboBox systemComboBox)
        {
            if (systemComboBox.SelectedItem.ToString() != "All")
            {
                return systemComboBox.SelectedItem.ToString();
            }

            else
            {
                return "All";
            }
        }

      /*Name: Michael Figueroa
      Function Name: FillRow
      Purpose: Returns a DataTable consisting of one row with the most recent status for the issue
      Parameters: int taskNum
      Return Value: DataTable
      Local Variables: string mostRecent, historyRow 
      Algorithm: defines mostRecent with Query containing one row with most recent status for issue ID taskNum, then SQL fills historyRow datatable which is then returned
      Version: 2.0.0.4
      Date modified: Prior to 1/1/20
      Assistance Received: N/A
      */
        public static DataTable FillRow(int taskNum)
        {
            string mostRecent = "SELECT TOP 1 TaskNum, FORMAT(EntryDate, 'MM/dd/yyyy') as EntryDate, New_StatusNote as LatestStatusNote, [Status] AS LatestStatus FROM History " +
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
        row count is 1, the row is added to recentHistory, else, nulls are added
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static void FillHistoryTable(DataTable recentHistory, string query)
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
                    using (SqlCommand IDCmd = new SqlCommand(query, con))
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
    }
}