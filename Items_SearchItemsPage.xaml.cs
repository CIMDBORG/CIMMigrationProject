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

namespace WpfApp1
{
    public partial class Items_SearchItemsPage : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery; //query used for excel export
        private string title_; //title
        private string cat_; //category
        private string bid_; //BCTI number
        private string user_; //Last name
        private string system_; //Sys_Impact
        private string status_; //status
        private string statusFilter; //string that stores the [Status] part of the WHERE clause
        private DataTable searchResults; //DataGrid that has the search results


        /*Name: Michael Figueroa
        Function Name: Items_SearchItemsPage
        Purpose: Constructor for the Items_SearchItemsPage form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: Calls FillSystemComboBox, collapses the Report datagrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public Items_SearchItemsPage(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            Report.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: Search
        Purpose: Builds query for search, then calls BindDataGrid
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: first, the member variables are assigned such as title and cat_.; then, if index 0 ("Open) is chosen from status combobox, then the statuses that are closed are excluded out in the WHERE clause; else
        the closed issues are chosen. Then BindDataGrid is called
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Search()
        {
            title_ = TitleBox.Text.ToString();
            //if null, then the cat_ string is empty to avoid exception error; else value vhosen from combobox is assigned to cat_ variable
            if (CategoryComboBox.SelectedItem == null)
            {
                cat_ = "";
            }
            else
            {
                cat_ = CategoryComboBox.SelectedItem.ToString();
            }
            bid_ = BIDBox.Text.ToString();
            user_ = User.Text.ToString();

            if (StatusComboBox.SelectedItem == null)
            {
                status_ = "";
            }
            else
            {
                status_ = StatusComboBox.SelectedItem.ToString();
            }


            if (SystemComboBox.SelectedItem == null)
            {
                system_ = "";
            }

            else
            {
                system_ = SystemComboBox.SelectedItem.ToString();
            }

            if (StatusComboBox.SelectedIndex == 0)
            {
                statusFilter = " (New_Issues.[Status] NOT LIKE '%closed%' " +
                       "AND New_Issues.[Status] NOT LIKE '%Implemented%' " +
                       "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') ";
            }
            else if (StatusComboBox.SelectedIndex == 1)
            {
                statusFilter = " (New_Issues.[Status] LIKE '%closed%' " +
                       "OR New_Issues.[Status] LIKE '%Implemented%' " +
                       "OR New_Issues.[Status] LIKE '%dropped%' OR New_Issues.[Status] LIKE '%deferred%') ";
            }
            else
            {
                statusFilter = "";
            }

            Report.Visibility = Visibility.Visible;

            BindDataGrid(title_, cat_, bid_, user_, system_, statusFilter);

        }

        /*Name: Michael Figueroa
        Function Name: Search
        Purpose: Event handler for search button
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls Search()
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        //Query process: The base of the query is instantiated as a string object, which is then used to instantiate a stringbuilder object
        //From the stringbuilder object, we append to the query based on whether certain filters are applied to the search or not i.e system, status, etc.
        /*Name: Michael Figueroa
        Function Name: Search
        Purpose: Builds query for search, then calls BindDataGrid
        Parameters: string[] user_data
        Return Value: None
        Local Variables: query, queryBuilder, endQuery
        Algorithm: first, the base of the query is assigned onto variable string query; query is used to instantiate a stringbuilder object; then the if clauses determine whether or not title, TFS_BC_HDFS_Num, etc. are
        included in the WHERE clause; endQuery then stores the complete query. Then the query results are binded to the datagrid to provide user search results.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid(string title, string category, string bid, string user, string system, string status)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query;
                    query = "SELECT ID, Priority_Number, Sys_Impact as [System], Category, TFS_BC_HDFS_Num as BID, " +
                                "Assigned_To as [Owner], CONVERT(date, Opened_Date) as Opened_Date, [Status], Title, " +
                                "Impact, IIf((([Status] != 'Closed') AND ([Status] != 'Implemented') AND ([Status] != 'Dropped') AND ([Status] != 'Deferred')), DATEDIFF(DAY, Opened_Date, GETDATE()), NULL) as [Days] " +
                                "FROM New_Issues Where ";

                    StringBuilder querybuilder = new StringBuilder(query);
                    if (bid.Length > 0)
                    {
                        querybuilder.Append(" (TFS_BC_HDFS_Num = " + bid + ") ");
                    }

                    if (title.Length > 0)
                    {
                        if (bid.Length > 0)
                        {
                            querybuilder.Append(" AND ");
                        }
                        querybuilder.Append(" (Title LIKE '%" + title + "%') ");
                    }
                    if (category.Length > 0)
                    {
                        if (bid.Length > 0 || title.Length > 0)
                        {
                            querybuilder.Append(" AND ");
                        }
                        querybuilder.Append(" (Category = '" + category + "') ");
                    }

                    if (user.Length > 0)
                    {
                        if (bid.Length > 0 || title.Length > 0 || category.Length > 0)
                        {
                            querybuilder.Append(" AND ");
                        }

                        querybuilder.Append(" (Assigned_To LIKE '%" + user + "%') ");
                    }

                    if (system.Length > 0)
                    {
                        if (bid.Length > 0 || title.Length > 0 || category.Length > 0 || user.Length > 0)
                        {
                            querybuilder.Append(" AND ");
                        }
                        querybuilder.Append(" (Sys_Impact = '" + system + "') ");
                    }

                    if (status.Length > 0)
                    {
                        if (bid.Length > 0 || title.Length > 0 || category.Length > 0 || user.Length > 0 || system.Length > 0)
                        {
                            querybuilder.Append(" AND ");
                        }
                        querybuilder.Append(status);
                    }

                    string endQuery = querybuilder.ToString();
                    reportQuery = endQuery;
                    con.Open();
                    SqlCommand cmd = new SqlCommand(endQuery, con);

                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                    Report.ItemsSource = dt.DefaultView;
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No Items Found!");
                    }

                    searchResults = dt;
                }

                catch (SqlException ex)
                {
                    MessageBox.Show("Please Fill in At Least One of the Fields in Order To Search");
                    MessageBox.Show(ex.ToString());
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
        public void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                priorBySystemRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);

                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, priorBySystemRow, IDList);
                editRecord.Show();
                //MessageBox.Show(priorBySystemRow[1].ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        /*Name: Michael Figueroa
         Function Name: Window_Loaded
         Purpose: Event handler for when window loads
         Parameters: Auto-generated
         Return Value: None
         Local Variables: DataRowView agingItemsRow
         Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - This method will be simplified by Mike at a later date
         Assistance Received: N/A
         */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.Items.Add("BC/TI");
            CategoryComboBox.Items.Add("Defect");
            CategoryComboBox.Items.Add("HDFS");
            CategoryComboBox.Items.Add("Inquiry");
            CategoryComboBox.Items.Add("Issue");
            CategoryComboBox.Items.Add("Strategic Task");
            CategoryComboBox.Items.Add("Task");

            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Closed");

            Helper.FillSystemComboBoxNoAll(SystemComboBox);
        }

        /*Name: Michael Figueroa
         Function Name: ClearButton_Click
         Purpose: Event handler for when window loads
         Parameters: Auto-generated
         Return Value: None
         Local Variables: messageBoxResult
         Algorithm: Clears search results
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20 - This method will be simplified by Mike at a later date
         Assistance Received: N/A
         */
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Clear Search Form?", "Clear?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                TitleBox.Text = "";
                CategoryComboBox.Text = "";
                BIDBox.Text = "";
                User.Text = "";
                SystemComboBox.Text = "";
                StatusComboBox.Text = "";
            }
            //pressing enter results in search being initiated
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Helper.ToExcelClosedXML(searchResults);
        }
    }
}