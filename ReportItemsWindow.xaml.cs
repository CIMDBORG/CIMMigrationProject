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
        public ReportItemsWindow(Page page)
        {
            InitializeComponent();

            ItemsFrame.NavigationService.Navigate(page);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Jefi jefi = new Jefi();
            ItemsFrame.NavigationService.Navigate(jefi);
        }
    }
}