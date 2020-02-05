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
    /// <summary>
    /// Interaction logic for StatusHistory.xaml
    /// </summary>
    public partial class StatusHistory : Window
    {
        public String connectionString = "Data Source=svrp0006ca66;Initial Catalog=Johnny_DB;Integrated Security=True";
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'ReportWindow' DataGrid
        private Window window;
        public int task;
        public string curSys;
        public StatusHistory(ReportsWindow reports, string[] user_data, DataRowView prioritizationBySystemResultRow)
        {
            InitializeComponent();
            window = new Window();
            window = reports;
            curSys = reports.GetComboBox();
            arr = user_data;
            priorBySystemRow = prioritizationBySystemResultRow;
            BindDataGrid(priorBySystemRow[10].ToString()); 
        }

        //*******************************************************************
        // DESCRIPTION: Function that runs the Prioritization by System query and fills the data grid with the result table.
        //              First, the SELECT query is run to pull the data on the open items. 
        //                  The system is specified by the system chosen in the combobox.
        //              Then, a SQLDataAdapter is used to fill the datatable with these results. 
        //              See Items_PrioritizationBySystemPage.xaml for more on data binding. Note that the names of the result columns
        //                  match the names of the binding columns. That is how the query result table is connected to the datagrid.
        //
        // INPUT:       string sys: this string specifies the system whose issues the user is trying to view. Is passed into the query
        //*******************************************************************
        public void BindDataGrid(String TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    string query = "SELECT FORMAT(EntryDate, 'MM/dd/yyyy') as EntryDate, New_StatusNote , [Status]" +
                                   "FROM History where TaskNum = " + TaskNum + "order by History.ID desc;"; 
                                  
                    /*"SELECT FORMAT(History.EntryDate, 'MM/dd/yyyy') as EntryDate, History.TaskNum as TaskNum, History.[New_StatusNote] as NewStatus, " +
                                          "History.[Status] as Status FROM History INNER JOIN New_Issues on History.ID = New_Issues.ID WHERE New_Issues.Sys_Impact = '" + curSys + "' AND  History.TaskNum = 2670 ORDER BY History.EntryDate desc;";*/

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
                    History.ItemsSource = dt.DefaultView;
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
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
