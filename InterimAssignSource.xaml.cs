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
    /// Interaction logic for AssignSource.xaml
    /// </summary>
    public partial class InterimAssignSource : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//Sql Connection string found in App.config

        /*Name: Michael Figueroa
        Function Name: InterimAssignSource
        Purpose: InterimAssignSource Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls BindDataGrid()
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimAssignSource()
        {
            InitializeComponent();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: SrcReportQuery
        Purpose: query that produces Source Report 
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private string SrcReportQuery()
        { 
            return "SELECT INTERIM_ID AS ID, INTERIM_TEST_CASE_CRITERIA, INTERIM_CC AS CC, INTERIM_TYPE FROM INTERIM_TEST_CASES WHERE(INTERIM_BILL_TYPE IS NULL) AND (INTERIM_ASSIGNED_NAME IS NULL) " +
                "AND((INTERIM_BI_SHIP_NUM1 IS NOT NULL AND INTERIM_BI_SHIP_NUM1 != '')" +
                "OR(INTERIM_BI_SHIP_NUM2 IS NOT NULL AND INTERIM_BI_SHIP_NUM2 != '') OR" +
                "(INTERIM_NI_SHIP_NUM1 IS NOT NULL AND INTERIM_NI_SHIP_NUM1 != '') OR(INTERIM_NI_SHIP_NUM2 IS NOT NULL AND INTERIM_NI_SHIP_NUM2 != ''));";
        }

        /*Name: Michael Figueroa
       Function Name: BindDataGrid
       Purpose: Binds SrcData using results from SrcReportQuery()
       Parameters: None
       Return Value: None
       Local Variables: string query, DataTable srcReportTable
       Algorithm: fills srcReportTable with information from string query, then uses standard SQL procedure to execute string query, then binds results to SrcData DataGrid
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void BindDataGrid()
        {
            string query = SrcReportQuery();
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
                    SrcData.ItemsSource = srcReportTable.DefaultView;
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
       Function Name: AltAssign_TextChanged
       Purpose: Event handler for AltAssign TextBox changed event
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: DataRowView reportRow, string name, string updateQuery
       Algorithm: The row in which the combobox is changed is retrieved and assigned to reportRow; then string name is given a value based on what the user types
            into AltAssign textbox; then updateQuery is assigned a value using the name and reportRow["ID"] values (ID being the ID of the scenario); and then normal SQL
            C# procedure executes updateQuery in the backend, and BindDataGrid is called to refresh the datagrid.
            NOTES ON updateQuery: So basically what happens here is
            1. we set INTERIM_ASSIGNED_ALT = 1 - this is a bit value column in INTERIM_TEST_CASES SQL table that denotes whether or not a test case is assigned to an
            alternate person (person other than the person that is assigned that source) - not the same as an alternate auditor
            2. INTERIM_ASSIGNED_NAME is set to string name value
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void AltAssign_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataRowView reportRow = (DataRowView)((TextBox)e.Source).DataContext;
            string name = ((TextBox)e.Source).Text.ToString();
            string updateQuery = "UPDATE INTERIM_TEST_CASES SET INTERIM_ASSIGNED_ALT = 1, INTERIM_ASSIGNED_NAME = '"+ name + "' WHERE INTERIM_ID = '" + reportRow["ID"] + "';";
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
