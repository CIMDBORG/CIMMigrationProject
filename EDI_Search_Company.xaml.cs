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
        public EDI_Search_Company()
        {
            InitializeComponent();
            FillCompanyComboBox();
            FillIDCompanyComboBox();
            SearchCompanyComboBox.SelectedIndex = 0;
            SearchIDComboBox.SelectedIndex = 0;
            BindDataGrid();
        }

        private void FillCompanyComboBox()
        {
            SearchCompanyComboBox.Items.Add("Item1");
        }
        private void FillIDCompanyComboBox()
        {
            SearchIDComboBox.Items.Add("Item2");
        }
        
        //Search by EDI ID
        private string SearchEDIIDQuery()
        {
            return "SELECT COMPANY_NAME FROM EDI_COMPANY WHERE EDI_ID = '" + SearchIDComboBox.SelectedItem.ToString() + "';" ;
        }

        //Search by EDI Company Name
        private string SearchCompanyQuery()
        {
            return "SELECT COMPANY_NAME FROM EDI_COMPANY WHERE COMPANY_NAME = '" + SearchCompanyComboBox.SelectedItem.ToString() + "';";
        }

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

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SearchCompanyComboBox.Visibility = Visibility.Collapsed;
            SearchIDComboBox.Visibility = Visibility.Visible;
        }
       
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            SearchIDComboBox.Visibility = Visibility.Collapsed;
            SearchCompanyComboBox.Visibility = Visibility.Visible;
        }
    }
}