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
    /// Interaction logic for EDI_Open_Tasks.xaml
    /// </summary>
    public partial class EDI_Open_Tasks : Window
    {
        private string[] arr;//array with user info
        private DataTable imp = new DataTable(); //array with implementations for a specific user
        private DataTable main_req = new DataTable(); //maintenance requests datatable

        /*Name: Michael Figueroa
        Function Name: EDI_Open_Tasks
        Purpose: Constructor for the EDI_Open_Tasks form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public EDI_Open_Tasks(string[] user_data)
        {
            InitializeComponent();
            EDI_Helper.Fill_Table(imp, Imp_Qry());
            EDI_Helper.Fill_Table(main_req, Mnt_Req_Qry());
            Fill_EDI_Combo_Box();
            arr = user_data;
            TaskComboBox.SelectedIndex = 0;
        }

        /*Name: Michael Figueroa
        Function Name: Fill_EDI_Combo_Box
        Purpose: Fills the EDIComboBox - Mike needs to re-name this for clarity sake
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void Fill_EDI_Combo_Box()
        {
            TaskComboBox.Items.Add("Implementations");
            TaskComboBox.Items.Add("Maintenance Requests");
        }

        /*Name: Michael Figueroa
       Function Name: Display_Grid
       Purpose: Controls what DataGrid is currently displayed based on the option chosen from combobox
       Parameters: None
       Return Value: None
       Local Variables: None
       Algorithm: If "Implementations" are chosen, Imp is visible; else, Maint_Req is visible
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private void Display_Grid()
        {
            Imp.ItemsSource = imp.DefaultView;
            Maint_Req.ItemsSource = main_req.DefaultView;
            if(EDI_Helper.Get_Chkbox_Str(TaskComboBox) == "Implementations")
            {
                Maint_Req.Visibility = Visibility.Collapsed;
                Imp.Visibility = Visibility.Visible;
            }

            else
            {
                Imp.Visibility = Visibility.Collapsed;
                Maint_Req.Visibility = Visibility.Visible;
            }
        }

        /*Name: Michael Figueroa
       Function Name: Imp_Qry
       Purpose: Returns the query that displays all implementations that are not in prod
       Parameters: None
       Return Value: string
       Local Variables: None
       Algorithm: None
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private string Imp_Qry()
        {
            return "SELECT COMPANY_NAME, PRODUCT, IMPLEMENTOR, STS_TYPE FROM EDI_CUSTOMER INNER JOIN EDI_CST_HST ON EDI_CUSTOMER.EDI_ID = EDI_CST_HST.EDI_ID WHERE STS_TYPE != 'Production';";
        }

        /*Name: Michael Figueroa
       Function Name: Mnt_Req_Qry
       Purpose: Returns the query that displays all maintenance requests
       Parameters: None
       Return Value: string
       Local Variables: None
       Algorithm: None
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private string Mnt_Req_Qry()
        {
            return "SELECT REQUEST_NUMBER, EDI_MAIN_REQ.TECHNICAL_CONTACT AS TECHNICAL_CONTACT, REQ_DATE, STATUS, NOTES FROM EDI_MAIN_REQ INNER JOIN EDI_CUSTOMER ON EDI_CUSTOMER.EDI_ID = EDI_MAIN_REQ.EDI_ID;";
        }

        /*Name: Michael Figueroa
       Function Name: TaskComboBox_SelectionChanged
       Purpose: Event handler for TaskComboBox selection changed
       Parameters: None
       Return Value: string
       Local Variables: None
       Algorithm: Calls Display_Grid
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Display_Grid();
        }
    }
}
