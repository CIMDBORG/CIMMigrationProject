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
using WpfApp1;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for EDI_Main_Menu.xaml
    /// </summary>
    public partial class EDI_Main_Menu : Page
    {
        private string[] arr;

        public EDI_Main_Menu(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Open_Tasks_Click(object sender, RoutedEventArgs e)
        {
            EDI_Open_Tasks tasks_open = new EDI_Open_Tasks(arr);
            tasks_open.Show();
        }

        private void Add_Company_Click(object sender, RoutedEventArgs e)
        {
            EDI_Add_Company addCompany = new EDI_Add_Company();
            addCompany.Show();

        }

        private void Search_Company_Click(object sender, RoutedEventArgs e)
        {
            EDI_Search_Company searchCompany = new EDI_Search_Company();
            searchCompany.Show();
        }
    }
}
