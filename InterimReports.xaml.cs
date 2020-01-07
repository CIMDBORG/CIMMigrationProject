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

namespace Interim
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class InterimReports : Window
    {
        public InterimReports()
        {
            InitializeComponent();
        }

        private void SourceDetailReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSourceDetailReport srcReport = new InterimSourceDetailReport();
            srcReport.Show();
        }

        private void SaturdayScenariosReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSaturdayScenariosReport satReport = new InterimSaturdayScenariosReport();
            satReport.Show();
        }
    }
}
