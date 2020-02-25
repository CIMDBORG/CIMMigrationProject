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

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for StaffMeeting.xaml
    /// </summary>
    public partial class StaffMeeting : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;//local variable to store login-based user data
        private DataRowView reportRow;

        /*Name: Michael Figueroa
        Function Name: StaffMeeting
        Purpose: StaffMeeting Constructor
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls BindDataGrid
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public StaffMeeting(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: StaffMeetingQuery
        Purpose: returns query to be used to bind data to StaffMeeting datagrid
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public string StaffMeetingQuery()
        {
            return "SELECT ID, Assigned_To, Opened_Date, Title, Supporting_Details, [Status], Due_Date, Completed_Date, Internal_Notes FROM New_Issues WHERE ManagerMeeting = 1 AND Opened_Date > '1/1/2019';";
        }

        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Calls FillReportTable
        Parameters: None
        Return Value: N/A
        Local Variables: DataTable reports
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void BindDataGrid()
        {
            DataTable reports = new DataTable();
            FillReportTable(reports);
        }

        /*Name: Michael Figueroa
       Function Name: FillReportTable
       Purpose: Fills DataTable table with the data that will be binded to the StaffMeeting datagrid
       Parameters: DataTable table
       Return Value: N/A
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void FillReportTable(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(StaffMeetingQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(table);
                        Report.ItemsSource = table.DefaultView;
                        Report.Visibility = Visibility.Visible;
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

        /*Name: Michael Figueroa
       Function Name: EditButton_Click
       Purpose: Event handler for edit button click
       Parameters: Auto-generated
       Return Value: None
       Local Variables: DataRowView agingItemsRow
       Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                reportRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDArray = Helper.FillIDList(StaffMeetingQuery());

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

    }
}
