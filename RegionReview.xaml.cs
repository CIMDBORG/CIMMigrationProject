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
        private string reportQuery; 
        public RegionReview(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillRegionComboBox();
            ReportHelper.FillStatusComboBoxWithAll(StatusComboBox);
            StatusComboBox.SelectedIndex = 0;
            RegionComboBox.SelectedIndex = 0;
        }

        private void FillRegionComboBox()
        {
            RegionComboBox.Items.Add("Americas");
            RegionComboBox.Items.Add("Asia");
            RegionComboBox.Items.Add("Eur");
            RegionComboBox.Items.Add("Canada");
            RegionComboBox.Items.Add("US");
        }

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

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }
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

        private void FillRegionTable(DataTable regionTable)
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

        public void BindDataGrid()
        {
            DataTable history = new DataTable();
            FillHistoryTable(history);
            DataTable region = new DataTable();
            FillRegionTable(region);
        }

        private DataTable SortByPriorityNum(DataTable history)
        {
            DataView tableSort = history.DefaultView;
            tableSort.Sort = "Priority_Number ASC";
            DataTable sortedTable = tableSort.ToTable();
            return sortedTable;
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

        //loads the full history data onto the grid for each item
        private void BindHistoryGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    //Query that generates individual full status history, to be displayed if the user wishes
                    string query = "SELECT format(EntryDate, 'MM/dd/yyyy') AS EntryDateHistory, New_StatusNote AS NewStatus, [Status] AS History_Status, History.TaskNum AS TaskNum " +
                                   "FROM History WHERE TaskNum = " + TaskNum + " ORDER BY History.EntryDate DESC;";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
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

            private void Lbx1_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(RegionComboBox, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }

            private void Region_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(Region, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }

            private void HistoryRecent_ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                ScrollViewer _listboxScrollViewer1 = GetDescendantByType(HistoryRecent, typeof(ScrollViewer)) as ScrollViewer;
                ScrollViewer _listboxScrollViewer2 = GetDescendantByType(Region, typeof(ScrollViewer)) as ScrollViewer;
                _listboxScrollViewer2.ScrollToVerticalOffset(_listboxScrollViewer1.VerticalOffset);
            }
        }
    }

