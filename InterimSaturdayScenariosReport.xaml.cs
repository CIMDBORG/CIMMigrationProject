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
using System.Windows.Shapes;

namespace Interim
{
    /// <summary>
    /// Interaction logic for SaturdayScenariosReport.xaml
    /// </summary>
    public partial class InterimSaturdayScenariosReport : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

        public InterimSaturdayScenariosReport()
        {
            InitializeComponent();
            BindDataGrid();
        }

        private string Wkd_Query()
        {
            return "Select INTERIM_ID, INTERIM_BILL_TYPE, INTERIM_TEST_CASE_CRITERIA, INTERIM_NI_SHIP_NUM1, INTERIM_NI_TRACK_NUM1, " +
                "INTERIM_NI_SHIP_NUM2, INTERIM_NI_TRACK_NUM2, INTERIM_BI_SHIP_NUM1, INTERIM_BI_TRACK_NUM1, INTERIM_BI_SHIP_NUM2, " +
                "INTERIM_BI_TRACK_NUM2, INTERIM_ASSIGNED_NAME from INTERIM_TEST_CASES WHERE INTERIM_TYPE = 'WEEKLY' " +
                "AND (INTERIM_BILL_TYPE IS NOT NULL OR INTERIM_BILL_TYPE <>0) " +
                "AND (INTERIM_TEST_CASE_CRITERIA IS NOT NULL OR INTERIM_TEST_CASE_CRITERIA <>0) ";
        }
        private void BindDataGrid()
        {
            string query = Wkd_Query();
            DataTable srcReportTable = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(srcReportTable);
                    }
                    SourceReport.ItemsSource = srcReportTable.DefaultView;
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

        private void Source_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataRowView reportRow = (DataRowView)((TextBox)e.Source).DataContext;
            string name = ((TextBox)e.Source).Text.ToString();
            string cc = reportRow["CC"].ToString();
            string updateQuery = "UPDATE INTERIM_TEST_CASES SET INTERIM_ASSIGNED_ALT = 1, INTERIM_ASSIGNED_NAME = '" + name + "' WHERE INTERIM_ID = '" + reportRow["ID"] + "';";
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand dailyCmd = new SqlCommand(updateQuery, connection);
                    dailyCmd.ExecuteNonQuery();
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
