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
        /*Name: Michael Figueroa
        Function Name: InterimReports
        Purpose: InterimReports Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimReports()
        {
            InitializeComponent();
        }

        /*Name: Michael Figueroa
        Function Name: SourceDetailReport_Click
        Purpose: SourceDetail button click event handler
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void SourceDetailReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSourceDetailReport srcReport = new InterimSourceDetailReport();
            srcReport.Show();
        }

        /*Name: Michael Figueroa
        Function Name: SaturdayScenariosReport_Click
        Purpose: SaturdayScenarios button click event handler
        Parameters: Auto-Generated
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void SaturdayScenariosReport_Click(object sender, RoutedEventArgs e)
        {
            InterimSaturdayScenariosReport satReport = new InterimSaturdayScenariosReport();
            satReport.Show();
        }
    }
}
