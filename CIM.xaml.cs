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
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for CIM.xaml
    /// </summary>
    public partial class CIM : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView cimBySystemRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery;
        public CIM(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            BindDataGrid();
        }

        public void BindDataGrid()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query = "SELECT Priority_Number, Sys_Impact as [System], Category, TFS_BC_HDFS_Num as BID_ID, " +
                                    "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy') AS [Opened Date], [Status], Title, " +
                                    "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as [Days], ID " +
                                    "FROM New_Issues Where (Sys_Impact like '%CIM%' AND [Status] NOT LIKE '%closed%' AND [Status] NOT LIKE '%implemented%' AND [Status] NOT LIKE '%dropped%' AND[Status] NOT LIKE '%deferred%') " +
                                    "ORDER BY ID DESC;";

                    reportQuery = query;

                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);

                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                    Report.ItemsSource = dt.DefaultView;
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

        private void BindHistoryGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    //Query that generates individual full status history, to be displayed if the user wishes
                    string query = "SELECT format(EntryDate, 'MM/dd/yyyy') AS EntryDateHistory, New_StatusNote AS NewStatus, [Status] AS History_Status, History.TaskNum AS TaskNum " +
                                   "FROM History WHERE TaskNum = " + TaskNum + " ORDER BY History.EntryDate DESC;";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                cimBySystemRow = (DataRowView)((Button)e.Source).DataContext;

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //cimBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, cimBySystemRow);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            //Generates an empty excel document 
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook wb = null;

            object missing = Type.Missing;
            Microsoft.Office.Interop.Excel.Worksheet ws = null;

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(reportQuery, con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable Reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(Reports);
                    }

                    string historyQuery = "SELECT History.TaskNum, FORMAT(History.EntryDate, 'MM/dd/yyyy') AS EntryDate, History.New_StatusNote AS LatestStatusNote, History.[Status] AS LatestStatus " +
                                            "FROM New_Issues INNER JOIN History ON History.TaskNum = New_Issues.ID WHERE New_Issues.Sys_Impact LIKE '%CIM%' " +
                                            "AND New_Issues.[Status] NOT LIKE '%closed%' AND New_Issues.[Status] NOT LIKE '%Implemented%' AND New_Issues.[Status] NOT LIKE '%dropped%' " +
                                            "AND New_Issues.[Status] NOT LIKE '%deferred%' ORDER BY History.EntryDate DESC;";

                    SqlCommand cmdTwo = new SqlCommand(historyQuery, con);
                    DataTable History = new DataTable();
                    SqlDataAdapter sdaTwo = new SqlDataAdapter(cmdTwo);

                    using (sdaTwo)
                    {
                        sdaTwo.Fill(History);
                    }

                    //A list object that will store each TaskNum from the rows generated by hQueryOne query
                    List<int> taskNums = new List<int>();

                    int taskOne = History.Rows[0].Field<int>("TaskNum"); //the TaskNum at position 0 in historyFull
                    int rowCounter = 0; //Counter that tells us what position we are in in datatable historyFull
                    //int numTaskNums = taskNums.Distinct().Count(); 
                    //will tell us what the currentTasknum is, will determine whether the row is exported to excel or not
                    int currentTask;

                    //The DataTable that will displayed to the user in excel file, with the most recent statuses
                    DataTable historyRecent = new DataTable();
                    //Columns that will be displayed in excel file
                    DataColumn dc1 = new DataColumn("TaskNum");
                    DataColumn dc2 = new DataColumn("EntryDate");
                    DataColumn dc3 = new DataColumn("LatestStatusNote");
                    DataColumn dc4 = new DataColumn("LatestStatus");

                    historyRecent.Columns.Add(dc1);
                    historyRecent.Columns.Add(dc2);
                    historyRecent.Columns.Add(dc3);
                    historyRecent.Columns.Add(dc4);
                    //Add rows to historyRecent based on condition that a row with that taskNum isn't already imported into the table

                    foreach (DataRow dr in History.Rows)
                    {
                        currentTask = History.Rows[rowCounter].Field<int>("TaskNum");
                        if ((!taskNums.Contains(currentTask)) || rowCounter == 0)
                        {
                            taskNums.Add(currentTask);
                            historyRecent.ImportRow(dr);
                        }
                        rowCounter++;
                    }

                    excel = new Microsoft.Office.Interop.Excel.Application();
                    wb = excel.Workbooks.Add();
                    ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

                    for (int idx = 0; idx < historyRecent.Columns.Count; idx++)
                    {
                        ws.Range["L1"].Offset[0, idx].Value = historyRecent.Columns[idx].ColumnName;
                    }

                    for (int idx = 0; idx < historyRecent.Rows.Count; idx++)
                    {

                        ws.Range["L2"].Offset[idx].Resize[1, historyRecent.Columns.Count].Value = historyRecent.Rows[idx].ItemArray;
                    }

                    for (int idx = 0; idx < Reports.Columns.Count; idx++)
                    {
                        ws.Range["A1"].Offset[0, idx].Value = Reports.Columns[idx].ColumnName;
                    }

                    for (int idx = 0; idx < Reports.Rows.Count; idx++)
                    {
                        ws.Range["A2"].Offset[idx].Resize[1, Reports.Columns.Count].Value = Reports.Rows[idx].ItemArray;
                    }

                    ws.Columns.AutoFit();
                    excel.Visible = true;
                    wb.Activate();
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
    }
}
