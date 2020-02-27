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
using WpfApp1;

namespace Interim
{
    /// <summary>
    /// Interaction logic for IndividualReport.xaml
    /// </summary>
    public partial class InterimIndividualReport : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//SQL connection string retrieved from App.config

        /*Name: Michael Figueroa
        Function Name: InterimIndividualReport
        Purpose: InterimIndividualReport Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls FillAssignedComboBox is called and assigned index 0, BindDataGrid is called
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimIndividualReport()
        {
            InitializeComponent();
            FillAssignedComboBox(AssignedCombobox);
            AssignedCombobox.SelectedIndex = 0;
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: FillAssignedComboBox
        Purpose: Fills comboBox
        Parameters: ComboBox comboBox
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void FillAssignedComboBox(ComboBox comboBox)
        {
            comboBox.Items.Add("Pawel");
            comboBox.Items.Add("Jeff");
            comboBox.Items.Add("Jan-Marie");
            comboBox.Items.Add("Chris");
            comboBox.Items.Add("Tau");
            comboBox.Items.Add("Dom");
            comboBox.Items.Add("Sam");
            comboBox.Items.Add("Brandon");
            comboBox.Items.Add("Nick");
            comboBox.Items.Add("Ellen");
        }

        /*Name: Michael Figueroa
        Function Name: VerifiedQry
        Purpose: query that shows the number of tracking numbers that have been verified per person - this may have to be looked at and improved
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private string VerifiedQry()
        {
            return "Select INTERIM_DAILY_ASSIGN, Count([INTERIM_NI_SHIP_NUM1_STAT]) + Count([INTERIM_NI_SHIP_NUM2_STAT]) + Count([INTERIM_BI_SHIP_NUM1_STAT]) + Count([INTERIM_BI_SHIP_NUM2_STAT]) " +
                 "AS Verified from INTERIM_HISTORY INNER JOIN INTERIM_ASSIGNMENTS " +
                "ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE AND INTERIM_HISTORY.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
                "WHERE([INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL " +
                "OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL) GROUP BY INTERIM_DAILY_ASSIGN;";
        }

        /*Name: Michael Figueroa
        Function Name: TotalScenarios
        Purpose: query that shows number of tracking numbers that have been assigned to a person - also needs to be examined
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private string TotalScenarios()
        {
            return "Select INTERIM_DAILY_ASSIGN, Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_BI_TRACK_NUM1]) + Count([INTERIM_BI_TRACK_NUM2]) " +
            "AS TotalScenarios from INTERIM_TEST_CASES INNER JOIN INTERIM_ASSIGNMENTS " +
            "ON(INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
            "WHERE(INTERIM_TEST_CASES.INTERIM_TYPE = 'Daily') AND(([INTERIM_NI_TRACK_NUM1] Like '1%') OR " +
            "([INTERIM_NI_TRACK_NUM2] Like '1%') OR ([INTERIM_BI_TRACK_NUM1] Like '1%') OR ([INTERIM_BI_TRACK_NUM2] Like '1%')) GROUP BY INTERIM_DAILY_ASSIGN;";

        }

        /*Name: Michael Figueroa
        Function Name: RemainingScenarios
        Purpose: this query shows how many scenarios a person has left - may also need to be examined further
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private string RemainingScenarios()
        {
            return "Select INTERIM_DAILY_ASSIGN, (Select Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_BI_TRACK_NUM1]) + Count([INTERIM_BI_TRACK_NUM2]) " +
                    "from INTERIM_TEST_CASES INNER JOIN INTERIM_ASSIGNMENTS ON(INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE " +
                    "AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) WHERE(INTERIM_TEST_CASES.INTERIM_TYPE = 'Daily') " +
                    "AND(([INTERIM_NI_TRACK_NUM1] Like '1%') OR ([INTERIM_NI_TRACK_NUM2] Like '1%') OR ([INTERIM_BI_TRACK_NUM1] Like '1%') OR ([INTERIM_BI_TRACK_NUM2] Like '1%'))) " +
                    "-(Select Count([INTERIM_NI_SHIP_NUM1_STAT]) + Count([INTERIM_NI_SHIP_NUM2_STAT]) + Count([INTERIM_BI_SHIP_NUM1_STAT]) + Count([INTERIM_BI_SHIP_NUM2_STAT]) " +
                    "from INTERIM_HISTORY INNER JOIN INTERIM_ASSIGNMENTS ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE " +
                    "AND INTERIM_HISTORY.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) WHERE(([INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL " +
                    "OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL))) AS TrkNumsLeft GROUP BY INTERIM_DAILY_ASSIGN;";
        }

        /*Name: Michael Figueroa
        Function Name: RemainingScenarios
        Purpose: BindsDataGrid using three queries - string query, string queryTwo, and string queryThree
        Parameters: None
        Return Value: string
        Local Variables: string query, string queryTwo, string queryThree, DataTable indTable
        Algorithm: Fills DataGrid IndRpt using results from query, queryTwo, and queryThree, then binds results to datagrid IndRpt
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void BindDataGrid()
        {
            string query = RemainingScenarios();
            string queryTwo = TotalScenarios();
            string queryThree = VerifiedQry();

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    DataTable indTable = new DataTable();
                    indTable.Columns.Add("TotalScenarios");
                    indTable.Columns.Add("TrkNumsLeft");
                    indTable.Columns.Add("Verified");

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);

                    using (sda)
                    {
                        sda.Fill(indTable);
                    }

                    SqlCommand cmdTwo = new SqlCommand(queryTwo, con);
                    SqlDataAdapter sdaTwo = new SqlDataAdapter(cmdTwo);
                    using (sdaTwo)
                    {
                        sdaTwo.Fill(indTable);
                    }

                    SqlCommand cmdThree = new SqlCommand(queryThree, con);
                    SqlDataAdapter sdaThree = new SqlDataAdapter(cmdThree);
                    using (sdaThree)
                    {
                        sdaThree.Fill(indTable);
                    }

                    IndRpt.ItemsSource = indTable.DefaultView;
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
        Function Name: AssignedCombobox_SelectionChanged
        Purpose: Event handler for AssignedComboBox selectionChanged
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls BindDataGrid to "refresh" datagrid
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void AssignedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid();
        }
    }
}