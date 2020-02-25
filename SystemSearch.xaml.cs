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

        /*Name: Michael Figueroa
        Function Name: SystemSearch
        Purpose: Constructor for SystemSearch
        Parameters: string[] user_data, bool inc_pri_ovr_300
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public SystemSearch(string[] user_data, bool inc_pri_ovr_300)
        {
            InitializeComponent();

            arr = user_data;
            inc_pri_num_ovr_300 = inc_pri_ovr_300;
        }

        /*Name: Michael Figueroa
        Function Name: Submit_Click
        Purpose: Event handler for submit button click
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: string sysString, string query, List<int> id_List, WeeklyReviewApps editRecord
        Algorithm: Assigns values to local variables, then in try block, WeeklyReviewApps form is opened up
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: SysFilter_KeyDown
        Purpose: Event handler for when Enter key is pressed
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: string sysString, string query, List<int> id_List, WeeklyReviewApps editRecord
        Algorithm: if Enter is pressed Assigns values to local variables, then in try block, WeeklyReviewApps form is opened up
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
