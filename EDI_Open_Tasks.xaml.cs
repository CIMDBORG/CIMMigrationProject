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
        private string[] arr;
        private DataTable imp = new DataTable();
        private DataTable main_req = new DataTable();

        public EDI_Open_Tasks(string[] user_data)
        {
            InitializeComponent();
            EDI_Helper.Fill_Table(imp, Imp_Qry());
            EDI_Helper.Fill_Table(main_req, Mnt_Req_Qry());
            Fill_EDI_Combo_Box();
            arr = user_data;
            TaskComboBox.SelectedIndex = 0;
        }

        private void Fill_EDI_Combo_Box()
        {
            TaskComboBox.Items.Add("Implementations");
            TaskComboBox.Items.Add("Maintenance Requests");
        }

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

        //Implementation query
        private string Imp_Qry()
        {
            return "SELECT COMPANY_NAME, PRODUCT, IMPLEMENTER, STS_TYPE FROM EDI_PRODUCT INNER JOIN EDI_CST_HST ON EDI_PRODUCT.EDI_ID = EDI_CST_HST.EDI_ID WHERE STS_TYPE != 'Production';";
        }

        //Maintenance Request Query
        private string Mnt_Req_Qry()
        {
            return "SELECT REQUEST_NUMBER, EDI_MAIN_REQ.TECHNICAL_CONTACT AS TECHNICAL_CONTACT, REQ_DATE, STATUS, NOTES FROM EDI_MAIN_REQ INNER JOIN EDI_PRODUCT ON EDI_PRODUCT.EDI_ID = EDI_MAIN_REQ.EDI_ID;";
        }

        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Display_Grid();
        }
    }
}