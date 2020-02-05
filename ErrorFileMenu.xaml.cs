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
using Interim;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for ErrorFileMenu.xaml
    /// </summary>
    public partial class ErrorFileMenu : Window
    {
        private string[] arr;

        public ErrorFileMenu(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemHome":
                    MenuScreen menu = new MenuScreen(arr);
                    menu.Show();
                    break;
                case "AddRec":
                    
                    break;
                case "GitHub":
                    System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
                    break;
                default:
                    break;
            }
        }
        private void EdiBtn_Click_1(object sender, RoutedEventArgs e)
        {
            EDI_User_Menu_Window ediM = new EDI_User_Menu_Window(arr);
            ediM.Show();
            this.Close();
        }

        private void IssuesBtn_Click_1(object sender, RoutedEventArgs e)
        {
            UserMenu_Window userM = new UserMenu_Window(arr);
            userM.Show();
            this.Close();
        }

        private void InterimBtn_Click_1(object sender, RoutedEventArgs e)
        {
            InterimMainMenu intM = new InterimMainMenu(arr);
            intM.Show();
            this.Close();
        }

        private void ImportRecs_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erM = new ErrorFileMenu(arr);
            erM.Show();

        }

        private void UpdateRecs_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erMe = new ErrorFileMenu(arr);
            erMe.Show();
        }

        private void LogRev_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu errM = new ErrorFileMenu(arr);
            errM.Show();
        }

        private void Archives_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu errMe = new ErrorFileMenu(arr);
            errMe.Show();
        }
    }
}
