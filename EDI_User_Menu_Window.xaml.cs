using Interim;
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

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for User_Menu_Window_EDI.xaml
    /// </summary>
    public partial class EDI_User_Menu_Window : Window
    {
        private string[] arr;
        private DataRowView reportRow;

        /*Name: Michael Figueroa
        Function Name: EDI_User_Menu_Window
        Purpose: Constructor for the EDI_User_Menu_Window form
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public EDI_User_Menu_Window(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;

            /*ADIDtext.Text = arr[0];
            Nametext.Text = arr[1] + " " + arr[2];
            Roletext.Text = arr[6];
            */
        }

        /*Name: Michael Figueroa
        Function Name: MainMenu_Click
        Purpose: Event handler for MainMenu_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuScreen mainMenu = new MenuScreen(arr);
            this.Close();
            mainMenu.Show();
        }

        /*Name: Michael Figueroa
        Function Name: Exitbutton_Click
        Purpose: Event handler for Exitbutton_Click
        Parameters: Auto-Generated
        Return Value: MessageBoxResult messageBoxResult
        Local Variables: None
        Algorithm: if user clicks ok on message box, then application shutsdown
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void Exitbutton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit Application?", "Exit Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Stop);

            if (messageBoxResult == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }

        /*Name: Michael Figueroa
        Function Name: Open_Tasks_Click
        Purpose: Event handler for Open_Tasks_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void Open_Tasks_Click(object sender, RoutedEventArgs e)
        {
            EDI_Open_Tasks tasks_open = new EDI_Open_Tasks(arr);
            tasks_open.Show();
        }

        /*Name: Michael Figueroa
        Function Name: Add_Company_Click
        Purpose: Event handler for Add_Company_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void Add_Company_Click(object sender, RoutedEventArgs e)
        {
            EDI_Add_Company addCompany = new EDI_Add_Company();
            addCompany.Show();

        }

        /*Name: Michael Figueroa
        Function Name: Search_Company_Click
        Purpose: Event handler for Search_Company_Click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void Search_Company_Click(object sender, RoutedEventArgs e)
        {
            EDI_Search_Company searchCompany = new EDI_Search_Company();
            searchCompany.Show();
        }

        /*Name: Brandon Cox
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

        /*Name: Brandon Cox
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

        /*Name: Michael Figueroa
       Function Name: EDIBtn_Click_1
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