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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for StatusChangeButton.xaml
    /// this is a form that contains two buttons, these buttons are for the managers to use in ManagerTasks.xaml in order to update statuses to BC Submitted or BC Approved
    /// </summary>
    /// 
    public partial class StatusChangeButton : Window
    {
        public bool approvedClicked = false;
        public bool submittedClicked = false;
        public StatusChangeButton()
        {
            InitializeComponent();
        }
         
        //closes form on click evens
        private void BCApproved_Click(object sender, RoutedEventArgs e)
        {
            approvedClicked = true;
            this.Close();
        }

        private void BCSubmitted_Click(object sender, RoutedEventArgs e)
        {
            submittedClicked = true;
            this.Close();
        }
    }
}
