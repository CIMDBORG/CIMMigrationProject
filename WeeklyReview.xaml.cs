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
        private bool fullHistoryChosen = false; //bool that determines whether the user has toggled on full status history for a particular issue

        /*Name: Michael Figueroa
       Function Name: WeeklyReview
       Purpose: Constructor for the WeeklyReview form
       Parameters: string[] user_data
       Return Value: None
       Local Variables: None
       Algorithm: Calls FillStatusComboBox, then FillSystemComboBox, both indexes set to "All Open", then SetStatusComboVis and BindDataGrid are called
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
       Function Name: BindDataGrid
       Purpose: Binds data to datagrid
       Parameters: None
       Return Value: None
       Local Variables: report
       Algorithm: Calls FillWeeklyReview
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void BindDataGrid()
        {
            DataTable report = new DataTable();
            FillWeeklyReview(report);
        }

        /*Name: Michael Figueroa
        Function Name: SetStatusComboVis
        Purpose: Sets visibility of StatusComboBox and StatusText (StatusComboBox label)
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: If All Open from SystemComboBox is chosen, then statuscombobox not visible; else, it is.
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillSystemComboBox
        Purpose: Sets visibility of StatusComboBox and StatusText (StatusComboBox label)
        Parameters: None
        Return Value: None
        Local Variables: query
        Algorithm: Calls SystemsInReportQuery to assign value to query. If status chosen from ComboBox is All Open, then All Open is added to SystemComboBox. Then, all systems that have an open issue for the week are
        added to the SystemComboBox using query. 
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
       Function Name: WeeklyQuery
       Purpose: Sets query used for WeeklyReview
       Parameters: None
       Return Value: string
       Local Variables: weeklyQuery
       Algorithm: query is set based on what options are chosen in System and Status comboboxes
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBox
        Purpose: Fills Status combobox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 - May no longer be needed, Mike may want to get rid of it idk 
        Assistance Received: N/A
        */
        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Closed");
        }

        /*Name: Michael Figueroa
        Function Name: FillWeeklyReview
        Purpose: Fills Weekly Review table
        Parameters: DataTable table
        Return Value: None
        Local Variables: None
        Algorithm: WeeklyQuery() is executed, then table is filled, binded to DataGrid
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: MarkAsReviewed
        Purpose: Allows manager/supervisor to clear items off of weekly review form after meeting is held
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Calls WeeklyQuery() to execute command and then uses reader to go through each record and update WeeklyReview column to 0 in order to clear it off the form
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
         Function Name: EditButton_Click
         Purpose: Event handler for edit button click
         Parameters: Auto-generated
         Return Value: None
         Local Variables: DataRowView priorBySystemRow
         Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - This method will be simplified by Mike at a later date
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
         Function Name: Status_SelectionChanged
         Purpose: Event handler for StatusComboBox
         Parameters: Auto-generated
         Return Value: None
         Local Variables: None
         Algorithm: SystemComboBox is cleared, then re-filled using FillSystemComboBox(), SelectedIndex is set to 0, then SetStatusComboVis and BindDataGrid are called
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20  
         Assistance Received: N/A
         */
        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SystemComboBox.Items.Clear();
            FillSystemComboBox();
            SystemComboBox.SelectedIndex = 0;
            SetStatusComboVis();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
         Function Name: GetComboBoxValue
         Purpose: retrieves string value of current status chosen from comboBox
         Parameters: Auto-generated
         Return Value: string
         Local Variables: None
         Algorithm: is selectedIndex is all open, then "Open" is return value; else, Closed is returned
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20  
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
         Function Name: MarkReviewed_Click
         Purpose: Event handler for Mark Reviewed click
         Parameters: Auto-generated
         Return Value: None
         Local Variables: None
         Algorithm: if MessageBox option chosen is yes, then MarkAsReviews is called along with BindDataGrid
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 
         Assistance Received: N/A
         */
        private void MarkReviewed_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Mark All as Reviewed?", "Mark as Reviewed", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                MarkAsReviewed();
                BindDataGrid();
            }
        }

        /*Name: Michael Figueroa
         Function Name: SystemsInReportQuery
         Purpose: Query that tells us what systems are included in this week's Weekly Review
         Parameters: None
         Return Value: None
         Local Variables: None
         Algorithm: If All Open is chosen, all non-closed items are chosen; else, if open, all non-closed items are chosen; else, all closed items are queried.
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - the else if clause is not needed. 
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
         Function Name: BindHistoryGrid
         Purpose: Query that pulls all statuses for a particular issue
         Parameters: string TaskNum
         Return Value: None
         Local Variables: query
         Algorithm: query is assigned a string value; then the data from the query is binded to the FullHistory datagrid
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - the else if clause is not needed. 
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
         Function Name: History_Click
         Purpose: Event handler for history button
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: query
         Algorithm: reportRow is assigned using the row that was clicked; then BindHistoryGrid is called; if fullHistoryChosen is true, the FullHistory datagrid is visible, else, it is not
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - the else if clause is not needed. 
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
        Function Name: SystemComboBox_SelectionChanged
        Purpose: Event handler for when SystemComboBox changes
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: query
        Algorithm: if SystemComboBox is not null, SetStatusComboVis and BindDataGrid are called
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - the else if clause is not needed. 
        Assistance Received: N/A
        */
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
