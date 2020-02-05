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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for StrategicTasks.xaml
    /// </summary>
    public partial class StrategicTasks : Page
    {
        private string[] arr;     //local variable to store login-based user data
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private DataRowView StrategicRow;       //local variable to store the row of data in the 'Business Cases' DataGrid
        private string reportQuery;

        public StrategicTasks(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            FillStatusComboBox();
            StatusComboBox.SelectedIndex = 0;
            BindDataGrid();
        }

        private void BindDataGrid()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query;
                    if (StatusComboBox.SelectedItem.ToString() == "Open")
                    {
                         query = "SELECT TFS_BC_HDFS_Num AS BID#, Assigned_To, ID, FORMAT(Opened_Date, 'MM/dd/yyyy') AS Opened_Date, Title, Supporting_Details, Internal_Notes, DATEDIFF(day, Opened_Date, GETDATE()) AS Age, " +
                            "FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, " +
                        "AnnualBenefit, Deliverables, Benefits, Annual_Cost_Savings, HP, Hours, [Status] FROM New_Issues WHERE " +
                        "(Category = 'Strategic Task') AND (New_Issues.[Status] = 'Open') ORDER BY TFS_BC_HDFS_Num ASC;";
                    }
                    else if(StatusComboBox.SelectedItem.ToString() == "Completed")
                    {
                        query = "SELECT TFS_BC_HDFS_Num AS BID#, Assigned_To, ID, FORMAT(Opened_Date, 'MM/dd/yyyy') AS Opened_Date, Title, Supporting_Details, Internal_Notes, DATEDIFF(day, Opened_Date, GETDATE()) AS Age, " +
                            "FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, " +
                        "AnnualBenefit, Deliverables, Benefits, Annual_Cost_Savings, HP, Hours, [Status] FROM New_Issues WHERE " +
                        "(Category = 'Strategic Task') AND (New_Issues.[Status] = 'Completed') ORDER BY TFS_BC_HDFS_Num ASC;";
                    }
                    else
                    {
                        query = "SELECT TFS_BC_HDFS_Num AS BID#, Assigned_To, ID, FORMAT(Opened_Date, 'MM/dd/yyyy') AS Opened_Date, Title, Supporting_Details, Internal_Notes, DATEDIFF(day, Opened_Date, GETDATE()) AS Age, " +
                            "FORMAT(Due_Date, 'MM/dd/yyyy') AS Due_Date, FORMAT(Completed_Date, 'MM/dd/yyyy') AS Completed_Date, " +
                        "AnnualBenefit, Deliverables, Benefits, Annual_Cost_Savings, HP, Hours, [Status] FROM New_Issues WHERE " +
                        "(Category = 'Strategic Task') AND (New_Issues.[Status] = 'Not Assigned') ORDER BY TFS_BC_HDFS_Num ASC;";
                    }


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

        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("Open");
            StatusComboBox.Items.Add("Completed");
            StatusComboBox.Items.Add("Not Assigned");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                StrategicRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDList = Helper.FillIDList(reportQuery);
                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, StrategicRow, IDList);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            NewRecord newRecord = new NewRecord(arr);
            newRecord.Show();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }
    }
}