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
using WpfApp1;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for EDI_Search_Products.xaml
    /// </summary>
    public partial class EDI_Search_Products : Window
    {
        public EDI_Search_Products()
        {
            InitializeComponent();
        }

        private void AddProd_Click(object sender, RoutedEventArgs e)
        {
            EDI_Add_Product addProd = new EDI_Add_Product();
            addProd.Show();
        }
    }
}
