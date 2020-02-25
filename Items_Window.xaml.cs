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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace WpfApp1
{      
    public partial class Items_Window : Window
    {
        /*Name: Michael Figueroa
         Function Name: Items_Window
         Purpose: Constructor for the Items_Window form
         Parameters: Page page
         Return Value: None
         Local Variables: None
         Algorithm: Calls NavigationService.Navigate (navigates to the page defined in the constructor)
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        public Items_Window(Page page)
        {
            InitializeComponent();

            ItemsFrame.NavigationService.Navigate(page);
        }

        /*Name: Michael Figueroa
         Function Name: Button_Click
         Purpose: Event handler for UPS Logo click
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: None
         Algorithm: Calls NavigationService.Navigate to navigate to the easter egg
         Version: 2.0.0.4
         Date modified: Prior to 1/1/20
         Assistance Received: N/A
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Jefi jefi = new Jefi();
            ItemsFrame.NavigationService.Navigate(jefi);
        }
    }
}