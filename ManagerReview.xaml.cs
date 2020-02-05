using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ManagerReview.xaml
    /// </summary>
    public partial class ManagerReview : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView reportRow;       //local variable to store the row of data in the from a specific row in the Report DataGrid
        private bool fullHistoryChosen = false; //hides the FullHistory DataGrid by default 

        public ManagerReview(string[] user_data)
        {
            InitializeComponent();
            FillStatusComboBox();
            FillSystemComboBox();
            StatusComboBox.SelectedIndex = 0;
            SystemComboBox.SelectedIndex = 0;
            BindDataGrid();
            arr = user_data;
        }

        private void FillSystemComboBox()
        {
            SystemComboBox.Items.Add("All");
            string query = SystemsInReportQuery();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand IDCmd = new SqlCommand(query, con))
                using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        SystemComboBox.Items.Add(reader2.GetString(0));
                    }
                    reader2.Close();
                }
            }
        }

        private void BindDataGrid()
        {
            DataTable report = new DataTable();
            FillManagerReview(report);
        }

        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Closed");
        }

        private string GetCheckBoxValue()
        {
            if(StatusComboBox.SelectedIndex == 0)
            {
                return "Open";
            }
            else
            {
                return "Closed";
            }
        }

        private string SystemsInReportQuery()
        {
            string query;

            if (GetCheckBoxValue() == "Open")
            {
                 query = "SELECT DISTINCT Sys_Impact " +
                            "FROM New_Issues WHERE ManagerReview = 0 AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                            "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                            "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') ORDER BY Sys_Impact;";
            }
            else
            {
                 query = "SELECT DISTINCT Sys_Impact " +
                            "FROM New_Issues WHERE ManagerReview = 0 AND (New_Issues.[Status] LIKE '%closed%' " +
                            "OR New_Issues.[Status] LIKE '%implemented%' " +
                            "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%') ORDER BY Sys_Impact;";
            }

            return query;
        }

        private void FillManagerReview(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(GetQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(table);
                            Review.ItemsSource = table.DefaultView;                    
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("No Issues Marked For Manager Review");
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

        private string GetQuery()
        {
            string query;

            if (GetCheckBoxValue() == "Open")
            {
                 query = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS[Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                           "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS Req_Name, Opened_Date, Due_Date, " +
                            "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                            "FROM New_Issues WHERE ManagerReview = 0 AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                            "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                            "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') " + SysQuery() + " ORDER BY Sys_Impact ASC;";
            }

            else
            {
                query = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS[Owner], [Status], Category, Title, Supporting_Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                           "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS Req_Name, Opened_Date, Due_Date, " +
                            "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                            "FROM New_Issues WHERE ManagerReview = 0 AND (New_Issues.[Status] LIKE '%closed%' " +
                            "OR New_Issues.[Status] LIKE '%implemented%' " +
                            "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%') " + SysQuery() + " ORDER BY Sys_Impact ASC;";
            }

            return query;
        }

        private string SysQuery()
        {
            if (ReportHelper.SystemChosen(SystemComboBox) != "All")
            {
                return "AND (Sys_Impact = '" + ReportHelper.SystemChosen(SystemComboBox) + "')";
            }
            else
            {
                return "";
            }
        }

        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                reportRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(GetQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, reportRow, IDList);
                editRecord.FormLabel.Text = "Manager Review";
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                SystemComboBox.Items.Clear();
                FillSystemComboBox();
                SystemComboBox.SelectedIndex = 0;
                BindDataGrid();
        }

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemComboBox.SelectedItem != null)
            {
                BindDataGrid();
            }
        }

        private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void BindHistoryGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    //Query that generates individual full status history, to be displayed if the user wishes
                    string query = "SELECT format(EntryDate, 'MM/dd/yyyy') AS EntryDateHistory, New_StatusNote AS NewStatus, [Status] AS History_Status, History.TaskNum AS TaskNum " +
                                   "FROM History WHERE (TaskNum = " + TaskNum + ") AND (New_StatusNote != 'Aging') ORDER BY History.EntryDate DESC;";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
                    FullHistory.ItemsSource = dt.DefaultView;
                }


                catch (IndexOutOfRangeException)
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

        private void History_Click(object sender, RoutedEventArgs e)
        {
                reportRow = (DataRowView)((Button)e.Source).DataContext; //Retrieves the information for the row that the user clicks
                BindHistoryGrid(reportRow["ID"].ToString()); //binds full status history for row clicked
                if (fullHistoryChosen)
                {
                    FullHistory.Visibility = Visibility.Collapsed;
                    fullHistoryChosen = false;
                    Review.Width = 1900;
                }

                else
                {
                    FullHistory.Visibility = Visibility.Visible; //displays full status history DataGrid
                    fullHistoryChosen = true;
                    Review.Width = 1400;
                }
        }

        private void Review_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }   
    }
}
