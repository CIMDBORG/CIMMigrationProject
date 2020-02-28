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
    /// Interaction logic for EDI_Search_Company.xaml
    /// </summary>
    public partial class EDI_Search_Company : Window
    {
        /*Name: Michael Figueroa
        Function Name: EDI_Search_Company
        Purpose: Constructor for the EDI_Search_Company form
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public EDI_Search_Company()
        {
            InitializeComponent();
            FillCompanyComboBox();
            FillIDCompanyComboBox();
            SearchCompanyComboBox.SelectedIndex = 0;
            SearchIDComboBox.SelectedIndex = 0;
            BindDataGrid();
        }

        /*Name: Michael Figueroa
        Function Name: FillCompanyComboBox
        Purpose: Fills CompanyComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void FillCompanyComboBox()
        {
            SearchCompanyComboBox.Items.Add("Item1");
        }

        /*Name: Michael Figueroa
        Function Name: FillIDCompanyComboBox
        Purpose: Fills IDCompanyComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private void FillIDCompanyComboBox()
        {
            SearchIDComboBox.Items.Add("Item2");
        }

        /*Name: Michael Figueroa
        Function Name: SearchEDIIDQuery
        Purpose: query that will search by EDI ID
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private string SearchEDIIDQuery()
        {
            return "SELECT COMPANY_NAME FROM EDI_CUSTOMER WHERE EDI_ID = '" + SearchIDComboBox.SelectedItem.ToString() + "';" ;
        }

        /*Name: Michael Figueroa
        Function Name: SearchCompanyQuery
        Purpose: query that will search by Company Name
        Parameters: None
        Return Value: string
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        private string SearchCompanyQuery()
        {
            return "SELECT COMPANY_NAME FROM EDI_CUSTOMER WHERE COMPANY_NAME = '" + SearchCompanyComboBox.SelectedItem.ToString() + "';";
        }

        /*Name: Michael Figueroa
       Function Name: BindDataGrid
       Purpose: BindsDataGrid with appropriate results from query
       Parameters: None
       Return Value: None
       Local Variables: string query, DataTable searchTable
       Algorithm: if EDIIDSearch is checked, then query is set to SearchEDIIDQuery; else, query is set to SearchCompanyQuery. Then datatable is instantiated,
       and Helper.BindDataGrid is used to bind searchTable to Report
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private void BindDataGrid()
        {
            string query;

            if (EDIIDSearch.IsChecked == true)
            {
                query = SearchEDIIDQuery();
            }

            else
            {
                query = SearchCompanyQuery();
            }

            DataTable searchTable = new DataTable();
            Helper.BindDataGrid(searchTable, query);
            Report.ItemsSource = searchTable.DefaultView;
        }

        /*Name: Michael Figueroa
       Function Name: RadioButton_Checked
       Purpose: RadioButton_Checked event handler
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: None
       Date modified:  1/2020
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SearchCompanyComboBox.Visibility = Visibility.Collapsed;
            SearchIDComboBox.Visibility = Visibility.Visible;
        }

        /*Name: Michael Figueroa
      Function Name: RadioButton_Checked_1
      Purpose: RadioButton_Checked1 event handler
      Parameters: Auto-Generated
      Return Value: None
      Local Variables: None
      Algorithm: None
      Date modified:  1/2020
      Assistance Received: N/A
      Version: 2.0.0.4
      */
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            SearchIDComboBox.Visibility = Visibility.Collapsed;
            SearchCompanyComboBox.Visibility = Visibility.Visible;
        }
    }
}