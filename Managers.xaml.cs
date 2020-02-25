using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
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
using System.Diagnostics;
using System.Threading;
using ClosedXML.Excel;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Globalization;
using Calendar = System.Globalization.Calendar;
using OfficeOpenXml;
using WpfApp2;
using WpfApp1;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for Managers.xaml
    /// </summary>
    public partial class Managers : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        private string[] arr;//array containing user data

        /*Name: Michael Figueroa
        Function Name: Managers
        Purpose: Constructor for Managers
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public Managers(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }

        /*Name: Michael Figueroa
        Function Name: ManagerReview_Click
        Purpose: Opens ManagerReview form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ManagerReview_Click(object sender, RoutedEventArgs e)
        {
            ManagerReview managerReview = new ManagerReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(managerReview);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: MainMenubutton_Click
        Purpose: Re-opens MainMenu form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None - this method has to be tweaked in future
        Version: 3.0.0.1
        Date modified: February 2020
        Assistance Received: N/A
        */
        private void MainMenubutton_Click(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
        }

        /*Name: Michael Figueroa
        Function Name: BusinessCases_Click
        Purpose: Opens BusinessCases form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 3.0.0.1
        Date modified: February 2020
        Assistance Received: N/A
        */
        private void BusinessCases_Click(object sender, RoutedEventArgs e)
        {
            BusinessCases businessCases = new BusinessCases(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(businessCases);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: BusinessCases_Click
        Purpose: Opens BusinessCases form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: If user clicks yes on MessageBoxResult, the aging items report will run with timestamps, if not, 
        then the aging items report will run in view-only mode without marking timestamps on the report
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private void AgingItems_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Run Aging Items Report With TimeStamps?", "Aging Items", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                ReportHelper.InsertIntoAging(ReportHelper.FillAging());
                AgingItems agingItems = new AgingItems(arr);
                ReportItemsWindow itemsWindow = new ReportItemsWindow(agingItems);
                itemsWindow.Show();
            }

            else if (result == MessageBoxResult.No)
            {
                AgingItems agingItems = new AgingItems(arr);
                ReportItemsWindow itemsWindow = new ReportItemsWindow(agingItems);
                itemsWindow.Show();
            }
        }

        /*Name: Michael Figueroa
        Function Name: HotTopics_Click
        Purpose: Opens HotTopics form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private void HotTopics_Click(object sender, RoutedEventArgs e)
        {
            HotTopics hotTopics = new HotTopics(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(hotTopics);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: RegionReview_Click
        Purpose: Opens RegionReview form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private void RegionReview_Click(object sender, RoutedEventArgs e)
        {
            RegionReview regionReview = new RegionReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(regionReview);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
       Function Name: ManagerTasks_Click
       Purpose: Opens ManagerTasks form
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: Before 1/1/20
       Assistance Received: N/A
       */
        private void ManagerTasks_Click(object sender, RoutedEventArgs e)
        {
            ManagerTasks managerTasks = new ManagerTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(managerTasks);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: AgingItemsExcel
        Purpose: Populates agingReport datatable
        Parameters: None
        Return Value: DataTable AgingReport
        Local Variables: DataTable agingReport, stirng aging, int rowCounter, DataTable countOwn, DateTimeFormatInfo dfi, Calendar cal,
        DateTime currentDate, DataColumn weekNumber, DataColumn weekNumberCount, DataColumn by52Weeks
        Algorithm: string aging is assigned by calling Helper.GetAgingHistory, countOwn is assigned using Helper.CountOfOwnerTable,
        currentDate is set to DateTime.Now, DataColumns are added to agingReport table, then, SqlDataReader reader reads cmd;
        if timeStamp value is not null, then values are added to agingReport table; else, values of 0 are added - look into this, make
        sure it is working the way it should
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private DataTable AgingItemsExcel()
        {
            DataTable agingReport = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    string aging = Helper.GetAgingHistory();
                    int rowCounter = 0;
                    SqlCommand cmd = new SqlCommand(aging, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);

                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(agingReport);
                    }
                    DataTable countOwn = Helper.CountOfOwnerTable();
                    DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                    Calendar cal = dfi.Calendar;
                    DateTime currentDate = DateTime.Now;
                    DataColumn weekNumber = new DataColumn("Week Number");
                    DataColumn weekNumberCount = new DataColumn("Weeks Item In Aging");
                    DataColumn by52Weeks = new DataColumn("% by 52 Weeks (Jan-Dec)");

                    agingReport.Columns.Add(weekNumber);
                    agingReport.Columns.Add(weekNumberCount);
                    agingReport.Columns.Add(by52Weeks);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ID = reader.GetValue(0).ToString();
                            if (!reader.IsDBNull(2))
                            {
                                DateTime timestamp = agingReport.Rows[rowCounter].Field<DateTime>("Timestamp");
                                int weekNumCount = Helper.GetWeekNoCount(ID, connectionString);
                                agingReport.Rows[rowCounter][weekNumber] = cal.GetWeekOfYear(timestamp, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                                agingReport.Rows[rowCounter][weekNumberCount] = Helper.GetWeekNoCount(ID, connectionString);
                                agingReport.Rows[rowCounter][by52Weeks] = Helper.ReturnPercentage(weekNumCount) + "%";
                                rowCounter++;
                            }
                            else
                            {
                                agingReport.Rows[rowCounter][weekNumber] = 0;
                                agingReport.Rows[rowCounter][weekNumberCount] = 0;
                                agingReport.Rows[rowCounter][by52Weeks] = 0;
                                rowCounter++;
                            }
                        }
                    }
                }

                catch (DuplicateNameException ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }

            return agingReport;
        }

        /*Name: Michael Figueroa
       Function Name: ManagerReviewOnly
       Purpose: Shows which items have not been marked for manager review (not sure, this is a little confusing)
       Parameters: None
       Return Value: DataTable managerReview
       Local Variables: DataTable managerReview, string manReview
       Algorithm: manReview is used to create new SqlCommand, then the cmd is used to fill managerReview DataTable - used for MyTalent,
       should be moved to MyTalent.xaml.cs
       Version: 2.0.0.4
       Date modified: Before 1/1/20
       Assistance Received: N/A
       */
        private DataTable ManagerReviewOnly()
        {
            DataTable managerReview = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    string manReview = "SELECT ID, Sys_Impact AS [System], Opened_Date, Assigned_To, Category, Title, Supporting_Details, Internal_Notes, Bus_Impact, " +
                        " Mgr_Notes FROM New_Issues WHERE Manager_Update_Bit = 0 AND (Opened_Date > '1/1/2019') ORDER BY Sys_Impact, Opened_Date;";

                    SqlCommand cmd = new SqlCommand(manReview, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(managerReview);
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
            return managerReview;
        }

        /*Name: Michael Figueroa
       Function Name: ManagerReviewOnly
       Purpose: Shows user statistics on percentage of items that they did not have an update requested by manager
       Parameters: None
       Return Value: DataTable managerReview
       Local Variables: DataTable countOfManagerReview, string countofManQuery
       Algorithm: countofManQuery is used to make a SqlCommand object, then the command fills countOfManagerReview table
       Version: 2.0.0.4
       Date modified: January 2020
       Assistance Received: N/A
       */
        private DataTable Count_Of_Manager_Review_Only()
        {
            DataTable countOfManagerReview = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string countofManQuery = "SELECT New_Issues.Assigned_To, [Updates Done] AS[Items Not Needing An Update], COUNT(ID) AS[Total Open Items], " +
                                            "CAST([Updates Done] as decimal(12,1))/ CAST(COUNT(ID) as decimal(12,1)) * 100 AS[% Effectiveness] " +
                                            "FROM New_Issues INNER JOIN(SELECT COUNT(ID) AS [Updates Done], Assigned_To FROM New_Issues WHERE (LEN(Mgr_Notes) = 0 OR Mgr_Notes IS NULL) " +
                                            "AND Opened_Date > '1/1/2020' GROUP BY Assigned_To) n1 " +
                                            "ON New_Issues.Assigned_To = n1.Assigned_To WHERE Opened_Date > '1/1/2020' GROUP BY New_Issues.Assigned_To, [Updates Done];";

                    SqlCommand cmd = new SqlCommand(countofManQuery, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(countOfManagerReview);
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
            return countOfManagerReview;
        }

        /*Name: Michael Figueroa
       Function Name: Updates_Per_Item
       Purpose: Total number of user updates made per issue
       Parameters: None
       Return Value: DataTable updatesPerItem
       Local Variables: DataTable updatesPerItem, DataColumn numUpdates, string updatesPer, int numUpdatesMade, string updateDates
       Algorithm: updatesPer query contained in a string object is used fill the updatesPerItem DataTable; numUpdatesMade is a result of using a delimiter in order to count how many update dates 
       are in a specific issue; this determines how many user updates have been made - once again, this may have to reevaluated
       Version: 2.0.0.4
       Date modified: January 2020
       Assistance Received: N/A
       */
        private DataTable Updates_Per_Item()
        {
            DataTable updatesPerItem = new DataTable();
            DataColumn numUpdates = new DataColumn("Num of User Updates Made");
            updatesPerItem.Columns.Add(numUpdates);
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    string updatesPer = "Select TFS_BC_HDFS_Num AS BID#, User_Update, Sys_Impact AS [System], Assigned_To FROM New_Issues WHERE Manager_Update_Bit = 1 AND User_Update != '0' ORDER BY Assigned_To;";
                    SqlCommand cmd = new SqlCommand(updatesPer, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated

                    using (sda)
                    {
                        sda.Fill(updatesPerItem);
                    }

                    //numUpdatesMade is going to be a result of using a delimiter in order to count how many update dates are in a specific issue; this determines how many user updates have been made

                    int numUpdatesMade;
                    string updateDates;
                    int i = 0;
                    using (SqlDataReader reader2 = cmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            updateDates = reader2.GetString(1);
                            if (updateDates.Length > 1)
                            {
                                numUpdatesMade = MyTalentHelper.GetUserUpdateCount(updateDates);
                                updatesPerItem.Rows[i][numUpdates] = numUpdatesMade;
                            }
                            else
                            {
                                updatesPerItem.Rows[i][numUpdates] = 0;
                            }
                            i++;
                        }
                        reader2.Close();
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
            return updatesPerItem;
        }

        /*Name: Michael Figueroa
       Function Name: ToExcel
       Purpose: Exports data to excel for MyTalent
       Parameters: None
       Return Value: None
       Local Variables: DataTable daysUntilFlagged, xlWorkbook wb
       Algorithm: daysUntilFlagged is populated using Helper.FlaggedReportTable, then calls AgingItemsExcel, ManagerReviewOnly, Count_Of_Manager_Review_Only, Updates_Per_Item methods in order to 
       fill first four sheets, with daysUntilFlagged populating data for the last. SaveFileDialog is shown, which prompts user to save worksheet where they'd like (if the file name
       written by user is not null), and then wb is disposed.
       Version: 2.0.0.4
       Date modified: Before 1/1/20
       Assistance Received: N/A
       */
        private void ToExcel()
        {
            DataTable daysUntilFlagged = Helper.FlaggedReportTable(connectionString);
            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(AgingItemsExcel(), "Aging Report");
            wb.Worksheets.Add(ManagerReviewOnly(), "Manager Review Only");
            wb.Worksheets.Add(Count_Of_Manager_Review_Only(), "Count of Manager Review Only");
            wb.Worksheets.Add(Updates_Per_Item(), "Total No. Of Updates Per Item");
            wb.Worksheets.Add(daysUntilFlagged, "No.DaysToUpdateA_FlaggedItem");

            wb.SaveAs("My Talent.xlsx");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx",
                Title = "Save an Excel File"
            };

            saveFileDialog.ShowDialog();

            if (!String.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                wb.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("File Saved As " + saveFileDialog.FileName.ToString());
            }

            wb.Dispose();
        }

        /*Name: Michael Figueroa
       Function Name: MyTalent_Click
       Purpose: Even handler for MyTalent_Click
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: Calls ToExcel
       Version: 2.0.0.4
       Date modified: Before 1/1/20
       Assistance Received: N/A
       */
        private void MyTalent_Click(object sender, RoutedEventArgs e)
        {
            ToExcel();
        }

        /*Name: Michael Figueroa
       Function Name: StrategicTasks_Click
       Purpose: Even handler for StrategicTasks_Click
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: Opens StrategicTasks form
       Version: 2.0.0.4
       Date modified: Before 1/1/20
       Assistance Received: N/A
       */
        private void StrategicTasks_Click(object sender, RoutedEventArgs e)
        {
            StrategicTasks strategicTasks = new StrategicTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(strategicTasks);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
      Function Name: AdHoc_Click
      Purpose: Even handler for AdHoc_Click
      Parameters: Auto-Generated
      Return Value: None
      Local Variables: None
      Algorithm: Opens AdHoc_Click form
      Version: 2.0.0.4
      Date modified: Before 1/1/20
      Assistance Received: N/A
      */
        private void AdHoc_Click(object sender, RoutedEventArgs e)
        {
            AdHoc adhoc = new AdHoc(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(adhoc);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: WeeklyReview_Click
        Purpose: Even handler for WeeklyReview_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Opens WeeklyReview_Click form
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private void WeeklyReview_Click(object sender, RoutedEventArgs e)
        {
            WeeklyReview weeklyReview = new WeeklyReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(weeklyReview);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
        Function Name: StaffMeeting_Click
        Purpose: Even handler for StaffMeeting_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Opens StaffMeeting_Click form
        Version: 2.0.0.4
        Date modified: Before 1/1/20
        Assistance Received: N/A
        */
        private void StaffMeeting_Click(object sender, RoutedEventArgs e)
        {
            StaffMeeting staffMeeting = new StaffMeeting(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(staffMeeting);
            itemsWindow.Show();
        }
    }
}
