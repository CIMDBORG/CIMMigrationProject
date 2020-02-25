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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Jefi.xaml
    /// </summary>
    public partial class Jefi : Page
    {
        /*Name: Michael Figueroa
        Function Name: Jefi
        Purpose: Constructor for Jefi form - the Jefi form is the Easter Egg which contains Jeff Wygant's face and contact info as a running gag - the Billing Center GOAT
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 
        */
        public Jefi()
        {
            InitializeComponent();
        }

        /*Name: Michael Figueroa
        Function Name: Back_Click
        Purpose: Event Handler for back button click
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 
        */
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
