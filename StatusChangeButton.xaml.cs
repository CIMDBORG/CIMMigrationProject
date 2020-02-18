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
    /*Name: Mike Figueroa 
       Function Name: StatusChangeButton
       Purpose:  Initializes window, declares local variables
       Parameters: N/A
       Return Value: N/A
       Local Variables: approvedClicked, submittedClicked
       Algorithm:   1. Opens Window
                    2. Sets local variables = false
                    3. Initializes component
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: Comments by Dom Carrubba
       */

    public partial class StatusChangeButton : Window
    {
        public bool approvedClicked = false;
        public bool submittedClicked = false;
        public StatusChangeButton()
        {
            InitializeComponent();
        }

        /*Name: Mike Figueroa 
       Function Name: BCApproved_Click
       Purpose: Event handler for the BC Approval Button 
       Parameters: object sender, RoutedEventArgs e
       Return Value: N/A
       Local Variables: approvedClicked 
       Algorithm:   1. Looks for Button click
                    2. Sets bool approvedClicked = true
                    3. Closes function
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: Comments by Dom Carrubba
       */
        private void BCApproved_Click(object sender, RoutedEventArgs e)
        {
            approvedClicked = true;
            this.Close();
        }


        /*Name: Mike Figueroa 
       Function Name: BCSubmitted_Click
       Purpose: Event handler for the BC Submit Button 
       Parameters: object sender, RoutedEventArgs e
       Return Value: N/A
       Local Variables: submittedClicked 
       Algorithm:   1. Looks for Button click
                    2. Sets bool submittedClicked = true
                    3. Closes function
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: Comments by Dom Carrubba
       */
        private void BCSubmitted_Click(object sender, RoutedEventArgs e)
        {
            submittedClicked = true;
            this.Close();
        }
    }
}
