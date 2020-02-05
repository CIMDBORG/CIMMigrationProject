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
        private string[] arr;
        public Managers(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }

        private void ManagerReview_Click(object sender, RoutedEventArgs e)
        {
            ManagerReview managerReview = new ManagerReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(managerReview);
            itemsWindow.Show();
        }

        private void MainMenubutton_Click(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
        }

        private void BusinessCases_Click(object sender, RoutedEventArgs e)
        {
            BusinessCases businessCases = new BusinessCases(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(businessCases);
            itemsWindow.Show();
        }

        //If user clicks yes, the aging items report will run with timestamps, if not, then the aging items report will run in view-only mode without marking timestamps on the report
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

        private void HotTopics_Click(object sender, RoutedEventArgs e)
        {
            HotTopics hotTopics = new HotTopics(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(hotTopics);
            itemsWindow.Show();
        }

        private void RegionReview_Click(object sender, RoutedEventArgs e)
        {
            RegionReview regionReview = new RegionReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(regionReview);
            itemsWindow.Show();
        }

        private void ManagerTasks_Click(object sender, RoutedEventArgs e)
        {
            ManagerTasks managerTasks = new ManagerTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(managerTasks);
            itemsWindow.Show();
        }
        //The following methods handle the "My Talent" button
        //These methods populate the datatables that are exported to excel as worksheets
        //method that populates agingReport datatable

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
        //prepares mangerReview datatable for excel export
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

        private DataTable Count_Of_Manager_Review_Only()
        {
            DataTable countOfManagerReview = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {

                    string countofManQuery = "SELECT New_Issues.Assigned_To, [Updates Done] AS[Items Not Needing An Update], COUNT(ID) AS[Total Open Items], " +
                                                "CAST([Updates Done] as decimal(12,1))/ CAST(COUNT(ID) as decimal(12,1)) * 100 AS[% Effectiveness] " +
                                                "FROM New_Issues INNER JOIN(SELECT COUNT(ID) AS [Updates Done], Assigned_To FROM New_Issues WHERE Manager_Update_Bit = 0 AND Opened_Date > '1/1/2019' GROUP BY Assigned_To) n1 " +
                                                "ON New_Issues.Assigned_To = n1.Assigned_To WHERE Opened_Date > '1/1/2019' GROUP BY New_Issues.Assigned_To, [Updates Done];";

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

        //Execute this using a SQL Reader
        private string LatestUpdateDate(string dates)
        {
            string latestDate;

            char delimiter = ';';
            string[] sys = dates.Split(delimiter);
            latestDate = sys[sys.Length - 1];
            return latestDate;
        }


        //Total number of user updates made per issue
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



        //helper method that executes excel export



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

        private void MyTalent_Click(object sender, RoutedEventArgs e)
        {
            ToExcel();
        }

        private void StrategicTasks_Click(object sender, RoutedEventArgs e)
        {
            StrategicTasks strategicTasks = new StrategicTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(strategicTasks);
            itemsWindow.Show();
        }

        private void AdHoc_Click(object sender, RoutedEventArgs e)
        {
            AdHoc adhoc = new AdHoc(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(adhoc);
            itemsWindow.Show();
        }

        private void WeeklyReview_Click(object sender, RoutedEventArgs e)
        {
            WeeklyReview weeklyReview = new WeeklyReview(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(weeklyReview);
            itemsWindow.Show();
        }

        private void StaffMeeting_Click(object sender, RoutedEventArgs e)
        {
            StaffMeeting staffMeeting = new StaffMeeting(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(staffMeeting);
            itemsWindow.Show();
        }
    }
}
