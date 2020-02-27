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
using System.Windows.Shapes;
using WpfApp1;
using Interim;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for ErrorFileMenu.xaml
    /// </summary>
    public partial class ErrorFileMenu : Window
    {
        private string[] arr;

        /*Name: Brandon Cox
        Function Name: UserMenu_Window
        Purpose: Constructor for the MenuScreen form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public ErrorFileMenu(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
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
        Function Name: ImportRecs_Click
        Purpose: Event handler for ImportRecs Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ImportRecs_Click(object sender, RoutedEventArgs e)
        {
            ErrorFile_Import erI = new ErrorFile_Import(arr);
            erI.Show();

        }

        /*Name: Brandon Cox
        Function Name: UpdateRecs_Click
        Purpose: Event handler for UpdateRecs Button click - opens ErrorFile_Updates
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void UpdateRecs_Click(object sender, RoutedEventArgs e)
        {
            ErrorFile_Updates erMe = new ErrorFile_Updates(arr);
            erMe.Show();
        }

        /*Name: Brandon Cox
        Function Name: LogRev_Click
        Purpose: Event handler for LogRev Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void LogRev_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu errM = new ErrorFileMenu(arr);
            errM.Show();
        }

        /*Name: Brandon Cox
        Function Name: Archives_Click
        Purpose: Event handler for Archives Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Archives_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu errMe = new ErrorFileMenu(arr);
            errMe.Show();
        }

        /*Name: Brandon Cox
        Function Name: DbBtn_Click
        Purpose: Event handler for DbBtn Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        /*Name: Brandon Cox
        Function Name: ErrFileBtn_Click
        Purpose: Event handler for ErrFileBtn Button click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ErrFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erM = new ErrorFileMenu(arr);
            erM.Show();
            this.Close();
        }
    }
}
