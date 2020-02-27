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
    /// Interaction logic for SourceDetailReport.xaml
    /// </summary>
    public partial class InterimSourceDetailReport : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//SQL Connection string from App.config

        /*Name: Michael Figueroa
        Function Name: InterimSaturdayScenariosReport
        Purpose: InterimSaturdayScenariosReport Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls BindDataGrid
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimSourceDetailReport()
        {
            InitializeComponent();
            BindDataGrid();
        }

        /*Name: Michael Figueroa
       Function Name: SrcReportQuery
       Purpose: Returns the query that will be used to bind data to SourceReport datagrid
       Parameters: None
       Return Value: N/A
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private string SrcReportQuery()
        {
            return "SELECT DISTINCT INTERIM_NI_TRACK_NUM1, [INTERIM_TEST_CASES].[INTERIM_TEST_CASE_CRITERIA] ,[INTERIM_TEST_CASES].[INTERIM_BILL_TYPE], [INTERIM_TEST_CASES].[INTERIM_CC], [INTERIM_TEST_CASES].[INTERIM_TYPE], " +
                    "[INTERIM_ASSIGNMENTS].[INTERIM_DAILY_ASSIGN], [INTERIM_ASSIGNMENTS].[INTERIM_SAT_ASSIGN],[INTERIM_TEST_CASES].[INTERIM_ALT_AUD], [INTERIM_HISTORY].[INTERIM_NI_SHIP_NUM1_STAT], " +
                    "[INTERIM_HISTORY].[INTERIM_NI_SHIP_NUM2_STAT], [INTERIM_HISTORY].[INTERIM_BI_SHIP_NUM1_STAT],[INTERIM_HISTORY].[INTERIM_BI_SHIP_NUM2_STAT], " +
                    "[INTERIM_TEST_CASES].[INTERIM_HC] FROM[INTERIM_TEST_CASES] INNER JOIN[INTERIM_ASSIGNMENTS] ON[INTERIM_TEST_CASES].[INTERIM_BILL_TYPE] = [INTERIM_ASSIGNMENTS].[INTERIM_SOURCE] " +
                    "AND[INTERIM_TEST_CASES].[INTERIM_CC] = [INTERIM_ASSIGNMENTS].[INTERIM_CC] " +
                    "INNER JOIN INTERIM_HISTORY ON[INTERIM_TEST_CASES].[INTERIM_BILL_TYPE] = INTERIM_HISTORY.[INTERIM_SOURCE] " +
                    "AND[INTERIM_TEST_CASES].[INTERIM_CC] = INTERIM_HISTORY.[INTERIM_CC];";
    }
        /*Name: Michael Figueroa
      Function Name: BindDataGrid
      Purpose: Binds results of SrcReportQuery to Source Report DataGrid - not necessary as this will be in helper
      Parameters: None
      Return Value: N/A
      Local Variables: string query, DataTable srcReportTable
      Algorithm: SrcReportQuery results fills srcReportTable, then srcReportTable info is binded to SourceReport DataGrid
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
    }
}