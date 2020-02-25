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
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Interim;
using WpfApp2;

namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	Window that holds the Main Menu and other sub-menu's of the application. 
    //                  On initialization, the frame displays UserMenuPage, the Page containing main app menu.
    //                  Frame also can display sub-menus via Navigation between Pages. See UserMenuPage.xaml.cs for more.
    //*******************************************************************
    public partial class UserMenu_Window : Window 
    {
        
        private string[] arr;
        private DataRowView reportRow;

        /*Name: Michael Figueroa
        Function Name: UserMenu_Window
        Purpose: Constructor for the UserMenu_Window form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: SetAgingWarning and SetUpdateWarning are called
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public UserMenu_Window(string[] user_data)
        {
            InitializeComponent();         
            arr = user_data;
            SetAgingWarning();
            SetUpdateWarning();
        }

        public System.Windows.Navigation.NavigationService NavigationService { get; }

        //if the user has 1 or more items that are about to age, they will be notified and the aging button will be visible
        private void SetAgingWarning()
        {
            DataTable agingItems = ReportHelper.FillAgingOwnerSpecific(arr[2]);
            if(agingItems.Rows.Count > 0)
            {
                AgingButton.Visibility = Visibility.Visible;
            }
        }

        //opens edit record form so the user can edit their items that are about to age
        private void AgingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                DataTable closeToAging = ReportHelper.FillAgingOwnerSpecific(arr[2]);
                DataView viewAging = closeToAging.DefaultView;
                reportRow = viewAging[0];
                List<int> IDArray = Helper.FillIDList(ReportHelper.OwnerAgingQuery(arr[2]));

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //if the user has 1 or more items that are marked as needing an update
        private void SetUpdateWarning()
        {
            DataTable updatesRequired = ReportHelper.FillUpdateRequired(arr[2]);
            if (updatesRequired.Rows.Count > 0)
            {
                UpdateReq.Visibility = Visibility.Visible;
            }
            else
            {
                UpdateReq.Visibility = Visibility.Collapsed;
            }
        }
       
        private void UpdateReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                DataTable updatesRequired = ReportHelper.FillUpdateRequired(arr[2]);
                DataView viewAging = updatesRequired.DefaultView;
                reportRow = viewAging[0];
                List<int> IDArray = Helper.FillIDList(ReportHelper.OwnerUpdatesReq(arr[2]));

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        //*******************************************************************
        // DESCRIPTION: Opens a new New Issue form on button click, by creating and showing an instance of Window NewRecord.
        //              Passes login-based data to NewRecord form for pre-population of fields.
        //*******************************************************************
        private void NewRecordbutton_Click(object sender, RoutedEventArgs e)
        {
            NewRecord newRecord = new NewRecord(arr);
            newRecord.WindowState = WindowState.Maximized;
            newRecord.Show();
        }


        private void StrategicTasks_Click(object sender, RoutedEventArgs e)
        {
            StrategicTasks strategicTasks = new StrategicTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(strategicTasks);
            itemsWindow.Show();
        }

        //*******************************************************************
        // DESCRIPTION: Runs when the page is loaded.
        //              This function checks the user's role. If the user is not a Manager, 
        //                  certain features are collapsed and unavailable.
        //*******************************************************************
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (arr[6] == "User")
            {
                ForManagersbutton.Visibility = Visibility.Collapsed;
            }
        }

        // Runs when 'Browse Items' button is clicked. Navigates to UserMenu_ItemsPage Page, passing login-based data in arr
        private void BrowseItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            UserMenu_Item itemsPage = new UserMenu_Item(arr);
            itemsPage.Show();
        }

        //*******************************************************************
        // DESCRIPTION: Runs when 'Generate Report' button is clicked.
        //*******************************************************************
        private void Reportbutton_Click_1(object sender, RoutedEventArgs e)
        {
            ReportsWindow reportsWindow = new ReportsWindow(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(reportsWindow);
            itemsWindow.Show();
        }

        private void ForManagersButton_click(object sender, RoutedEventArgs e)
        {
            Managers forManagers = new Managers(arr);
            forManagers.Show();
        }

        private void WeeklyReviewApps_Click(object sender, RoutedEventArgs e)
        {
            bool include_300s;
            string[] systems = Helper.UsersSystems(arr[7]);

            MessageBoxResult include_low_pri = MessageBox.Show("Include Items with Priority Over 300?", "Priority Over 300", MessageBoxButton.YesNo);

            if (include_low_pri == MessageBoxResult.No)
            {
                include_300s = false;
            }

            else
            {
                include_300s = true;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show("Filter By System?", "Filter By System", MessageBoxButton.YesNoCancel);

            if (messageBoxResult == MessageBoxResult.No)
            {
                string query = WeeklyReviewApps.GetWeeklyAppsQuery(systems, include_300s);
                WeeklyReviewApps editRecord = new WeeklyReviewApps(arr, include_300s, Helper.FillIDList(query));
                editRecord.Show();
            }
            //if cancel, do nothing
            else if (messageBoxResult == MessageBoxResult.Cancel)
            {

            }
            else
            {
                SystemSearch sysSearch = new SystemSearch(arr, include_300s);
                sysSearch.Show();
            }
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
                    nR.WindowState = WindowState.Maximized;
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

        private void ErrorFile_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm1 = new ErrorFileMenu(arr);
            erm1.Show();
            this.Close();
        }

        private void ErrFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm2 = new ErrorFileMenu(arr);
            erm2.Show();
            this.Close();
        }
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
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