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
    // Will be used for Search Items functionality, did not have add anything to this page yet.
    public partial class Items_SearchItemsPage : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery;
        private string title_;
        private string cat_;
        private string bid_;
        private string user_;
        private string system_;
        private string status_;
        private string statusFilter;
        private DataTable searchResults;

        public Items_SearchItemsPage(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;

            Report.Visibility = Visibility.Collapsed;

        }

        private void Search()
        {
            title_ = TitleBox.Text.ToString();
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

        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        //Query process: The base of the query is instantiated as a string object, which is then used to instantiate a stringbuilder object
        //From the stringbuilder object, we append to the query based on whether certain filters are applied to the search or not i.e system, status, etc.
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