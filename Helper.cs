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
        public static string connection = ConfigurationManager.ConnectionStrings["connectionS"].ConnectionString;

        public static string GetLatestUpdateDate(string updateHistory)
        {
            char delimiter = ';';
            string[] updates = updateHistory.Split(delimiter);
            return updates[updates.Length - 1];
        }

        public static void ParseUpdates(string updateHistory)
        {
            char delimiter = ';';
            string[] updates = updateHistory.Split(delimiter);
        }

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

        //returns string[] with each index containing the user's systems
        public static string[] UsersSystems(string systemString)
        {
            char[] delimiter = new char[] { '/', ';', ','};
            string[] sys = systemString.Split(delimiter);
            return sys;
        }

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

        //this returns a list of the current ID numbers for the current report; 
        //this is used in order to provide the ID Array for Edit Record, where the user is able to move through issues using the arrows

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

        public static string GetAgingHistory()
        {
            return "SELECT DISTINCT TaskNum, Assigned_To, TimeStamp FROM History INNER JOIN New_Issues ON " +
                    "New_Issues.ID = History.TaskNum " +
                    "WHERE New_StatusNote = 'Aging' " +
                    "ORDER BY TaskNum ";
        }



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

        public static double ReturnPercentage(int weeks)
        {
            double percent = ((double)weeks / 52) * 100;
            return Math.Round(percent, 2);
        }

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
                string diffDate;
                string issueID;
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

        //this returns today's date, then appends it to the date in sqlserver userUpdate or managerUpdate columns
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

        public static void FillSystemComboBox(ComboBox systemComboBox)
        {
            systemComboBox.Items.Add("All");
            systemComboBox.Items.Add("Auto");
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

        public static void FillSystemComboBoxNoAll(ComboBox systemComboBox)
        {
            systemComboBox.Items.Add("Auto");
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

        //condences datatable one in order to export it into an excel spreadsheet
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