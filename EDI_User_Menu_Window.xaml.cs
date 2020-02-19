using Interim;
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
    /// Interaction logic for User_Menu_Window_EDI.xaml
    /// </summary>
    public partial class EDI_User_Menu_Window : Window
    {
        private string[] arr;
        private DataRowView reportRow;

        public EDI_User_Menu_Window(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;

            /*ADIDtext.Text = arr[0];
            Nametext.Text = arr[1] + " " + arr[2];
            Roletext.Text = arr[6];
            */
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuScreen mainMenu = new MenuScreen(arr);
            this.Close();
            mainMenu.Show();
        }

        private void Exitbutton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit Application?", "Exit Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Stop);

            if (messageBoxResult == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
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
                    this.Close();
                    break;
                case "AddRec":
                    NewRecord nR = new NewRecord(arr);
                    nR.WindowState = WindowState.Maximized;
                    nR.Show();
                    this.Close();
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

        private void ErrorFile_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm1 = new ErrorFileMenu(arr);
            erm1.Show();
            this.Close();
        }

        private void ErrFileBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorFileMenu erm2 = new ErrorFileMenu(arr);
            erm2.Show();
            this.Close();
        }
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/issues");
        }
        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CIMDBORG/CIMMigrationProject/wiki");
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }
        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }
    }
}