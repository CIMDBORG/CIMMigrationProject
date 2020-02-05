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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	This Page contains the menu that appears when 'Browse Items' is clicked in Main Menu.
    //                  Login-based data from New_Contacts about the user is passed, enabling options based on role.
    //                  'Prioritization by System' button and 'Search Items' buttons both open the Window ItemsWindow, 
    //                      but each of those buttons has the app display a different page upon opening ItemsWindow.
    //                  Also features the ability to navigate back to the Main Menu (UserMenuPage) on button click.
    //*******************************************************************
    public partial class UserMenu_ItemsPage : Window
    {
        
        private string[] arr;   



        
        public UserMenu_ItemsPage(string[] user_data)
        {
            InitializeComponent();
            
            arr = user_data;
        }



        // Navigates back to UserMenuPage (main menu), preserving login-based user data
        private void MainMenubutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            UserMenu_Window userWindow = new UserMenu_Window(arr);
            userWindow.Show();
        }



        //*******************************************************************
        // DESCRIPTION: Runs when Page is loaded. This function checks the user's role. 
        //              If the user is not a Manager, certain features are collapsed and unavailable.
        //*******************************************************************
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (arr[6] == "User")
            {
                BrowseOpenItemsbutton.Visibility = Visibility.Collapsed;
            }
        }




        //*******************************************************************
        // DESCRIPTION: Runs when 'Prioritize By System' button is clicked.
        //              Creates an instance of PrioritizeBySystemPage, passing login-based user data. 
        //              Creates an instance of ItemsWindow, passing the PrioritizeBySystemPage as parameter.
        //              The result is a new ItemsWindow with PrioritizeBySystemPage displayed.
        //*******************************************************************
        private void PrioritizeBySysbutton_Click(object sender, RoutedEventArgs e)
        {
            Items_PrioritizeBySystemPage prioritizeBySystem = new Items_PrioritizeBySystemPage(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(prioritizeBySystem);
            itemsWindow.Show();
        }

        //*******************************************************************
        // DESCRIPTION: Runs when 'Search Items' button is clicked.
        //              Creates an instance of SearchItemsPage, passing login-based user data. 
        //              Creates an instance of ItemsWindow, passing the SearchItemsPage as parameter.
        //              The result is a new ItemsWindow with SearchItemsPage displayed.
        //*******************************************************************
        private void SearchItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            Items_SearchItemsPage searchItemsPage = new Items_SearchItemsPage(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(searchItemsPage);
            itemsWindow.Show();
        }



        private void OpenItemsButton_Click(object sender, RoutedEventArgs e)
        {
            //Items_OpenItemsPage; 
        }

        private void BrowseOpenItemsbutton_Click(object sender, RoutedEventArgs e)
        {
            BrowseOpenItems browseOpen = new BrowseOpenItems(arr);
            ReportItemsWindow itemsWindow = new ReportItemsWindow(browseOpen);
            itemsWindow.Show();
        }
    }
}
