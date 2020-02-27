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

        /*Name: Michael Figueroa
        Function Name: ManagerReview
        Purpose: Constructor for the ManagerReview form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: Calls FillStatusComboBox, FillSystemComboBox, and BindDataGrid
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillSystemComboBox
        Purpose: Fills SystemComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Calls SystemsInReportQuery to assign value to string query; then uses data reader to read results of query and add systems in the query to SystemComboBox
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds results from report to datagrid
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Calls FillManagerReview
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void BindDataGrid()
        {
            DataTable report = new DataTable();
            FillManagerReview(report);
        }

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBox
        Purpose: Fills StatusComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Closed");
        }

        /*Name: Michael Figueroa
        Function Name: GetCheckBoxValue
        Purpose: Retrives value of StatusComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: if StatusComboBox index is 0, then open is returned; else closed is returned.
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SystemsInReportQuery
        Purpose: returns string that has query which returns results that show what systems
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: if the checkbox value is open, then all opened items marked for managerreview are returned; else, the ones that are closed are returned
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillManagerReview
        Purpose: binds review datatable with results from GetQuery()
        Parameters: DataTable table
        Return Value: None
        Local Variables: None
        Algorithm: Uses standard SQL procedure to bind Review to GetQuery()/DataTable table
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillManagerReview
        Purpose: binds review datatable with results from GetQuery()
        Parameters: None
        Return Value: string query
        Local Variables: string query
        Algorithm: if the checkbox value is open, then all opened systems set for manager review are queried; else, all closed ones are
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SysQuery
        Purpose: for use in GetQuery()
        Parameters: None
        Return Value: string query
        Local Variables: None
        Algorithm: if SystemComboBox is set to anything but All, then A sys_impact condition is returned; else, nothing is returned
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: Status_SelectionChanged
        Purpose: Event handler Re-freshes datagrid when new status is chosen from dropdown
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: Clears all SystemComboBox items, then calls FillsSystemComboBox to fill them again based upon the new status chosen, then calls
        BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                SystemComboBox.Items.Clear();
                FillSystemComboBox();
                SystemComboBox.SelectedIndex = 0;
                BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: SystemComboBox_SelectionChanged
        Purpose: Event handler for SystemComboBox selection change
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: If the systemcombobox is not null, BindDataGrid is called
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemComboBox.SelectedItem != null)
            {
                BindDataGrid();
            }
        }

        /*Name: Michael Figueroa
       Function Name: BindHistoryGrid
       Purpose: Binds FullHistory Datagrid
       Parameters: string TaskNum
       Return Value: None
       Local Variables: string query
       Algorithm: goes through standard SQL Procedure to bind results from string query to FullHistory DataGrid
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
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
    }
}