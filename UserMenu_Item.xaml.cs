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

namespace WpfApp2
{
    //*******************************************************************
    // DESCRIPTION: 	This Page contains the menu that appears when 'Browse Items' is clicked in Main Menu.
    //                  Login-based data from New_Contacts about the user is passed, enabling options based on role.
    //                  'Prioritization by System' button and 'Search Items' buttons both open the Window ItemsWindow, 
    //                      but each of those buttons has the app display a different page upon opening ItemsWindow.
    //                  Also features the ability to navigate back to the Main Menu (UserMenuPage) on button click.
    //*******************************************************************


    public partial class UserMenu_Item : Window
    {
        private string[] arr;

        /*Name: Michael Figueroa
        Function Name: UserMenu_Item
        Purpose: Constructor for UserMenu_Item
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public UserMenu_Item(string[] user_data)
        {
            InitializeComponent();
            
            arr = user_data;

        }

        /*Name: Michael Figueroa
        Function Name: MainMenubutton_Click
        Purpose: MainMenubutton_Click is an event handler that opens up a UserMenu_Window (Main Menu) form
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void MainMenubutton_Click(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
            this.Close();
        }




        /*Name: Michael Figueroa
        Function Name: Page_Loaded
        Purpose: Page_Loaded is an event handler triggered when the page loads
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: if arr[6] equals user, then user is cannot browse open items.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (arr[6] == "User")
            {
                BrowseOpenItemsbutton.Visibility = Visibility.Collapsed;
            }
        }




        /*Name: Michael Figueroa
        Function Name: PrioritizeBySysbutton_Click
        Purpose: PrioritizeBySysbutton_Click is an event handler triggered when clicking the Prioritize by system button;
        opens Prioritize by system form
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void PrioritizeBySysbutton_Click(object sender, RoutedEventArgs e)
        {
            Items_PrioritizeBySystemPage prioritizeBySystem = new Items_PrioritizeBySystemPage(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(prioritizeBySystem);
            itemsWindow.Show();
            this.Close();
        }

        /*Name: Michael Figueroa
        Function Name: SearchItemsbutton_Click
        Purpose: SearchItemsbutton_Click is an event handler triggered when clicking the Search items button; Opens
        Search form
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SearchItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            Items_SearchItemsPage searchItemsPage = new Items_SearchItemsPage(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(searchItemsPage);
            itemsWindow.Show();
            this.Close();
        }

        /*Name: Michael Figueroa
        Function Name: BrowseOpenItemsbutton_Click
        Purpose: BrowseOpenItemsbutton_Click is an event handler triggered when clicking the Browse Open items button; Opens
        BrowseOpenItems form
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void BrowseOpenItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            BrowseOpenItems browseOpen = new BrowseOpenItems(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(browseOpen);
            itemsWindow.Show();
            this.Close();
        }
    }
}
