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
       public partial class StatusChangeButton : Window
    {
        public bool approvedClicked = false; // set to false because the Approved button hasn't been clicked yet
        public bool submittedClicked = false; //set to false because Submitted button hasn't been pushed yet
        
        /*Name: Mike Figueroa 
       Function Name: StatusChangeButton
       Purpose:  Initializes window
       Parameters: N/A
       Return Value: N/A
       Local Variables: None
       Algorithm:   1. Opens Window
                    2. Sets local variables = false
                    3. Initializes component
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: Comments by Dom Carrubba
       */
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
                    3. Closes form
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
                    3. Closes form
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
