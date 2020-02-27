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

        /*Name: Michael Figueroa
        Function Name: InterimMainMenu
        Purpose: InterimMainMenu Constructor
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: if the user's role is User, then the Assign visibility is collapsed; then TrkNumsVerified, ScenariosLeft, and TotalTrkNums are
        called
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: TrkNumsVerified
        Purpose: Query that tells us how many tracking numbers have been verified
        Parameters: None
        Return Value: N/A
        Local Variables: string trkNumsVerQry
        Algorithm: Using standard Sql procedure, the result of trkNumsVerQry is read, then TrkNumsVer.Text is set to the ToString() value of that result
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: TotalTrkNums
        Purpose: Query that tells us how many tracking numbers have been verified
        Parameters: None
        Return Value: N/A
        Local Variables: string trkNumsQry
        Algorithm: Using standard Sql procedure, the result of trkNumsQry is read, then TrkNumsTotal.Text is set to the ToString() value of that result
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void TotalTrkNums()
        {
            string trkNumsQry= "Select Count([INTERIM_NI_TRACK_NUM2]) +Count([INTERIM_NI_TRACK_NUM2]) " +
                "+ Count([INTERIM_BI_TRACK_NUM1]) +Count([INTERIM_BI_TRACK_NUM2]) from INTERIM_TEST_CASES " +
                "WHERE ([INTERIM_NI_TRACK_NUM1] Like '1%') OR([INTERIM_NI_TRACK_NUM2] Like '1%') " +
                "OR([INTERIM_BI_TRACK_NUM1] Like '1%') OR([INTERIM_BI_TRACK_NUM2] Like '1%')";

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(trkNumsQry, con);
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

        /*Name: Michael Figueroa
        Function Name: ScenariosLeft
        Purpose: Query that tells us how many tracking numbers have been verified
        Parameters: None
        Return Value: N/A
        Local Variables: string scenariosLeftQry
        Algorithm: Using standard Sql procedure, the result of scenariosLeftQry is read, then TotalScenLeft.Text is set to the ToString() value of that result
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: WeekendVerification_Click
        Purpose: Event handler for  WeekendVerification button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: if wkd_ver.IDCount > 0 (in other words, if there are test cases loaded for weekend verification), then the wkd_ver form is shown
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void WeekendVerification_Click(object sender, RoutedEventArgs e)
        {
            InterimWeekendVerification wkd_ver = new InterimWeekendVerification();
            if (wkd_ver.IDCount() > 0)
            {
                wkd_ver.Show();
            }
        }

        /*Name: Michael Figueroa
        Function Name: DailyVerification_Click
        Purpose: Event handler for  DailyVerification button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void DailyVerification_Click(object sender, RoutedEventArgs e)
        {
            InterimDailyVerification daily = new InterimDailyVerification();
            daily.Show();
        }

        /*Name: Michael Figueroa
        Function Name: Assign_Click
        Purpose: Event handler for  Assign button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            InterimAssignments assignments = new InterimAssignments();
            assignments.Show();
        }

        /*Name: Michael Figueroa
       Function Name: HelpNeeded_Click
       Purpose: Event handler for HelpNeeded button click
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: if wkd_ver.IDCount > 0 (in other words, if there are test cases loaded for weekend verification), then the wkd_ver form is shown
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void HelpNeeded_Click(object sender, RoutedEventArgs e)
        {
            InterimHelpNeeded help_needed = new InterimHelpNeeded();
            if (help_needed.IDCount() > 0)
            {
                help_needed.Show();
            }
        }

        /*Name: Michael Figueroa
        Function Name: Report_Click
        Purpose: Event handler for  Report button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Report_Click(object sender, RoutedEventArgs e)
        {
            InterimReports reports = new InterimReports();
            reports.Show();
        }

        /*Name: Michael Figueroa
        Function Name: AssignSources_Click
        Purpose: Event handler for AssignSources button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void AssignSources_Click(object sender, RoutedEventArgs e)
        {
            InterimAssignSource assignSrc = new InterimAssignSource();
            assignSrc.Show();
        }

        /*Name: Michael Figueroa
        Function Name: IndivdualReport_Click
        Purpose: Event handler for IndivdualReport button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void IndivdualReport_Click(object sender, RoutedEventArgs e)
        {
            InterimIndividualReport idvRep = new InterimIndividualReport();
            idvRep.Show();
        }

        /*Name: Michael Figueroa
        Function Name: MainMenu_Click
        Purpose: Event handler for MainMenu button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuScreen mainMenu = new MenuScreen(arr);
            this.Close();
            mainMenu.Show();
        }

        /*Name: Michael Figueroa
        Function Name: ReportBtn_Click
        Purpose: Event handler for ReportBtn button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
        }

        /*Name: Michael Figueroa
       Function Name: ButtonOpenMenu_Click
       Purpose: Event handler for  ButtonOpenMenu button click
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: None
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
         Function Name: ButtonCloseMenu_Click
         Purpose: Runs when ButtonClose button is clicked. This collapses the hamburger menu
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: None
         Algorithm: None
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        /*Name: Brandon Cox
       Function Name: ListViewMenu_SelectionChanged
       Purpose: Event handler for ListViewMenu selection changed 
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: Case ItemHome: MenuScreen is opened and this form is closed
       Case AddRec: NewRecord opened, this form closed
       Case GitHub: internet browser opens to github CIM Project repository
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
        Function Name: EDI_Click
        Purpose: Event handler for EDI Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void EdiBtn_Click_1(object sender, RoutedEventArgs e)
        {
            EDI_User_Menu_Window ediM = new EDI_User_Menu_Window(arr);
            ediM.Show();
            this.Close();
        }

        /*Name: Michael Figueroa
        Function Name: IssuesBtn_Click_1
        Purpose: Event handler for Issues Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void IssuesBtn_Click_1(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
            this.Close();
        }

        /*Name: Michael Figueroa
        Function Name: InterimBtn_Click_1
        Purpose: Event handler for Interim Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void InterimBtn_Click_1(object sender, RoutedEventArgs e)
        {
            InterimMainMenu intM = new InterimMainMenu(arr);
            intM.Show();
            this.Close();
        }

        /*Name: Brandon Cox
    Function Name: ErrorFile_Click
    Purpose: Event handler for error file Button click
    Parameters: Auto-Generated
    Return Value: None
    Local Variables: None
    Algorithm: None
    Date modified: Prior to 1/1/20
    Assistance Received: N/A
    */
        private void ErrFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm2 = new ErrorFileMenu(arr);
            erm2.Show();
            this.Close();
        }

        /*Name: Brandon Cox
   Function Name: HelpBtn_Click
   Purpose: Event handler for help Button click
   Parameters: Auto-Generated
   Return Value: None
   Local Variables: None
   Algorithm: None
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/wiki");
        }

        /*Name: Michael Figueroa
   Function Name: LogoutBtn_Click
   Purpose: Event handler for logout Button click
   Parameters: Auto-Generated
   Return Value: None
   Local Variables: None
   Algorithm: Calls Application.Current.Shutdown to close the application
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }

        /*Name: Brandon Cox
   Function Name: DbBtn_Click
   Purpose: Event handler for logout Button click
   Parameters: Auto-Generated
   Return Value: None
   Local Variables: None
   Algorithm: Calls Application.Current.Shutdown to close the application
   Date modified: Prior to 1/1/20
   Assistance Received: N/A
   */
        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }
    }
}