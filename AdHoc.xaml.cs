using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using WpfApp1;

namespace WpfApp2
{
    public partial class AdHoc : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString; ////ConnectionString comes from App.config
        private DataRowView reportRow; //stores data from row clicked
        private string customQuery;  //variable to store query
        private string[] arr; //variable to store login-based user data

        /*Name: Michael Figueroa
        Function Name: AdHoc
        Purpose: AdHoc Constructor
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public AdHoc(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }

        /*Name: Michael Figueroa
        Function Name: AdHoc
        Purpose: Returns ad hoc query based on user choices
        Parameters: string[] user_data
        Return Value: N/A
        Local Variables: None
        Algorithm: first, we determine where the query starts then, we append the appropriate conditions to the query based on what criteria are checked off (we add sys_impact to the where clause 
        if systems are checked, same with Category and etc.
        Get list of all categories that are checked, and then add to the query based on that
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string ConstructCustomQuery()
        {
            string query = "SELECT ID, Priority_Number, Sys_Impact, [Status], Title, Supporting_Details, Internal_Notes, Mgr_Notes, Assigned_To FROM New_Issues WHERE " + DetermineQueryStart();
            string[] assigned = ParseString();

            StringBuilder sb = new StringBuilder(query);
            if (Assigned_ToCheckBox.IsChecked == true)
            {
                for (int i = 0; i < assigned.Length; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(" OR Assigned_To = ");
                    }
                    sb.Append("'" + assigned[i] + "'");
                }
                sb.Append(") ");
            }

            if (StatusCheckBox.IsChecked == true)
            {
                sb.Append(" " + StatusQuery());
            }
           
            if (CategoryCheckBox.IsChecked == true)
            {
                List<string> categoryFilters = CategoryQuery();

                for (int i = 0; i < categoryFilters.Count; i++)
                {
                    if ((DetermineQueryStart() != "(Category = ") && (i == 0))
                    {
                        sb.Append("AND (Category = ");
                    }
                    else if (i != 0)
                    {
                        sb.Append(" OR Category = ");
                    }
                    sb.Append("'" + categoryFilters[i] + "'");
                }
                sb.Append(") ");
            }

            if (SystemCheckBox.IsChecked == true)
            {
                List<string> sysFilters = SystemFilter();

                for (int i = 0; i < sysFilters.Count; i++)
                {
                    if ((DetermineQueryStart() != "(Sys_Impact = ") && (i == 0))
                    {
                        sb.Append("AND (Sys_Impact = ");
                    }
                    else if (i != 0)
                    {
                        sb.Append(" OR Sys_Impact = ");
                    }
                    sb.Append("'" + sysFilters[i] + "'");
                }
                sb.Append(") ");
            }

            if (ManagerMeetingCheckBox.IsChecked == true)
            {
                if ((DetermineQueryStart() != "(ManagerMeeting = 1"))
                {
                    sb.Append("AND (ManagerMeeting = 1");
                }
                sb.Append(") ");
            }

            if (UpdateNeeded.IsChecked == true)
            {
                if ((DetermineQueryStart() != "(Manager_Update_Bit = 1 AND User_Update_Bit = 0)"))
                {
                    sb.Append("AND (Manager_Update_Bit = 1 AND User_Update_Bit = 0)");
                }
            }

            if (ManagerReviewCheckBox.IsChecked == true)
            {
                if ((DetermineQueryStart() != "(ManagerReview = 0"))
                {
                    sb.Append("AND (ManagerReview = 0");
                }
                sb.Append(") ");
            }

            sb.Append(" ORDER BY Priority_Number DESC");
            customQuery = sb.ToString();

            return sb.ToString();
        }

        /*Author : Michael Figueroa
        Function Name: FillReportTable
        Purpose: Table is filled using the data from the customQuery
        Parameters: DataTable table
        Return Value: None
        Local Variables: None 
        Algorithm: Table is filled using the data from the customQuery, then Report DataGrid filled with info from query generated
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void FillReportTable(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(ConstructCustomQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                  
                    using (sda)
                    {
                        sda.Fill(table);
                        Report.ItemsSource = table.DefaultView;
                        Report.Visibility = Visibility.Visible;
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }
        }
        /*Author : Michael Figueroa
        Function Name: DetermineQueryStart
        Purpose: Determines start of the query
        Parameters: None
        Return Value: string
        Local Variables: None 
        Algorithm: if assigned_to is checked, then Assigned_to is the start of the query; else if status is checked, empty string is returned; else if category is checked, then category portion is returned;
        else etc. etc.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private string DetermineQueryStart()
        {
            if (Assigned_ToCheckBox.IsChecked == true)
            {
                return "(Assigned_To = ";
            }
            else if (StatusCheckBox.IsChecked == true)
            {
                return "";
            }
            else if (CategoryCheckBox.IsChecked == true)
            {
                return "(Category = ";
            }
            else if (SystemCheckBox.IsChecked == true)
            {
                return "(Sys_Impact = ";
            }

            else if (ManagerMeetingCheckBox.IsChecked == true)
            {
                return "(ManagerMeeting = 1)";
            }

            else if (UpdateNeeded.IsChecked == true)
            {
                return "(Manager_Update_Bit = 1 AND User_Update_Bit = 0)";
            }

            else if (ManagerReviewCheckBox.IsChecked == true)
            {
                return "(ManagerReview = 0)";
            }

            else
            {
                return null;
            }
        }

        /*Author : Michael Figueroa
        Function Name: SetVisiblity
        Purpose: Sets Visibility of the checkboxes that are under assigned_to, status, category, and system
        Parameters: None
        Return Value: string
        Local Variables: None 
        Algorithm: if assigned_to is checked, then Assigned_to is the start of the query; else if status is checked, empty string is returned; else if category is checked, then category portion is returned;
        else etc. etc.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void SetVisibility(CheckBox checkBox, StackPanel stack)
        {
            if (checkBox.IsChecked == true)
            {
                stack.Visibility = Visibility.Visible;
            }
            else
            {
                stack.Visibility = Visibility.Collapsed;

            }
        }

        /*Author : Michael Figueroa
        Function Name: SetVisiblity
        Purpose: Sets Visibility of a TextBox 
        Parameters: None
        Return Value: string
        Local Variables: None 
        Algorithm: if the checkbox for the appropriate TextBox text is checked, then text is visible; else, it is not
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void SetVisibility(CheckBox checkBox, TextBox text)
        {
            if (checkBox.IsChecked == true)
            {
                text.Visibility = Visibility.Visible;
            }
            else
            {
                text.Visibility = Visibility.Collapsed;
            }
        }

        /*Author : Michael Figueroa
        Function Name: ParseString
        Purpose: parses the Assigned_To_Text string using char[] delimiter 
        Parameters: None
        Return Value: string[]
        Local Variables: string assignedToString, char [] delimter, string[] assignedTo
        Algorithm: assignedToString is split using delimiter
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private string[] ParseString()
        {
            string assignedToString = Assigned_To_Text.Text.ToString();
            char[] delimiter = new char[] { '/', ';', ',', ' ' };
            string[] assignedTo = assignedToString.Split(delimiter);
            return assignedTo;
        }

        /*Author : Michael Figueroa
        Function Name: StatusFilters
        Purpose: returns a list of StatusFilters; I don't feel this is necessary, this may be better off as a variable 
        Parameters: None
        Return Value: List<string> statusesChosen
        Local Variables: List<string> statusesChosen
        Algorithm: assignedToString is split using delimiter
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */

