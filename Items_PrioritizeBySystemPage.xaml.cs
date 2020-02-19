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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    public partial class Items_PrioritizeBySystemPage : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery;
        private bool includeStrategicTasks = false;

        /*Name: Michael Figueroa
        Function Name: Items_PrioritizeBySystemPage
        Purpose: Constructor for the Items_PrioritizeBySystemPage form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: Calls FillSystemComboBox, collapses the Report datagrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public Items_PrioritizeBySystemPage(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillSystemComboBox(arr[7]);
            Report.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: FillSystemComboBox
        Purpose: Fills System Combo Box
        Parameters: string systemString
        Return Value: None
        Local Variables: None
        Algorithm: splits systemstring using delimiter, then uses for loop to add systems to ComboBox
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillSystemComboBox(string systemString)
        {
            char delimiter = '/';
            string[] sys = systemString.Split(delimiter);

            int len = sys.Length;
            for (int x = 0; x < len; x++)
            {
                SystemComboBox.Items.Add(sys[x]);
            }
            SystemComboBox.Items.Add("CIM");
        }




        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds DataGrid
        Parameters: string sys
        Return Value: None
        Local Variables: None
        Algorithm: assigns query, then binds information to datagrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid(string sys)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query = "SELECT ID, Priority_Number, Sys_Impact as [System], Category, Req_Name AS Req, TFS_BC_HDFS_Num as BID, " +
                                    "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy') AS Opened_Date, [Status], Title, " +
                                    "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as DaysOpen, Hot_Topic, User_Update_Bit as User_Update, " +
                                    "Manager_Update_Bit AS Manager_Update, DATEDIFF(DAY, Latest_Status_Update, GETDATE()) AS DaysSinceLastUpdate " +
                                    "FROM New_Issues INNER JOIN(SELECT TaskNum, MAX(EntryDate) AS Latest_Status_Update FROM History " +
                                    "GROUP BY TaskNum) h1 ON h1.TaskNum = New_Issues.ID WHERE (Sys_Impact like '%" + sys + "%' AND [Status]!= 'Closed' AND [Status]!= 'Deferred' AND [Status]!= 'Implemented' " +
                                    "AND [Status]!= 'Dropped') " +
                                    AppendStratTaskFilter() + "ORDER BY Priority_Number ASC, TFS_BC_HDFS_Num;";
                    reportQuery = query;
                    
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    
                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                        Report.ItemsSource = dt.DefaultView;                   
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
        Function Name: AppendStratTaskFilter
        Purpose: Appends strategic task condition to where clause
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: if strategic task is chosen, then there is no condition added to the WHERE clause; else, then there is a condition added that excludes them
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public string AppendStratTaskFilter()
        {
            if (includeStrategicTasks == true)
            {
                return "";
            }
            else
            {
                return " AND (Category NOT LIKE '%Strategic Task%') ";
            }
        }

        /*Name: Michael Figueroa
        Function Name: SystemComboBox_SelectionChanged
        Purpose: refreshes datagrid when new system is selected
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Report.Visibility = Visibility.Visible;
            
            BindDataGrid(SystemComboBox.SelectedItem.ToString());
        }




        /*Name: Michael Figueroa
         Function Name: EditButton_Click
         Purpose: Event handler for edit button click
         Parameters: Auto-generated
         Return Value: None
         Local Variables: DataRowView agingItemsRow
         Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - This method will be simplified by Mike at a later date
         Assistance Received: N/A
         */
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                priorBySystemRow = (DataRowView)((System.Windows.Controls.Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, priorBySystemRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /*Name: Michael Figueroa
        Function Name: Export_Click
        Purpose: Excel export event handler (this method will no longer exist after the excel export method is moved to Helper class)
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable reports, DataTable historyTable
        Algorithm: reports and historyTable DataTables are filled, then the helper ToExcelClosedXML method completes the export.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - Mike would like to eliminate as part of code cleanup
        Assistance Received: N/A
        */
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(reportQuery, con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(reports);
                    }

                    Helper.ToExcelClosedXML(reports);
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
        Function Name: StratCheckBox_Click
        Purpose: Sets whether or not the stategic task checkbox is clicked
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: if the checkBox is clicked, then includeStrategicTask equals true, else, false; then BindDataGrid is called
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20 - Mike would like to eliminate as part of code cleanup
        Assistance Received: N/A
        */
        private void StratCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (StratCheckBox.IsChecked.Value)
            {
                includeStrategicTasks = true;
            }
            else
            {
                includeStrategicTasks = false;
            }
            BindDataGrid(SystemComboBox.SelectedItem.ToString());
        }
    }
}