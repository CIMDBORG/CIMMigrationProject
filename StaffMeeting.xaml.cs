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
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;
        private DataRowView reportRow;

        public StaffMeeting(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            BindDataGrid();
        }

        public string StaffMeetingQuery()
        {
            return "SELECT ID, Assigned_To, Opened_Date, Title, Supporting_Details, [Status], Due_Date, Completed_Date, Internal_Notes FROM New_Issues WHERE ManagerMeeting = 1 AND Opened_Date > '1/1/2019';";
        }

        public void BindDataGrid()
        {
            DataTable reports = new DataTable();
            FillReportTable(reports);
        }

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
