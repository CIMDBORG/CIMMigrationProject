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
using Excel = Microsoft.Office.Interop.Excel;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class RegionReview : Page
    {
        private DataRowView regionRow;       //local variable to store the row of data in the from a specific row in the Report DataGrid
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;                       //local variable to store login-based user data
        private string reportQuery; //query used for excel export

        /*Name: Michael Figueroa
        Function Name: RegionReview
        Purpose: Constructor for RegionReview
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: Calls FillRegionComboBox, Calls ReportHelper.FillStatusComboBoxWithAll, then sets both Status and Region ComboBox indexes to 0
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public RegionReview(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillRegionComboBox();
            ReportHelper.FillStatusComboBoxWithAll(StatusComboBox);
            StatusComboBox.SelectedIndex = 0;
            RegionComboBox.SelectedIndex = 0;
        }

        /*Name: Michael Figueroa
        Function Name: FillRegionComboBox
        Purpose: Fills RegionComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillRegionComboBox()
        {
            RegionComboBox.Items.Add("Americas");
            RegionComboBox.Items.Add("Asia");
            RegionComboBox.Items.Add("Eur");
            RegionComboBox.Items.Add("Canada");
            RegionComboBox.Items.Add("US");
        }

        /*Name: Michael Figueroa
        Function Name: RegionComboBox_SelectionChanged
        Purpose: Event handler for RegionComboBox SelectionChanged event
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Sets both Datascroll, Region, and Export, and HistoryRecent
        visiblity to visible, while hiding FullHistory, then calls BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void RegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataScroll.Visibility = Visibility.Visible;
            Region.Visibility = Visibility.Visible;
            //Excel Export button
            Export.Visibility = Visibility.Visible;
            FullHistory.Visibility = Visibility.Hidden; //Hides full history by default when a system is chosem
            HistoryRecent.Visibility = Visibility.Visible; //DataGrid pops up with most recent statuses for each item by default
            BindDataGrid();//Executes custom query based on what report the user would like
        }

        /*Name: Michael Figueroa
        Function Name: StatusComboBox_SelectionChanged
        Purpose: Event handler for StatusComboBox SelectionChanged event
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: calls BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: RegionQuery
        Purpose: Returns RegionQuery based on ComboBox selections chosen
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: WHERE clause is modified based on the option chosen from the StatusComboBox
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string RegionQuery()
        {
            if (ReportHelper.StatusChosen(StatusComboBox) == "All Opened")
            {
                return "SELECT New_Issues.ID AS ID, Priority_Number, Sys_Impact AS [System], Category, TFS_BC_HDFS_Num AS BID, Assigned_To AS [Owner], " +
                       "Req_Name AS Req, New_Issues.[Status], Title, Impact, DATEDIFF(DAY, Opened_Date, Getdate()) AS [Days] " +
                       "FROM New_Issues WHERE (Req_Dept LIKE '%" + RegionChosen() + "%' OR Req_Dept LIKE '" + RegionChosen() + "%' OR Req_Dept LIKE '%" + RegionChosen() + "') AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                       "AND New_Issues.[Status] NOT LIKE '%Implemented%' " +
                       "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') ORDER BY Priority_Number ASC;";
            }
            else if(ReportHelper.StatusChosen(StatusComboBox) == "All Closed")
            {
                return "SELECT New_Issues.ID AS ID, Priority_Number, Sys_Impact AS [System], Category, TFS_BC_HDFS_Num AS BID, Assigned_To AS [Owner], " +
                       "Req_Name AS Req, New_Issues.[Status], Title, Impact, DATEDIFF(DAY, Opened_Date, Getdate()) AS [Days] " +
                       "FROM New_Issues WHERE (Req_Dept LIKE '%" + RegionChosen() + "%' OR Req_Dept LIKE '" + RegionChosen() + "%' OR Req_Dept LIKE '%" + RegionChosen() + "') AND (New_Issues.[Status] = 'closed' " +
                       "OR New_Issues.[Status] = 'Implemented' " +
                       "OR New_Issues.[Status] = 'dropped' OR New_Issues.[Status] = 'deferred') ORDER BY Priority_Number ASC;";
            }
            else
            {
                return "SELECT New_Issues.ID AS ID, Priority_Number, Sys_Impact AS [System], Category, TFS_BC_HDFS_Num AS BID, Assigned_To AS [Owner], " +
                       "Req_Name AS Req, New_Issues.[Status], Title, Impact, DATEDIFF(DAY, Opened_Date, Getdate()) AS [Days] " +
                       "FROM New_Issues WHERE (Req_Dept LIKE '%" + RegionChosen() + "%' OR Req_Dept LIKE '" + RegionChosen() + "%' OR Req_Dept LIKE '%" + RegionChosen() + "') " +
                       "AND New_Issues.[Status] = '" + ReportHelper.StatusChosen(StatusComboBox) + "' ORDER BY Priority_Number ASC;";
            }
        }

        /*Name: Michael Figueroa
        Function Name: FillRegionTable
        Purpose: Fills DataTable that will be used to bind records to the DataGrid in RegionReview form
        Parameters: None
        Return Value: None
        Local Variables: string query
        Algorithm: string query is given value from RegionQuery call, then DataTable dt is filled using SqlCommand cmd.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillRegionTable()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query = RegionQuery();
                    reportQuery = query;

                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);

                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                    Region.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }

                finally
                {
                    con.Close();
                }
        }

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Fills DataTables that will be used to bind records to the Region datagrid and History datagrid
        Parameters: None
        Return Value: None
        Local Variables: DataTable history
        Algorithm: Calls FillHistoryTable and FillRegionTable
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid()
        {
            DataTable history = new DataTable();
            FillHistoryTable(history);
            FillRegionTable();
        }

        /*Name: Michael Figueroa
        Function Name: FillRow
        Purpose: Returns a DataTable containing 1 row with most recent status of issue with TaskNum tasknum
        Parameters: int taskNum
        Return Value: DataTable historyRow
        Local Variables: string mostRecent, DataTable historyRow
        Algorithm: Fills historyRow DataTable with data from SqlCommand recentCmd; mostRecent is a query, query returns
        most recent
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillHistoryTable
        Purpose: Fills DataTable recentHistory with the most recent status for each item in the RegionReview report
        Parameters: DataTable recentHistory
        Return Value: None
        Local Variables: int taskNum, DataTable tabRecent
        Algorithm: DataColumns are added to recentHistory table, then reader2 retrieves ID of each issue, uses that value
        to call FillRow method, and the if the row retrieved using FillRow is not null, the row is added to recentHistory;
        else, nulls are added. Then, the recentHistory table is binded to the HistoryRecent datagrid
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
                        DataColumn dc2 = new DataColumn("EntryDate");
                        DataColumn dc3 = new DataColumn("LatestStatusNote");
                        DataColumn dc4 = new DataColumn("LatestStatus");

                        recentHistory.Columns.Add(dc2);
                        recentHistory.Columns.Add(dc3);
                        recentHistory.Columns.Add(dc4);

                        int taskNum;
                        using (SqlCommand IDCmd = new SqlCommand(RegionQuery(), con))
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
        Function Name: RegionChosen
        Purpose: returns string value of currently chosen value in the RegionComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: if the RegionComboBox isn't null, ToString value is returned; else, Americas is returned - Michael does 
        not remember why this is the case
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string RegionChosen()
        {
            if (RegionComboBox.SelectedItem != null)
            {
                return RegionComboBox.SelectedItem.ToString();
            }
            else
            {
                return "Americas";
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
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                regionRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(RegionQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //regionRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, regionRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
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
            //Generates an empty excel document 
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook wb;

            Microsoft.Office.Interop.Excel.Worksheet ws;

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(reportQuery, con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable Reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(Reports);
                    }


                    DataTable history = new DataTable();
                    FillHistoryTable(history);


                    Helper.ToExcelClosedXML(history, Reports);
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
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(RegionComboBox, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: Region_ScrollChanged
        Purpose: Event handler for Region scrollChanged that keeps DataGrids in sync when scrolling 
        Parameters: Auto-generated
        Return Value: None
        Local Variables: _listboxScrollViewer1 and _listboxScrollViewer2
        Algorithm: Region and HistoryRecent scrollviewers retrieved using GetDescendantByType; then vertical offset of _listboxScrollViewer2 is set to offset of _listboxScrollViewer1
        in order to keep DataGrids in sync when scrolling
        NOTE: This is also used in other windows such as ReportsWindow, so this may be better off in the helper.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Region_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
             ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Region, typeof(ScrollViewer)) as ScrollViewer;
             ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
             _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }

        /*Name: Michael Figueroa
        Function Name: HistoryRecent_ScrollChanged
        Purpose: Event handler for HistoryRecent scrollchanged that keeps DataGrids in sync when scrolling 
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
        private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(Region, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
        }
      }
    }

