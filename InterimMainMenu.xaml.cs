using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2;
using Interim;
using WpfApp1;

namespace Interim
{
    /// <summary>
    /// Interaction logic for InterimMainMenu.xaml
    /// </summary>
    public partial class InterimMainMenu : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        public string[] arr;

        public InterimMainMenu(string[] user_data)
        {
           InitializeComponent();
            arr = user_data;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (arr[6] == "User")
            {
                Assign.Visibility = Visibility.Collapsed;
            }
           TrkNumsVerified();
           ScenariosLeft();
           TotalTrkNums();
        }

        private void TrkNumsVerified()
        {
            string trkNumsVerQry = "Select Count([INTERIM_NI_SHIP_NUM1_STAT]) +Count([INTERIM_NI_SHIP_NUM2_STAT]) +Count([INTERIM_BI_SHIP_NUM1_STAT]) +Count([INTERIM_BI_SHIP_NUM2_STAT]) " +
                                    "from INTERIM_HISTORY WHERE ([INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] " +
                                    "is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL) AND INTERIM_SOURCE IS NOT NULL;";

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(trkNumsVerQry, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    int cols = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            TrkNumsVer.Text = reader.GetInt32(0).ToString();
                        }
                    }
                    reader.Close();
                    con.Close();
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

        private void TotalTrkNums()
        {
            string trkNumsVerQry = "Select Count([INTERIM_NI_TRACK_NUM2]) +Count([INTERIM_NI_TRACK_NUM2]) " +
                "+ Count([INTERIM_BI_TRACK_NUM1]) +Count([INTERIM_BI_TRACK_NUM2]) from INTERIM_TEST_CASES " +
                "WHERE ([INTERIM_NI_TRACK_NUM1] Like '1%') OR([INTERIM_NI_TRACK_NUM2] Like '1%') " +
                "OR([INTERIM_BI_TRACK_NUM1] Like '1%') OR([INTERIM_BI_TRACK_NUM2] Like '1%')";

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(trkNumsVerQry, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    int cols = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            TrkNumsTotal.Text = "/" + reader.GetInt32(0).ToString() + " Tracking Nums Verified";
                        }
                    }
                    reader.Close();
                    con.Close();
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

        private void ScenariosLeft()
        {
            string scenariosLeftQry = "Select(Select Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_NI_TRACK_NUM2]) + Count([INTERIM_BI_TRACK_NUM1]) + Count([INTERIM_BI_TRACK_NUM2]) " +
                                       "from INTERIM_TEST_CASES WHERE ([INTERIM_NI_TRACK_NUM1] Like '1%') OR([INTERIM_NI_TRACK_NUM2] Like '1%')OR([INTERIM_BI_TRACK_NUM1] Like '1%') " +
                                       "OR([INTERIM_BI_TRACK_NUM2] Like '1%')) - (Select Count([INTERIM_NI_SHIP_NUM1_STAT])+Count([INTERIM_NI_SHIP_NUM2_STAT]) + " +
                                        "Count([INTERIM_BI_SHIP_NUM1_STAT]) +Count([INTERIM_BI_SHIP_NUM2_STAT]) from INTERIM_HISTORY WHERE [INTERIM_NI_SHIP_NUM1_STAT] is NOT NULL " +
                                        "OR[INTERIM_NI_SHIP_NUM2_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM1_STAT] is NOT NULL OR[INTERIM_BI_SHIP_NUM2_STAT] is NOT NULL);";

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(scenariosLeftQry, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    int cols = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            TotalScenLeft.Text = "Tracking Nums Left: " + reader.GetInt32(0).ToString();
                        }
                    }
                    reader.Close();
                    con.Close();
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

        //The following event handlers take you to the respective forms
        private void WeekendVerification_Click(object sender, RoutedEventArgs e)
        {
            InterimWeekendVerification wkd_ver = new InterimWeekendVerification();
            if (wkd_ver.IDCount() > 0)
            {
                wkd_ver.Show();
            }
        }

        private void DailyVerification_Click(object sender, RoutedEventArgs e)
        {
            InterimDailyVerification daily = new InterimDailyVerification();
            daily.Show();
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            InterimAssignments assignments = new InterimAssignments();
            assignments.Show();
        }

        private void HelpNeeded_Click(object sender, RoutedEventArgs e)
        {
            InterimHelpNeeded help_needed = new InterimHelpNeeded();
            if (help_needed.IDCount() > 0)
            {
                help_needed.Show();
            }
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            InterimReports reports = new InterimReports();
            reports.Show();
        }

        private void AssignSources_Click(object sender, RoutedEventArgs e)
        {
            InterimAssignSource assignSrc = new InterimAssignSource();
            assignSrc.Show();
        }

        private void IndivdualReport_Click(object sender, RoutedEventArgs e)
        {
            InterimIndividualReport idvRep = new InterimIndividualReport();
            idvRep.Show();
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuScreen mainMenu = new MenuScreen(arr);
            this.Close();
            mainMenu.Show();
        }

        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
        }
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemHome":
                    MenuScreen menu = new MenuScreen(arr);
                    menu.Show();
                    this.Close();
                    break;
                case "AddRec":
                    NewRecord nR = new NewRecord(arr);
                    nR.Show();
                    this.Close();
                    break;
                case "GitHub":
                    System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
                    break;
                default:
                    break;
            }
        }
        private void EdiBtn_Click_1(object sender, RoutedEventArgs e)
        {
            EDI_User_Menu_Window ediM = new EDI_User_Menu_Window(arr);
            ediM.Show();
            this.Close();
        }

        private void IssuesBtn_Click_1(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
            this.Close();
        }

        private void InterimBtn_Click_1(object sender, RoutedEventArgs e)
        {
            InterimMainMenu intM = new InterimMainMenu(arr);
            intM.Show();
            this.Close();
        }
        private void ErrFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm2 = new ErrorFileMenu(arr);
            erm2.Show();
            this.Close();
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/wiki");
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }
        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }
    }
}