        private List<string> StatusFilters()
        {
            List<string> statusesChosen = new List<string>();
            statusesChosen.Add("Implemented");
            statusesChosen.Add("Dropped");
            statusesChosen.Add("Closed");
            statusesChosen.Add("Deferred");
            return statusesChosen;
        }

        /*Author : Michael Figueroa
        Function Name: SystemFilter
        Purpose: returns a list of systemFilters
        Parameters: None
        Return Value: List<string> statusesChosen
        Local Variables: None
        Algorithm: checkBox content added to systemFilters if var item is a CheckBox that is checked - there are two columns on the form with System combobox options,
        so there are two foreach loops for each seperate column
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private List<string> SystemFilter()
        {
            List<string> systemFilters = new List<string>();
            var children = LogicalTreeHelper.GetChildren(SystemsStack);

            foreach (var item in children)
            {
                var checkBox = item as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    systemFilters.Add(checkBox.Content.ToString());
                }
            }

            var childrenTwo = LogicalTreeHelper.GetChildren(SystemStackTwo);

            foreach (var item in childrenTwo)
            {
                var checkBox = item as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    systemFilters.Add(checkBox.Content.ToString());
                }
            }
            return systemFilters;
        }

        /*Author : Michael Figueroa
        Function Name: CategoryQuery
        Purpose: returns a list of category filters for the query
        Parameters: None
        Return Value: List<string> categoryFilters
        Local Variables: List<string> categoryFilters
        Algorithm: foreach checkbox item in children (children in this case is StackPanel named CategoryStack), if the checkbox is checked, the category string is added
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private List<string> CategoryQuery()
        {
            List<string> categoryFilters = new List<string>();

            var children = LogicalTreeHelper.GetChildren(CategoryStack);

            foreach (var item in children)
            {
                var checkBox = item as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    categoryFilters.Add(checkBox.Content.ToString());
                }
            }
            return categoryFilters;
        }

        /*Author : Michael Figueroa
        Function Name: StatusQuery
        Purpose: returns string used for Status condition in where clause
        Parameters: None
        Return Value: string
        Local Variables: List<string> statuses
        Algorithm: if both OpenedCheckBox and Assigned_ToCheckBox are checked, then the items that are not closed are queried; else, the items that are closed
        are queried. Else if both closed and assigned to checkboxes are checked, the closed issues are queried; else if opened is checked but assigned_to are closed,
        then the closed ones are not queried, else if closed is checked but assigned to is not checked, then all closed issues are queried; else, null returned.
        This if-else clause can definitely be simplified.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private string StatusQuery()
        {
            List<string> statuses;
            statuses = StatusFilters();
            if (OpenedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == true)
            {
                return "AND (New_Issues.[Status] != '" + statuses[0] + "' AND New_Issues.[Status] != '" + statuses[1] + "' AND New_Issues.[Status] != '" + statuses[2] + "' AND New_Issues.[Status] != '" + statuses[3] + "') ";
            }

            else if (ClosedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == true)
            {
                return "AND (New_Issues.[Status] = '" + statuses[0] + "' OR New_Issues.[Status] = '" + statuses[1] + "' OR New_Issues.[Status] = '" + statuses[2] + "' OR New_Issues.[Status] = '" + statuses[3] + "') ";
            }

            else if (OpenedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == false)
            {
                return " (New_Issues.[Status] != '" + statuses[0] + "' AND New_Issues.[Status] != '" + statuses[1] + "' AND New_Issues.[Status] != '" + statuses[2] + "' AND New_Issues.[Status] != '" + statuses[3] + "') ";
            }
            else if (ClosedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == false)
            {
                return " (New_Issues.[Status] = '" + statuses[0] + "' OR New_Issues.[Status] = '" + statuses[1] + "' OR New_Issues.[Status] = '" + statuses[2] + "' OR New_Issues.[Status] = '" + statuses[3] + "') ";
            }
            else
            {
                return null;
            }
        }

        /*Author : Michael Figueroa
        Function Name: SetCheckboxes
        Purpose: returns string used for Status condition in where clause
        Parameters: None
        Return Value: string
        Local Variables: List<string> statuses
        Algorithm: if both OpenedCheckBox and Assigned_ToCheckBox are checked, then the items that are not closed are queried; else, the items that are closed
        are queried. Else if both closed and assigned to checkboxes are checked, the closed issues are queried; else if opened is checked but assigned_to are closed,
        then the closed ones are not queried, else if closed is checked but assigned to is not checked, then all closed issues are queried; else, null returned.
        This if-else clause can definitely be simplified.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void SetCheckboxes()
        {
            if (SystemFilter().Count == 0)
            {
                SystemCheckBox.IsChecked = false;
            }
            if (CategoryQuery().Count == 0)
            {
                CategoryCheckBox.IsChecked = false;
            }
            if (String.IsNullOrWhiteSpace(Assigned_To_Text.ToString()))
            {
                Assigned_ToCheckBox.IsChecked = false;
            }
            if (StatusQuery() == null)
            {
                StatusCheckBox.IsChecked = false;
            }
        }

        /*Author : Michael Figueroa
        Function Name: GenerateReport_Click
        Purpose: Event handler for the generate report click button
        Parameters: None
        Return Value: None
        Local Variables: DataTable report
        Algorithm: Calls SetCheckboxes, FillReportTable, then sets Back.Visibility to visible and collapses the generate report button
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.Visibility = Visibility.Collapsed;
            DataTable report = new DataTable();
            SetCheckboxes();
            FillReportTable(report);
            Back.Visibility = Visibility.Visible;
            GenerateReport.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: EditButton_Click
        Purpose: Event handler for edit button click
        Parameters: Auto-generated
        Return Value: None
        Local Variables: DataRowView agingItemsRow
        Algorithm: The DataRow in which the Edit button was clicked is retrieved, and the EditRecord form is opened using that DataRowView in the constructor
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                reportRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDArray = Helper.FillIDList(customQuery);
                EditRecord editRecord = new EditRecord(this, arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /*Author : Michael Figueroa
        Function Name: StatusCheckBox_Click
        Purpose: Event handler for statuscheckboxclick
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetVisibility
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void StatusCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(StatusCheckBox, StatusCheckBoxes);
        }

        /*Author : Michael Figueroa
        Function Name: Assigned_ToCheckBox_Click
        Purpose: Event handler for Assigned_ToCheckBox
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetVisibility
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void Assigned_ToCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(Assigned_ToCheckBox, Assigned_To_Text);
        }

        /*Author : Michael Figueroa
        Function Name: CategoryCheckBox_Click
        Purpose: Event handler for CategoryCheckBox
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetVisibility
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void CategoryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(CategoryCheckBox, CategoryStack);
        }

        /*Author : Michael Figueroa
        Function Name: SystemCheckBox_Click
        Purpose: Event handler for SystemCheckBox
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls SetVisibility twice (once for the first stackpanel of systems, once for the other)
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void SystemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(SystemCheckBox, SystemsStack);
            SetVisibility(SystemCheckBox, SystemStackTwo);
        }

        /*Author : Michael Figueroa
        Function Name: Back_Click
        Purpose: Event handler for Back button click
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Sets the form back to default values
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.Visibility = Visibility.Visible;
            Report.Visibility = Visibility.Collapsed;
            Back.Visibility = Visibility.Collapsed;
            GenerateReport.Visibility = Visibility.Visible;
        }

        /*Author : Michael Figueroa
        Function Name: ManagerMeetingCheckBox_Click
        Purpose: Event handler for ManagerMeetingCheckBox click event
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: var childrenTwo - contains the list of elements within StatusCheckBoxes stackpanel
        Algorithm: Sets stauscheckbox to checked, StatusCheckBoxes becomes visible, then foreach item in childrenTwo, is checkBox.name
        is OpenedCheckBox, then openedcheckbox is checked. - Not sure why I set it this way, pls look into this.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/2020
        Assistance Received: N/A
        */
        private void ManagerMeetingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            StatusCheckBox.IsChecked = true;
            StatusCheckBoxes.Visibility = Visibility.Visible;

            var childrenTwo = LogicalTreeHelper.GetChildren(StatusCheckBoxes);

            foreach (var item in childrenTwo)
            {
                var checkBox = item as CheckBox;
                if (checkBox.Name == "OpenedCheckBox")
                {
                    checkBox.IsChecked = true;
                }
            }
        }
    }
}