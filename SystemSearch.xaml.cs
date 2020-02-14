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
    /// Interaction logic for SystemSearch.xaml
    /// </summary>
    public partial class SystemSearch : Window
    {
        private static string[] arr;
        private bool inc_pri_num_ovr_300;

        public SystemSearch(string[] user_data, bool inc_pri_ovr_300)
        {
            InitializeComponent();

            arr = user_data;
            inc_pri_num_ovr_300 = inc_pri_ovr_300;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string sysString = SysFilter.Text.ToString();
            string query = WeeklyReviewApps.GetWeeklyAppsQuery(sysString, inc_pri_num_ovr_300);
            List<int> id_List = Helper.FillIDList(query);
            try
            {
                WeeklyReviewApps editRecord = new WeeklyReviewApps(arr, sysString, Helper.FillIDList(query), inc_pri_num_ovr_300);
                editRecord.Show();
                this.Close();
            }

            catch
            {
                if (id_List.Count == 0)
                {
                    MessageBox.Show("There Are No Open Items for This System");
                }
            }
        }

        //allows user to hit enter after inputting into texbox and moving to next page if viable
        private void SysFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string sysString = SysFilter.Text.ToString();
                string query = WeeklyReviewApps.GetWeeklyAppsQuery(sysString, inc_pri_num_ovr_300);
                List<int> id_List = Helper.FillIDList(query);
                try
                {
                    WeeklyReviewApps editRecord = new WeeklyReviewApps(arr, sysString, Helper.FillIDList(query), inc_pri_num_ovr_300);
                    editRecord.Show();
                    this.Close();
                }

                catch
                {
                    if (id_List.Count == 0)
                    {
                        MessageBox.Show("There Are No Open Items for This System");
                    }
                }

            }
        }
    }
}
