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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for WeeklyReview.xaml
    /// </summary>
    public partial class WeeklyReview : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView reportRow;       //local variable to store the row of data in the from a specific row in the Report DataGrid
        private bool fullHistoryChosen = false;

        public WeeklyReview(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillStatusComboBox();
            FillSystemComboBox();
            StatusComboBox.SelectedIndex = 0;
            SystemComboBox.SelectedIndex = 0;
            SetStatusComboVis();
            BindDataGrid();
        }

        private void BindDataGrid()
        {
            DataTable report = new DataTable();
            FillWeeklyReview(report);
        }

        private void SetStatusComboVis()
        {
            if(SystemComboBox.SelectedItem.ToString() == "All Open")
            {
                StatusComboBox.Visibility = Visibility.Collapsed;
                StatusText.Visibility = Visibility.Collapsed;
            }
            else
            {
                StatusComboBox.Visibility = Visibility.Visible;
                StatusText.Visibility = Visibility.Collapsed;
            }
        }

        private void FillSystemComboBox()
        {
            string query = SystemsInReportQuery();
            if (StatusComboBox.SelectedIndex == 0)
            {
                SystemComboBox.Items.Add("All Open");
            }
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

        private string WeeklyQuery()
        {
            string weeklyQuery;
            if(ReportHelper.SystemChosen(SystemComboBox) == "All Open")
            {
                weeklyQuery = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS[Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                           "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name, FORMAT(Opened_Date, 'MM/dd/yyyy') as Opened_Date, FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, " +
                            "FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS Days FROM New_Issues WHERE (WeeklyReview = 1) AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                            "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                            "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'BC/TI') AND (Category != 'Strategic Task') ORDER BY Sys_Impact ASC, Assigned_To ASC;";
            }

            else if (GetComboBoxValue() == "Open")
            {
                 weeklyQuery = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS[Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                           "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name, FORMAT(Opened_Date, 'MM/dd/yyyy') as Opened_Date, FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, " +
                            "FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS Days FROM New_Issues WHERE (WeeklyReview = 1) AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                            "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                            "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'BC/TI') AND (Category != 'Strategic Task') AND (Sys_Impact = '" + ReportHelper.SystemChosen(SystemComboBox) + "') ORDER BY Sys_Impact ASC, Assigned_To ASC;";
            }

            else
            {
                 weeklyQuery = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS[Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                        "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name, FORMAT(Opened_Date, 'MM/dd/yyyy') as Opened_Date, FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, " +
                            "FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS Days FROM New_Issues WHERE (WeeklyReview = 1) AND (New_Issues.[Status] LIKE '%closed%' " +
                            "OR New_Issues.[Status] LIKE '%implemented%' " +
                            "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%') AND (Category != 'BC/TI') AND (Category != 'Strategic Task') AND (Sys_Impact = '" + ReportHelper.SystemChosen(SystemComboBox) + "') ORDER BY Sys_Impact ASC, Assigned_To ASC;";
            }
            return weeklyQuery;
        }

        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Closed");
        }

        private void FillWeeklyReview(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(WeeklyQuery(), con);
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
                    MessageBox.Show("No Issues Marked For Weekly Review");
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

        private void MarkAsReviewed()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(WeeklyQuery(), con);
                    //fill report DataGrid with the query generated
                    using (SqlDataReader reader2 = cmd.ExecuteReader())
                    {
                        using (SqlConnection con2 = new SqlConnection(connectionString))
                        {
                            con2.Open();
                            while (reader2.Read())
                            {
                                string query = "UPDATE New_Issues SET WeeklyReview = 0 WHERE ID = " + reader2.GetInt32(0) + ";";
                                SqlCommand updateCmd = new SqlCommand(query, con2);
                                updateCmd.ExecuteNonQuery();
                            }
                            con2.Close();
                        }
                        reader2.Close();
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    MessageBox.Show("No Issues Marked For Weekly Review");
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
                List<int> IDList = Helper.FillIDList(WeeklyQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                WeeklyReviewApps weeklyReviewApps = new WeeklyReviewApps(arr,reportRow,IDList);
                weeklyReviewApps.FormLabel.Text = "Weekly Review";
                weeklyReviewApps.Show();
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
            SetStatusComboVis();
            BindDataGrid();
        }

        private string GetComboBoxValue()
        {
            if (StatusComboBox.SelectedIndex == 0)
            {
                return "Open";
            }

            else
            {
                return "Closed";
            }
        }

        private void MarkReviewed_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Mark All as Reviewed?", "Mark as Reviewed", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                MarkAsReviewed();
                BindDataGrid();
            }
        }

        private string SystemsInReportQuery()
        {
            string query;

            if (GetComboBoxValue() == "All Open")
            {
                query = "SELECT DISTINCT Sys_Impact " +
                           "FROM New_Issues WHERE WeeklyReview = 1 AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                           "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                           "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'BC/TI') ORDER BY Sys_Impact;";
            }
            else if (GetComboBoxValue() == "Open")
            {
                query = "SELECT DISTINCT Sys_Impact " +
                            "FROM New_Issues WHERE WeeklyReview = 1 AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                            "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                            "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'BC/TI') ORDER BY Sys_Impact;";
            }

            else 
            {
                query = "SELECT DISTINCT Sys_Impact " +
                           "FROM New_Issues WHERE WeeklyReview = 1 AND (New_Issues.[Status] LIKE '%closed%' " +
                           "OR New_Issues.[Status] LIKE '%implemented%' " +
                           "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%') AND (Category != 'BC/TI') ORDER BY Sys_Impact;";
            }

            return query;
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

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemComboBox.SelectedItem != null)
            {
                SetStatusComboVis();
                BindDataGrid();
            }
        }
    }
}
