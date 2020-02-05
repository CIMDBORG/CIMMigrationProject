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
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ManagerTasks.xaml
    /// </summary>
    public partial class ManagerTasks : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'ManagerTasks' DataGrid
        private string reportQuery;
        private static DataTable Reports = new DataTable();

        public ManagerTasks(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            BindDataGrid();
            UpdatedToday();
        }

        public void BindDataGrid()
        {
            string query = ManagerTasksQuery();
            DataTable managerTasks = new DataTable();
            
            reportQuery = query;
     
            //History query from which rows will be extracted to display most recent status updates for each item in the report 
            DataTable historyTable = new DataTable();

            FillManTasksTable(managerTasks);
            FillHistoryTable(historyTable);
        }
        

        private string ManagerTasksQuery()
        {
            string query = "SELECT New_Issues.ID as ID, TFS_BC_HDFS_Num AS BID#, [Status] AS Status, FORMAT(Opened_Date, 'MM/dd/yyyy') AS Opened_Date, FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, Assigned_To, Supporting_Details, Internal_Notes, " +
                            "Priority_Number FROM New_Issues " +
                            "WHERE [Status] NOT LIKE '%closed%' AND [Status] NOT LIKE '%implemented%' AND [Status] NOT LIKE '%dropped%' AND[Status] NOT LIKE '%deferred%' " +
                            "AND [Status] NOT LIKE '%BC Approved%' AND Category = 'BC/TI' " +
                            "ORDER BY Priority_Number ASC;";
            return query;
        }

        private DataTable FillRow(int taskNum)
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

        private void FillHistoryTable(DataTable recentHistory)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    DataColumn dc1 = new DataColumn("TaskNum");
                    DataColumn dc2 = new DataColumn("EntryDate");
                    DataColumn dc3 = new DataColumn("LatestStatusNote");
                    DataColumn dc4 = new DataColumn("LatestStatus");

                    recentHistory.Columns.Add(dc1);
                    recentHistory.Columns.Add(dc2);
                    recentHistory.Columns.Add(dc3);
                    recentHistory.Columns.Add(dc4);

                    int taskNum;
                    using (SqlCommand IDCmd = new SqlCommand(ManagerTasksQuery(), con))
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
                                    recentHistory.Rows.Add(null, null, null, null);
                                }
                            }
                            reader2.Close();
                        }
                        HistoryRecent.ItemsSource = recentHistory.DefaultView;
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

            private void FillManTasksTable(DataTable managerTasks)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(ManagerTasksQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(Reports);
                        ManTasks.Visibility = Visibility.Visible;
                        ManTasks.ItemsSource = Reports.DefaultView;
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

        private void UpdatedToday()
        {
            DataTable updated = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string todaysDate = DateTime.Now.ToString("M/d/yyyy");
                    string updatedQuery = "SELECT TFS_BC_HDFS_Num AS BID#, [Status] AS Status FROM New_Issues WHERE Manager_Update LIKE '" + todaysDate + "';";
                    SqlCommand cmd = new SqlCommand(updatedQuery, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);

                    using (sda)
                    {
                        sda.Fill(updated);
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
            Updated.ItemsSource = updated.DefaultView;
        }

      
        //allows manager to change the status of an issue
        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    //form that will act as a second button, which will ask the manager if they want the issue updated to BC Submitted or BC Approved
                    StatusChangeButton statusChange = new StatusChangeButton();
                    DialogResult changeStatusResult;
                    connection.Open();
                    priorBySystemRow = (DataRowView)((System.Windows.Controls.Button)e.Source).DataContext;

                    //On ChangeStatus button click, pulls the data from that row of the datagrid, and stores it as a DataRow object

                    changeStatusResult = MessageBox.Show("Would You Like to Change Status?", "Change Status", MessageBoxButtons.YesNo);
                    //If yes, ID of issue is extracted in order to update the issue's status to active in the database ONLY if the update is successful

                    if (changeStatusResult == DialogResult.Yes)
                    {
                        string updatedDateString = Helper.GetUpdatedDateString(priorBySystemRow[0].ToString(), true);
                        string checkQuery = "SELECT [Status] FROM New_Issues WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";
                        SqlCommand cd = new SqlCommand(checkQuery, connection);
                        SqlDataAdapter stat = new SqlDataAdapter(cd);
                        DataTable status = new DataTable();

                        using (stat)
                        {
                            stat.Fill(status);
                        }
                         
                        //checks if the status has already been updated during this session; if so , messagebox will notify user

                        if (status.Rows[0].Field<String>("Status") != "BC Approved" && status.Rows[0].Field<String>("Status") != "BC Submitted")
                        {
                            statusChange.ShowDialog();
                            //this adds ID to the table that shows which statuses have been updated during that session
                            //the buttons come from another form called StatusChangeButton.xaml, which then closes upon click
                            if ((statusChange.approvedClicked))
                            {
                                //Updates both status and the entrydate simultaneously
                                string query = "UPDATE New_Issues SET [Status] = 'BC Approved' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";
                                string queryHistory = "UPDATE New_Issues SET Manager_Update = '" + updatedDateString + "' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";

                                SqlCommand command = new SqlCommand(query, connection);
                                //Updates the updated table
                                SqlCommand upCommand = new SqlCommand(queryHistory, connection);

                                command.ExecuteNonQuery();
                                upCommand.ExecuteNonQuery();

                                MessageBox.Show("Updated to BC Approved!");
                            }

                            else
                            {
                                string query = "UPDATE New_Issues SET [Status] = 'BC Submitted' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";
                                string queryHistory = "UPDATE New_Issues SET Manager_Update = '" + updatedDateString + "' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";

                                SqlCommand command = new SqlCommand(query, connection);
                                //Updates the updated table
                                SqlCommand upCommand = new SqlCommand(queryHistory, connection);
                                command.ExecuteNonQuery();
                                upCommand.ExecuteNonQuery();

                                MessageBox.Show("Updated to BC Submitted!");
                            }
                        }

                        //if the status has already been changed during this session, the user will be informed here
                        else if (status.Rows[0].Field<String>("Status") == "BC Submitted")
                        {
                            statusChange.BCSubmitted.Visibility = Visibility.Collapsed;
                            statusChange.ShowDialog();
                            //this adds ID to the table that shows which statuses have been updated during that session
                            //the buttons come from another form called StatusChangeButton.xaml, which then closes upon click. In this case, one button, BC Approved is shown due to the status already having been BC Submitted
                            if ((statusChange.approvedClicked))
                            {
                                //Updates both status and the entrydate simultaneously
                                string query = "UPDATE New_Issues SET [Status] = 'BC Approved' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";
                                string queryHistory = "UPDATE New_Issues SET Manager_Update = '" + updatedDateString + "' WHERE TFS_BC_HDFS_Num = " + priorBySystemRow[1] + ";";

                                SqlCommand command = new SqlCommand(query, connection);
                                //Updates the updated table
                                SqlCommand upCommand = new SqlCommand(queryHistory, connection);

                                command.ExecuteNonQuery();
                                upCommand.ExecuteNonQuery();

                                MessageBox.Show("Updated to BC Approved!");
                            }
                            else
                            {
                                MessageBox.Show("Status Already Changed!");
                            }
                        }
                    }
                    //Refreshes the updated and manager tasks table so it can be up-to-the-minute
                    UpdatedToday();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }

                finally
                {
                    connection.Close();
                }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                priorBySystemRow = (DataRowView)((System.Windows.Controls.Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(ManagerTasksQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //regionRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, priorBySystemRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //The rest of the methods allow for the scrolling of both datagrids to be in sync
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

            private void lbx1_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(ManTasks, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }

            private void ManTasks_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(ManTasks, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }

            private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(ManTasks, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }

        
    }
}