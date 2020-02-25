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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ReportItemsWindow.xaml
    /// </summary>
    public partial class ReportItemsWindow : Window
    {
        /*Name: Michael Figueroa
        Function Name: ReportItemsWindow
        Purpose: ReportItemsWindow Constructor; ReportItemsWindow is the "outline" used for all the reports (BusinessCases,
        ReportsWindow, etc.)
        Parameters: Page page
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public ReportItemsWindow(Page page)
        {
            InitializeComponent();

            ItemsFrame.NavigationService.Navigate(page);
        }

        /*Name: Michael Figueroa
        Function Name: Button_Click
        Purpose: Event handler for Button_Click; takes you to Jefi form
        Parameters: Page page
        Return Value: N/A
        Local Variables: None
        Algorithm: None
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