using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;

namespace Interim
{
    /// <summary>
    /// Interaction logic for InterimAssignments.xaml
    /// The assignments form allows managers and supervisors to assign sources to different Users
    /// They can also access source detail report and saturday scenarios report in order to assign appropriately
    /// </summary>
    public partial class InterimAssignments : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//SQL ConnectionString - found in App.config
        public List<string> names = new List<string>();//List<string> containing the names of the users who will be participating in verification

        /*Name: Michael Figueroa
        Function Name: InterimAssignments
        Purpose: InterimAssignments Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls BindDataGrid()
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimAssignments()
        {
            InitializeComponent();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: InterimAssignments
        Purpose: query that lists every scenario
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private string DailyScenarioListingQuery()
        {
             return "SELECT ID, INTERIM_CC, INTERIM_SOURCE, INTERIM_DAILY_ASSIGN, INTERIM_SAT_ASSIGN FROM INTERIM_ASSIGNMENTS;";
        }

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds Data Grid
        Parameters: None
        Return Value: None
        Local Variables: string query, DataTable dailyAssignments
        Algorithm: fills dailyAssignments with information from string query, then binds information to DataGris AssignList - this method will be obselete when we put it in Helper
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void BindDataGrid()
        {
                string query = DailyScenarioListingQuery();
                DataTable dailyAssignments = new DataTable();
            
                using (SqlConnection con = new SqlConnection(connectionString))
                    try
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand(query, con);
                        SqlDataAdapter sda = new SqlDataAdapter(cmd);
                        //fill report DataGrid with the query generated
                        using (sda)
                        {
                            sda.Fill(dailyAssignments);
                        }
                        AssignList.ItemsSource = dailyAssignments.DefaultView;
                        
                    }
            
                    catch (Exception ex)
                    {
                    System.Windows.MessageBox.Show(ex.ToString());
                    }

                    finally
                    {
                        con.Close();
                    }
            }

        /*Name: Michael Figueroa
            Function Name: SatReport_Click
            Purpose: Event handler for SatReport button click
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: None
            Algorithm: None
            Version: 2.0.0.4
            Date modified: 1/7/20
            Assistance Received: N/A
            */
        private void SatReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSaturdayScenariosReport satReport = new InterimSaturdayScenariosReport();
            satReport.Show();
        }

        /*Name: Michael Figueroa
            Function Name: SourceDetailReport_Click
            Purpose: Event handler for SourceDetail button click
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: None
            Algorithm: None
            Version: 2.0.0.4
            Date modified: 1/7/20
            Assistance Received: N/A
            */
        private void SourceDetailReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSourceDetailReport srcReport = new InterimSourceDetailReport();
            srcReport.Show();
        }

        /*Name: Michael Figueroa
            Function Name: DailyComboBox_SelectionChanged
            Purpose: Event handler when DailyComboBox selected item changes - this assigns scenarios to users
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: DataRowView reportRow, string assignment, string updateQuery
            Algorithm: The row in which the combobox is changed is retrieved and assigned to reportRow; then string assignment is given a value based on what name the user chose
            from DailyComboBox; then updateQuery is assigned a value using the assignment and reportRow["ID"] values (ID being the ID of the scenario); and then normal SQL 
            C# procedure executes updateQuery in the backend, and BindDataGrid is called to refresh the datagrid.
            Version: 2.0.0.4
            Date modified: 1/7/20
            Assistance Received: N/A
            */
        private void DailyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView reportRow = (DataRowView)((ComboBox)e.Source).DataContext;
            string assignment = ((ComboBox)e.Source).SelectedValue.ToString();
            string updateQuery = "UPDATE INTERIM_ASSIGNMENTS SET INTERIM_DAILY_ASSIGN = '" + assignment + "' WHERE ID = '" + reportRow["ID"] + "';";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand dailyCmd = new SqlCommand(updateQuery, connection);
                    dailyCmd.ExecuteNonQuery();
                    BindDataGrid();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    connection.Close();
                }
        }

        /*Name: Michael Figueroa
            Function Name: WeeklyAssign_SelectionChanged
            Purpose: Event handler when WeeklyAssign combobox selected item changes - this assigns scenarios to users
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: DataRowView reportRow, string assignment, string updateQuery
            Algorithm: The row in which the combobox is changed is retrieved and assigned to reportRow; then string assignment is given a value based on what name the user chose
            from WeeklyAssign combobox; then updateQuery is assigned a value using the assignment and reportRow["ID"] values (ID being the ID of the scenario); and then normal SQL 
            C# procedure executes updateQuery in the backend, and BindDataGrid is called to refresh the datagrid.
            Version: 2.0.0.4
            Date modified: 1/7/20
            Assistance Received: N/A
            */
        private void WeeklyAssign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView reportRow = (DataRowView)((ComboBox)e.Source).DataContext;
            string assignment = ((ComboBox)e.Source).SelectedValue.ToString();
            string updateQuery = "UPDATE INTERIM_ASSIGNMENTS SET INTERIM_SAT_ASSIGN = '" + assignment + "' WHERE ID = '" + reportRow["ID"] + "';";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand weeklyCmd = new SqlCommand(updateQuery, connection);
                    weeklyCmd.ExecuteNonQuery();
                    BindDataGrid();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    connection.Close();
                }
        }
    }   
}