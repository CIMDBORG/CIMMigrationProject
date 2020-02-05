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
using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	ItemsWindow can display any page that is passed to it in the constructor.
    //                  But for the purposes of this app, it is meant to interact with
    //                      PrioritizeBySystemPage and SearchItemsPage.
    //*******************************************************************
    public partial class Items_Window : Window
    {
       
        public Items_Window(Page page)
        {
            InitializeComponent();

            ItemsFrame.NavigationService.Navigate(page);
        }

        private void Jefi_Click(object sender, RoutedEventArgs e)
        {
            Jefi jefi = new Jefi();
            ItemsFrame.NavigationService.Navigate(jefi);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Jefi jefi = new Jefi();
            ItemsFrame.NavigationService.Navigate(jefi);
        }
    }
}