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
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;
using DataTable = System.Data.DataTable;
using WpfApp2;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

//AUTHOR: Michael Figueroa
//This class is a helper class that deals with multiple forms in the application

namespace WpfApp1
{
    public static class Helper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config

        /*Author : Michael Figueroa
        Function Name: GetLatestUpdateDate
        Purpose: This is intended to be used to retrieve the latest Manager and User Updates...the dates on which updates are made
        are stored in Manager_Update and User_Update columns. The dates are broken up by semi-colons (;). This method
        allows you to retrieve the last date in those columns.
        Parameters: string updateHistory
        Return Value: updates[updates.Length - 1]
        Local Variables: char delimiter, string[] updates
        Algorithm: uses split method to split updateHistory into array
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        public static string GetLatestUpdateDate(string updateHistory)
        {
            char delimiter = ';';
            string[] updates = updateHistory.Split(delimiter);
            return updates[updates.Length - 1];
        }

        /*Author : Michael Figueroa
       Function Name: ParseUpdates
       Purpose: This splits updateHistory into array values.
       Parameters: string updateHistory
       Return Value: updates[updates.Length - 1]
       Local Variables: char delimiter, string[] updates
       Algorithm: uses split method to split updateHistory into array - pls evaluate this as this method may not be neccessary
       Version: 2.0.0.4
       Date modified: Prior to 1/1/2020
       Assistance Received: N/A
       */
        public static void ParseUpdates(string updateHistory)
        {
            char delimiter = ';';
            string[] updates = updateHistory.Split(delimiter);
        }

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds results from SQL query string query to DataTable table
        Return Value: DataTable table
        Local Variables: None 
        Parameters: DataTable table, string query
        Algorithm: Fills DataTable table with information from string query
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static DataTable BindDataGrid(DataTable table, string query)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(table);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }
            return table;
        }

        /*Name: Michael Figueroa
       Function Name: GetWeekNoCount
       Purpose: Returns the number of times a particular issue has been on aging report - this needs to be overhauled, not clear on what the true purpose of this is
       Return Value: int weekCount
       Local Variables: int weekCount, string query
       Parameters: string ID, string connectionString
       Algorithm: query is read by SqlDataReader, and COUNT(Timestamp) is returned for issue with ID string ID
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static int GetWeekNoCount(string ID, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                int weekCount;
                string query = "SELECT COUNT([Timestamp]) FROM History INNER JOIN New_Issues ON New_Issues.ID = History.TaskNum WHERE New_Issues.ID = " + ID;
                SqlCommand cmd = new SqlCommand(query, con);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        weekCount = reader.GetInt32(0);
                        return weekCount;
                    }
                }
                return weekCount = 0;
            }
        }

        /*Name: Michael Figueroa
       Function Name: UsersSystems
       Purpose: returns string[] with each index containing the user's systems - this comes from the New_Contacts systems column in SQL
       Return Value: string[] sys
       Local Variables: char delimiter, string[] sys
       Parameters: string systemString
       Algorithm: systemString is split using delimeter, then the array is returned
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static string[] UsersSystems(string systemString)
        {
            char[] delimiter = new char[] { '/', ';', ','};
            string[] sys = systemString.Split(delimiter);
            return sys;
        }

        /*Name: Michael Figueroa
       Function Name: CountOfOwnerTable
       Purpose: returns datatable showing how many times a user ended up on the aging report (I think - please evaluate)
       Return Value: DataTable countOwner
       Local Variables: DataTable countOwner, string query
       Parameters: None
       Algorithm: execute string query using basic SQL procedure to fill countOwner DataTable
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static DataTable CountOfOwnerTable()
        {
            DataTable countOwner = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Timestamp, Assigned_To, ROW_NUMBER() OVER(partition by Assigned_To ORDER BY Assigned_To DESC) AS CountOwner FROM History " +
                                "INNER JOIN New_Issues ON History.TaskNum = New_Issues.ID WHERE Timestamp IS NOT NULL GROUP BY Timestamp, Assigned_To;";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                //fill report DataGrid with the query generated
                using (sda)
                {
                    sda.Fill(countOwner);
                }
            }
            return countOwner;
        }

               /*Name: Michael Figueroa
       Function Name: FillIDList
       Purpose: this returns a list of the current ID numbers for the current report - used in EditRecord and WeeklyReviewWithApps in order to use arrows to scroll through each item
       Return Value: List<int> IDList
       Local Variables: List<int> IDList
       Parameters: string query
       Algorithm: using string query, IDList is filled using SqlDataReader, filling values from the ID column. If the query has no ID column, an empty list is returned.
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static List<int> FillIDList(string query)
        {
            List<int> IDList = new List<int>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.GetName(i) == "ID")
                            {
                                IDList.Add(reader.GetInt32(i));
                            }
                        }
                    }
                }
                return IDList;
            }
        }

        /*Name: Michael Figueroa
     Function Name: GetAgingHistory
     Purpose: returns query that shows each individual time an issue has shown up on the aging report - this query as constructed currently does not work 
     Return Value: string
     Local Variables: None
     Parameters: None
     Algorithm: None
     Version: 2.0.0.4
     Date modified: Prior to 1/1/20
     Assistance Received: N/A
     */
        public static string GetAgingHistory()
        {
            return "SELECT DISTINCT TaskNum, Assigned_To, TimeStamp FROM History INNER JOIN New_Issues ON " +
                    "New_Issues.ID = History.TaskNum " +
                    "WHERE New_StatusNote = 'Aging' " +
                    "ORDER BY TaskNum ";
        }


        /*Name: Michael Figueroa
    Function Name: GetLatestManagerUpdate
    Purpose: returns latest date that manager has requested an update on a specific item
    Return Value: string latestUpdateString
    Local Variables: string query, string latestUpdateString
    Parameters: string ID, string connectionString
    Algorithm: string query is read using SqlDataReader; then, in while statement, latestUpdateString value is obtained by calling GetLatestUpdateDate
    Version: 2.0.0.4
    Date modified: Prior to 1/1/20
    Assistance Received: N/A
    */
        public static string GetLatestManagerUpdate(string ID, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Manager_Update FROM New_Issues WHERE ID = " + ID;
                string latestUpdateString;
                using (SqlCommand IDCmd = new SqlCommand(query, con))
                using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        latestUpdateString = GetLatestUpdateDate(reader2.GetString(0));
                        return latestUpdateString;
                    }
                }
            }
            return null;
        }


        /*Name: Michael Figueroa
    Function Name: FlaggedQuery
    Purpose: Displays all issues where a manager request a user update
    Return Value: string
    Local Variables: None
    Parameters: None
    Algorithm: None
    Version: 2.0.0.4
    Date modified: Prior to 1/1/20
    Assistance Received: N/A
    */
        public static string FlaggedQuery()
        {
            return "SELECT New_Issues.ID AS IssuesID, New_Issues.TFS_BC_HDFS_Num AS [BID], New_Issues.Assigned_To AS [User], " +
                    "New_Issues.Original_Title AS [Original Title], New_Issues.Original_Bus_Imp AS [Original Bus Impact], " +
                    "New_Issues.Original_Sup_Dtls AS [Original Sup Details], New_Issues.New_Title AS [New Title], " +
                    "New_Issues.New_Sup_Dtls AS [New Sup Details], " +
                    "New_Issues.New_Bus_Imp AS [New Bus Impact], " +
                    "New_Issues.Mgr_Notes AS [Manager Notes], New_Issues.Manager_Update AS [All Manager Update Dates], " +
                    "User_Update AS [All User Update Dates] " +
                    "FROM New_Issues WHERE New_Issues.Opened_Date > '01/01/2019' AND New_Issues.Manager_Update_Bit = 1 AND User_Update != '0' " +
                    "ORDER BY New_Issues.ID, New_Issues.Manager_Update;";
        }

        /*Name: Michael Figueroa
   Function Name: GetLatestUserUpdate 
   Purpose: Retrieves latest user update made for issue with ID string ID
   Return Value: string
   Local Variables: string query
   Parameters: string ID, string connectionString
   Algorithm: uses string query in order to execute IDCmd, then reades IDCmd, and calls GetLatestUpdateDate in order to get last date where an update was made
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static string GetLatestUserUpdate(string ID, string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT User_Update FROM New_Issues WHERE ID = " + ID;
                string latestUpdateString;
                using (SqlCommand IDCmd = new SqlCommand(query, con))
                using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        latestUpdateString = GetLatestUpdateDate(reader2.GetString(0));
                        return latestUpdateString;
                    }
                }
            }
            return null;
        }

        /*Name: Michael Figueroa
   Function Name: ReturnPercentage 
   Purpose: used to determine percentage of weeks a user has been on the aging report
   Return Value: double
   Local Variables: double recent
   Parameters: int weeks
   Algorithm: divides int weeks by 52 (using casting, assigns it to a double), then returns the result of percent rounded to 2 decimal places.
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static double ReturnPercentage(int weeks)
        {
            double percent = ((double)weeks / 52) * 100;
            return Math.Round(percent, 2);
        }

        /*Name: Michael Figueroa
   Function Name: FlaggedReportTable 
   Purpose: returns a DataTable with results from FlaggedQuery()
   Return Value: DataTable flaggedReport
   Local Variables: DataTable flaggedReport, string query
   Parameters: string connectionString
   Algorithm: fills flaggedReport using string query, 
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static DataTable FlaggedReportTable(string connectionString)
        {
            DataTable flaggedReport = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = FlaggedQuery();

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                //fill report DataGrid with the query generated
                using (sda)
                {
                    sda.Fill(flaggedReport);
                }

                string firstManagerUpdate;
                string latestUserUpdate;
                int rowCounter = 0;

                DataColumn managerUpdate = new DataColumn("First Manager Update");
                DataColumn userUpdate = new DataColumn("Last User Update");
                DataColumn numDays = new DataColumn("# Days To Update");
                flaggedReport.Columns.Add(managerUpdate);
                flaggedReport.Columns.Add(userUpdate);
                flaggedReport.Columns.Add(numDays);
                try
                {
                    using (SqlDataReader reader2 = cmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            firstManagerUpdate = MyTalentHelper.GetFirstManagerUpdate(reader2.GetString(10));
                            flaggedReport.Rows[rowCounter][managerUpdate] = firstManagerUpdate;                           
                            latestUserUpdate = GetLatestUserUpdate(reader2.GetInt32(0).ToString(), connectionString);
                            flaggedReport.Rows[rowCounter][userUpdate] = latestUserUpdate;
                            DateTime firstManDate;
                            DateTime latestUserUpd;
                            if (DateTime.TryParse(firstManagerUpdate, out firstManDate) && (DateTime.TryParse(latestUserUpdate, out latestUserUpd)))
                            {
                                TimeSpan diff = latestUserUpd - firstManDate;
                                flaggedReport.Rows[rowCounter][numDays] = diff.Days;
                            }
                            rowCounter++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return flaggedReport;
        }

        /*Name: Michael Figueroa
   Function Name: GetUpdatedDateString 
   Purpose: this takes today's date and appends it to the date in sqlserver userUpdate or managerUpdate columns if those columns were not null before; if they were null before,
   then the column is set to today's date.
   Return Value: string
   Local Variables: double recent
   Parameters: int weeks
   Algorithm: 
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static string GetUpdatedDateString(string ID, bool manager)
        {
            string todaysDate = DateTime.Now.ToString("M/d/yyyy");
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    StringBuilder updateDateBuilder;
                    string query = "SELECT Manager_Update, User_Update FROM New_Issues WHERE ID = " + ID;
                    using (SqlCommand IDCmd = new SqlCommand(query, con))
                    using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            if (manager)
                            {
                                if (!reader2.IsDBNull(0))
                                {
                                    updateDateBuilder = new StringBuilder(reader2.GetString(0).ToString());
                                    updateDateBuilder.Append(";" + todaysDate);
                                    return updateDateBuilder.ToString();
                                }

                                else
                                {
                                    return todaysDate;
                                }
                            }

                            else
                            {
                                if (!reader2.IsDBNull(1))
                                {
                                    updateDateBuilder = new StringBuilder(reader2.GetString(1).ToString());
                                    updateDateBuilder.Append(";" + todaysDate);
                                    return updateDateBuilder.ToString();
                                }

                                else
                                {
                                    return todaysDate;
                                }
                            }
                        }
                        reader2.Close();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("No date retrieved");
                    return null;
                }

            return null;
        }

        /*Name: Michael Figueroa
   Function Name: FillSystemComboBox 
   Purpose: Fills ComboBox systemComboBox
   Return Value: None
   Local Variables: None
   Parameters: ComboBox systemComboBox
   Algorithm: None
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static void FillSystemComboBox(ComboBox systemComboBox)
        {
            systemComboBox.Items.Add("All");
            systemComboBox.Items.Add("ABR");
            systemComboBox.Items.Add("BAT");
            systemComboBox.Items.Add("BFR");
            systemComboBox.Items.Add("BIS");
            systemComboBox.Items.Add("BRRS");
            systemComboBox.Items.Add("BWS");
            systemComboBox.Items.Add("CDC");
            systemComboBox.Items.Add("CIM");
            systemComboBox.Items.Add("CRIS");
            systemComboBox.Items.Add("DOC");
            systemComboBox.Items.Add("EBA");
            systemComboBox.Items.Add("EBCM");
            systemComboBox.Items.Add("eBilling");
            systemComboBox.Items.Add("EDI");
            systemComboBox.Items.Add("FBR");
            systemComboBox.Items.Add("FCB");
            systemComboBox.Items.Add("IB");
            systemComboBox.Items.Add("IFA");
            systemComboBox.Items.Add("MDC");
            systemComboBox.Items.Add("ODBI");
            systemComboBox.Items.Add("PMC");
            systemComboBox.Items.Add("PS");
            systemComboBox.Items.Add("SOX");
            systemComboBox.Items.Add("Vendor");
        }

        /*Name: Michael Figueroa
   Function Name: FillSystemComboBoxNoAll
   Purpose: Fills ComboBox systemComboBox without All as an option
   Return Value: None
   Local Variables: None
   Parameters: ComboBox systemComboBox
   Algorithm: None
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        public static void FillSystemComboBoxNoAll(ComboBox systemComboBox)
        {
            systemComboBox.Items.Add("ABR");
            systemComboBox.Items.Add("BAT");
            systemComboBox.Items.Add("BFR");
            systemComboBox.Items.Add("BIS");
            systemComboBox.Items.Add("BRRS");
            systemComboBox.Items.Add("BWS");
            systemComboBox.Items.Add("CDC");
            systemComboBox.Items.Add("CIM");
            systemComboBox.Items.Add("CRIS");
            systemComboBox.Items.Add("DOC");
            systemComboBox.Items.Add("EBA");
            systemComboBox.Items.Add("EBCM");
            systemComboBox.Items.Add("eBilling");
            systemComboBox.Items.Add("EDI");
            systemComboBox.Items.Add("FBR");
            systemComboBox.Items.Add("FCB");
            systemComboBox.Items.Add("IB");
            systemComboBox.Items.Add("IFA");
            systemComboBox.Items.Add("MDC");
            systemComboBox.Items.Add("ODBI");
            systemComboBox.Items.Add("PMC");
            systemComboBox.Items.Add("PS");
            systemComboBox.Items.Add("SOX");
            systemComboBox.Items.Add("Vendor");
        }

        /*Name: Brandon Cox
   Function Name: CurrentStatus
   Purpose: Determines what the current status is
   Return Value: None
   Local Variables: string query, string lastStatus
   Parameters: string ID, ComboBox statusComboBox
   Algorithm: using string query and SqlDataReader, lastStatus is assigned the status obtained by the result of the query; for loop goes through statusComboBox.items.count iterations,
   and if lastStatus matches statusComboBox.selectedIndex i.ToString, the for loop is broken
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: Michael Figueroa (comments)
   */
        public static void CurrentStatus(string ID, ComboBox statusComboBox)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT [Status] FROM New_Issues WHERE ID = " + ID;
                string lastStatus;
                using (SqlCommand IDCmd = new SqlCommand(query, con))
                using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        lastStatus = reader2.GetString(0);
                        for (int i = 0; i < statusComboBox.Items.Count; i++)
                        {
                            statusComboBox.SelectedIndex = i;
                            if (lastStatus == statusComboBox.SelectedItem.ToString())
                                break;
                        }
                    }
                }
            }
        }

        /*Name: Michael Figueroa
   Function Name: ToExcelClosedXML
   Purpose: For the Excel export with both recent status for each issue and the information from New_Issues table - does not use ChopTable method
   Return Value: None
   Local Variables: int numCol, XLWorkbook wb, var ws
   Parameters: DataTable historyRecent, DataTable report
   Algorithm: numCol is set to the number of columns in DataTable report; InsertTable is called on wb; then save dialog is shown for user to save spreadsheet
   Version: 2.0.0.4
   Date modified: Prior to 1/1/20
   Assistance Received: None
   */
        public static void ToExcelClosedXML(DataTable historyRecent, DataTable report)
        {
            XLWorkbook wb = new XLWorkbook();
            int numCol = report.Columns.Count;
            var ws = wb.Worksheets.Add(report, "Report");
            wb.Worksheet(1).Cell(1,numCol + 1).InsertTable(historyRecent);
                                                
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx",
                Title = "Save an Excel File"
            };

            saveFileDialog.ShowDialog();

            if (!String.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                wb.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("File Saved As " + saveFileDialog.FileName.ToString());
            }

            wb.Dispose();
        }

        /*Name: Michael Figueroa
        Function Name: ChopTable
        Purpose: For the Excel export 
        Return Value: None
        Local Variables: DataColumn system, DataColumn priority_number, DataColumn category, DataColumn bid, DataColumn opened_date, DataColumn title
        Parameters: DataTable tableOne, DataTable tableTwo
        Algorithm: DataColumns are added to tableTwo, which is empty. The foreach loop imports the DataRows, only importing the columns that are included in tableTwo. (system,
        priority_number, etc.)
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: None
        */
        public static void ChopTable(DataTable tableOne, DataTable tableTwo)
        {
            DataColumn system = new DataColumn("System");
            DataColumn priority_Number = new DataColumn("Priority_Number");
            DataColumn category = new DataColumn("Category");
            DataColumn bid = new DataColumn("BID");
            DataColumn opened_Date = new DataColumn("Opened_Date");
            DataColumn title = new DataColumn("Title");

            tableTwo.Columns.Add(system);
            tableTwo.Columns.Add(priority_Number);
            tableTwo.Columns.Add(category);
            tableTwo.Columns.Add(bid);
            tableTwo.Columns.Add(opened_Date);
            tableTwo.Columns.Add(title);

            foreach (DataRow row in tableOne.Rows)
            {
                tableTwo.ImportRow(row);
            }
        }

        /*Name: Michael Figueroa
       Function Name: ToExcelClosedXML
       Purpose: Excel export - this uses ChopTable
       Return Value: None
       Local Variables: DataTable shortenedReport, XLWorkbook wb, int numCol, var ws
       Algorithm: Calls ChopTable with parameters report and shortenedReport, then adjusts the column width of the spreadsheet,then prompts user to save worksheet for viewing.
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: None
       */
        public static void ToExcelClosedXML(DataTable report)
        {
            DataTable shortenedReport = new DataTable();
            ChopTable(report, shortenedReport);
            XLWorkbook wb = new XLWorkbook();
            int numCol = report.Columns.Count;
            var ws = wb.Worksheets.Add(shortenedReport, "Report");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx",
                Title = "Save an Excel File"
            };

            ws.Columns().AdjustToContents();
            ws.Rows().Height = 45;
            //columns a and b, widths 7 and 10
            ws.Columns("A").Width = 7;
            ws.Columns("B").Width = 13.27;
            ws.Columns("F").Width = 72;
            ws.Columns("F").Style.Alignment.WrapText = true;

            saveFileDialog.ShowDialog();

            if (!String.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                wb.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("File Saved As " + saveFileDialog.FileName.ToString());
            }
            wb.Dispose();
        }
    }
}