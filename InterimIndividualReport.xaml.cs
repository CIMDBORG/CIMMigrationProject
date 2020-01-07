using System;
using System.Collections.Generic;
using System.Data;
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
        string dailyAssign;

        public InterimIndividualReport()
        {
            InitializeComponent();
            FillAssignedComboBox(AssignedCombobox);
            AssignedCombobox.SelectedIndex = 0;
        }

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

        //Number of tracking numbers that have been verified per person
        private string VerifiedQry(string assigned)
        {
            return "Select Count([INTERIM_NI_SHIP_NUM1_STAT]) + Count([INTERIM_NI_SHIP_NUM2_STAT]) + Count([INTERIM_BI_SHIP_NUM1_STAT]) + Count([INTERIM_BI_SHIP_NUM2_STAT]) " +
                 "from INTERIM_HISTORY INNER JOIN INTERIM_ASSIGNMENTS " +
                "ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE AND INTERIM_HISTORY.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
                "WHERE(INTERIM_ASSIGNMENTS.INTERIM_DAILY_ASSIGN = 'Pawel') AND([INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL " +
                "OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL);";
        }

        //number of tracking numbers that have been assigned by person
        private string TotalScenarios(string assigned)
        {
            return "Select Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_BI_TRACK_NUM1]) + Count([INTERIM_BI_TRACK_NUM2]) " +
            "from INTERIM_TEST_CASES INNER JOIN INTERIM_ASSIGNMENTS " +
            "ON(INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
            "WHERE(INTERIM_ASSIGNMENTS.INTERIM_DAILY_ASSIGN = '" + assigned + "' AND INTERIM_TEST_CASES.INTERIM_TYPE = 'Daily') AND(([INTERIM_NI_TRACK_NUM1] Like '1%') OR " +
            "([INTERIM_NI_TRACK_NUM2] Like '1%') OR ([INTERIM_BI_TRACK_NUM1] Like '1%') OR ([INTERIM_BI_TRACK_NUM2] Like '1%'));";

        }

        private string RemainingScenarios(string assigned)
        {
            return "Select(Select Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_BI_TRACK_NUM1]) + Count([INTERIM_BI_TRACK_NUM2]) " +
                    "from INTERIM_TEST_CASES INNER JOIN INTERIM_ASSIGNMENTS ON(INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE " +
                    "AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) WHERE(INTERIM_ASSIGNMENTS.INTERIM_DAILY_ASSIGN = 'Pawel' AND INTERIM_TEST_CASES.INTERIM_TYPE = 'Daily') " +
                    "AND(([INTERIM_NI_TRACK_NUM1] Like '1%') OR ([INTERIM_NI_TRACK_NUM2] Like '1%') OR ([INTERIM_BI_TRACK_NUM1] Like '1%') OR ([INTERIM_BI_TRACK_NUM2] Like '1%'))) " +
                    "-(Select Count([INTERIM_NI_SHIP_NUM1_STAT]) + Count([INTERIM_NI_SHIP_NUM2_STAT]) + Count([INTERIM_BI_SHIP_NUM1_STAT]) + Count([INTERIM_BI_SHIP_NUM2_STAT]) " +
                    "from INTERIM_HISTORY INNER JOIN INTERIM_ASSIGNMENTS ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE " +
                    "AND INTERIM_HISTORY.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) WHERE(INTERIM_ASSIGNMENTS.INTERIM_DAILY_ASSIGN = 'Pawel') AND([INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL " +
                    "OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL));";
        }

        private void AssignedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dailyAssign = AssignedCombobox.SelectedItem.ToString();
            DataTable indReport = new DataTable();
            indReport = Helper.BindDataGrid(indReport, RemainingScenarios(dailyAssign));
            IndRpt.ItemsSource = indReport.DefaultView;
        }
    }
}