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

        /*Name: Michael Figueroa
        Function Name: SetAgingWarning
        Purpose: if the user has 1 or more items that are about to age, they will be notified and the aging button will be visible
        Parameters: None
        Return Value: None
        Local Variables: DataTable agingItems
        Algorithm: calls ReportHelper.FillAgingOwnerSpecific to fill agingItems; if agingItems.Rows.Count > 0, then the aging button becomes visible; else, nothing happens.
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SetAgingWarning()
        {
            DataTable agingItems = ReportHelper.FillAgingOwnerSpecific(arr[2]);
            if(agingItems.Rows.Count > 0)
            {
                AgingButton.Visibility = Visibility.Visible;
            }
        }

        /*Name: Michael Figueroa
        Function Name: AgingButton_Click
        Purpose: Event handler for aging button click - opens edit record form so the user can edit their items that are about to age
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable closeToAging, DataView viewAging, List<int> IDArray
        Algorithm: Calls ReportHelper.FillAgingOwnerSpecific to fill closeToAging table, then assigns reportRow = DataView viewAging[0]; FillIDList is called in order to fill the IDArray,
        and then these are used to call EditRecord and open up an EditRecord form in which the user can update all of their aging items in one shot
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SetUpdateWarning
        Purpose: Sets the visibility of the button that gives the user a warning that one of their items has been requested to be updated by a manager
        Parameters: None
        Return Value: None
        Local Variables: DataTable updatesRequired
        Algorithm: calls FillUpdateRequired in order to fill updatesRequired; if updatesRequired.Rows.Count > 0 (i.e if updatesRequired is not empty), then the button is visible;
        if it is no, then it is not visible.
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: UpdateReq_Click
        Purpose: Event Handler for UpdateReq button click; purpose is to direct user to edit record form in order to edit the items that have been marked by a manager as needing an
        update
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable updatesRequired, DataView viewUpdateReq, List<int> IDArray
        Algorithm: Calls FillUpdateRequired to fill updatesRequired, sets DataView viewUpdateReq = updatesRequired.DefaultView, then calls FillIDList to retrieve IDs of items
        marked for update by manager, then edit record form is opened
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void UpdateReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                DataTable updatesRequired = ReportHelper.FillUpdateRequired(arr[2]);
                DataView viewUpdateReq = updatesRequired.DefaultView;
                reportRow = viewUpdateReq[0];
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
        /*Name: Michael Figueroa
        Function Name: NewRecordbutton_Click
        Purpose: Event Handler for NewRecord button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: sets newRecord.WindowState to maximized before opening
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void NewRecordbutton_Click(object sender, RoutedEventArgs e)
        {
            NewRecord newRecord = new NewRecord(arr);
            newRecord.WindowState = WindowState.Maximized;
            newRecord.Show();
        }

        /*Name: Michael Figueroa
        Function Name: StrategicTasks_Click
        Purpose: Event Handler for StrategicTasks button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void StrategicTasks_Click(object sender, RoutedEventArgs e)
        {
            StrategicTasks strategicTasks = new StrategicTasks(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(strategicTasks);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
          Function Name: Page_Loaded
          Purpose: Event Handler for when the page loads
          Parameters: Auto-Generated
          Return Value: None
          Local Variables: None
          Algorithm: if the user's role is equal to user, the ForManagers button is collapsed
          Date modified: Prior to 1/1/20
          Assistance Received: N/A
          */
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (arr[6] == "User")
            {
                ForManagersbutton.Visibility = Visibility.Collapsed;
            }
        }

        /*Name: Michael Figueroa
         Function Name: BrowseItemsbutton_Click
         Purpose: Runs when 'Browse Items' button is clicked. Navigates to UserMenu_ItemsPage Page, passing login-based data in arr
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: None
         Algorithm: None
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        private void BrowseItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            UserMenu_Item itemsPage = new UserMenu_Item(arr);
            itemsPage.Show();
        }

        /*Name: Michael Figueroa
         Function Name: Reportbutton_Click_1
         Purpose: Runs when 'Reports' button is clicked. Navigates to UserMenu_ItemsPage Page, passing login-based data in arr
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: None
         Algorithm: None
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        private void Reportbutton_Click_1(object sender, RoutedEventArgs e)
        {
            ReportsWindow reportsWindow = new ReportsWindow(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(reportsWindow);
            itemsWindow.Show();
        }

        /*Name: Michael Figueroa
         Function Name: ForManagersButton_click
         Purpose: Runs when 'For Managers' button is clicked. Navigates to Managers Page, passing login-based data in arr
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: None
         Algorithm: None
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        private void ForManagersButton_click(object sender, RoutedEventArgs e)
        {
            Managers forManagers = new Managers(arr);
            forManagers.Show();
        }

        /*Name: Michael Figueroa
         Function Name: WeeklyReviewApps_Click
         Purpose: Runs when 'Weekly Review W/Apps' button is clicked.
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: bool include_300s, string[] systems, MessageBoxResult include_low_pri, MessageBoxResult messageBoxResult, string query
         Algorithm: Calls UsersSystems to fill systems array, then prompts user if they want to include priority numbers over 300; if user clicks no, include_300s is set to false,
         else, it is set to true
         Another messagebox appears asking users whether they wanna filter by system; if no, weeklyReviewApps is opened using array systems in parameter, else if they chose cancel,
         the messagebox closes without anything happening, else, SystemSearch form opens.
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
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

        /*Name: Michael Figueroa
         Function Name: ButtonOpenMenu_Click
         Purpose: Runs when ButtonOpen button is clicked. This expands the hamburger menu
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

        /*Name: Brandon Cox
        Function Name: EdiBtn_Click_1
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

        /*Name: Brandon Cox
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

        /*Name: Brandon Cox
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
        private void ErrorFile_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm1 = new ErrorFileMenu(arr);
            erm1.Show();
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
   Function Name: ReportBtn_Click
   Purpose: Event handler for report Button click
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

        /*Name: Michael Figueroa
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