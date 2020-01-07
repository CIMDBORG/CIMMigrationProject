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
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        public List<string> names = new List<string>();

        public InterimAssignments()
        {
            InitializeComponent();
            BindDataGrid();
        }

        //query that lists every scenario
        private string DailyScenarioListingQuery()
        {
             return "SELECT ID, INTERIM_CC, INTERIM_SOURCE, INTERIM_DAILY_ASSIGN, INTERIM_SAT_ASSIGN FROM INTERIM_ASSIGNMENTS;";
        }
 
        //Data binding
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

        //Saturday scenario report
        private void SatReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSaturdayScenariosReport satReport = new InterimSaturdayScenariosReport();
            satReport.Show();
        }

        private void SourceDetailReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSourceDetailReport srcReport = new InterimSourceDetailReport();
            srcReport.Show();
        }

        //When the combobox for the daily assignments changes, the scenario is re-assigned in the backend
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

        //When the weekly combobox changes, the scenario is re-assigned
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