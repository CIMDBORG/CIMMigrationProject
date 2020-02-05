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

        //This allows the user to query issues consisting of all their systems
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

        public static string OwnerAgingQuery(string owner)
        {
            string query = "SELECT Sys_Impact as [System], New_Issues.[Status], Assigned_To AS[Owner], Category, TFS_BC_HDFS_Num as BID#, Impact, " +
                            "Title, FORMAT(Latest_Status_Update, 'MM/dd/yyyy') as Latest_Status_Update, " +
                            "(SELECT DATEDIFF(day, Opened_Date, CONVERT(date, GETDATE())))as Open_Days,(SELECT DATEDIFF(day, Latest_Status_Update, CONVERT(date, GETDATE()))) as Status_Days, ID as ID " +
                            "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                            "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE((Category LIKE 'BC%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 178)) " +
                            "OR((Category NOT LIKE 'BC%' AND Impact NOT LIKE '%Not Billed Items%') AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 20)) OR(Impact LIKE '%Not Billed Items%'" +
                            "AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 6)) OR(Category LIKE '%Strategic Task%' AND(DATEDIFF(day, h1.Latest_Status_Update, CONVERT(date, GETDATE())) > 12))) " +
                            "AND(New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%' " +
                            "AND New_Issues.[Status] NOT LIKE '%Not Assigned%' AND New_Issues.[Status] NOT LIKE '%Completed%') AND(Assigned_To LIKE '%" + owner + "%') ORDER BY TaskNum ASC;";
            return query;
        }

        //this notifies user that they are two days away from having an aging item
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

        public static string OwnerUpdatesReq(string owner)
        {
            return "SELECT ID FROM New_Issues WHERE (Manager_Update_Bit = 1) AND (User_Update_Bit = 0) AND (Assigned_To = '" + owner + "') AND" +
                " (New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%');";
        }

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

        public static void FillStatusComboBox(ComboBox statusComboBox)
        {
            statusComboBox.Items.Add("Pending");
            statusComboBox.Items.Add("Active");
            statusComboBox.Items.Add("App Review");
            statusComboBox.Items.Add("BC Submitted");
            statusComboBox.Items.Add("BC Approved");
        }

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