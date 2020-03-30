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
using System.Diagnostics;
using WpfApp2;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	NewRecord is the form for logging a new issue into the Issues DB.
    //                  The form contains various textboxes, comboboxes, checkboxes and datepickers which collect
    //                      all the information needed for a new issue. There is also the option to add a new status note.
    //*******************************************************************
    public partial class NewRecord : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString; //SQL Connection string; see App.config
        private string[] arr;               //local variable to store login-based user data
        private int IDnum;                      //local variable to store issue ID number

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);



        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint MF_ENABLED = 0x00000000;

        const uint SC_CLOSE = 0xF060;

        const int WM_SHOWWINDOW = 0x00000018;
        const int WM_CLOSE = 0x10;

        /*Name: Michael Figueroa
        Function Name: NewRecord
        Purpose: Constructor for NewRecord.xaml
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: user-specific info is passed onto string[] arr, then the opened date field is set to the current date
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public NewRecord(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;
            Startdatepicker.SelectedDate = DateTime.Today;
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }


        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHOWWINDOW)
            {
                IntPtr hMenu = GetSystemMenu(hwnd, false);
                if (hMenu != IntPtr.Zero)
                {
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }
            }
            return IntPtr.Zero;
        }


        /*Name: Michael Figueroa
        Function Name: SubmitIssueButton_Click
        Purpose: Event handler for submit button
        Parameters: string[] user_data, string historyQuery, string tasknumQuery, string issuesQuery
        Return Value: None
        Local Variables: None
        Algorithm: First, the ID of the issue just submitted is pulled. Then, in try block, issuesQuery is assigned using InsertData_NewContacts, then executed. The reader retrieves the ID of the issue, assigning it to global 
        variable IDnum, and uses IDnum to execute InsertData_History method, which assigns value to historyQuery; NewRecord closes, UserMenu_Window form shows
        Version: 3.3.0.0
        Date modified: 3/25/2020
        Assistance Received: Brandon Cox
        */
        private void SubmitIssueButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection con = new SqlConnection(connectionString);

            string tasknumQuery = GetIssueIDQuery();

            try
            {
                string issuesQuery = InsertData_NewContacts();
                con.Open();

                SqlCommand issuesCmd = new SqlCommand(issuesQuery, con);
                issuesCmd.ExecuteNonQuery();

                SqlDataReader reader2;
                SqlCommand IDCmd = new SqlCommand(tasknumQuery, con);
                reader2 = IDCmd.ExecuteReader();

                while (reader2.Read())
                {
                    IDnum = reader2.GetInt32(0);
                }

                reader2.Close();

                string historyQuery = InsertData_History(IDnum);

                SqlCommand historyCmd = new SqlCommand(historyQuery, con);
                historyCmd.ExecuteNonQuery();

                MessageBox.Show("Insert Successful!");

                this.Close();
            }

            catch (SqlException ex)
            {
                MessageBox.Show("Fields Marked with an * Must Be Filled");
                MessageBox.Show(ex.ToString());
            }

            catch (NullReferenceException)
            {
                MessageBox.Show("Fields Marked With An * Must Be Filled, Please Try Again");
            }
            catch (InvalidOperationException)
            {

            }
            finally
            {
                con.Close();
            }

        }

        /*Name: Michael Figueroa
        Function Name: InsertData_NewContacts
        Purpose: This returns the string of the query that inserts the record into the database
        Parameters: None
        Return Value: None
        Local Variables: strings: title, assigned_to, req_dept, req_name, opened_date, due_date, status, category, BCTINumber, sys_impact, priority_num, supporting_details, internal_notes, control_enhancement, process_improvement,
        impact, cim_val, bus_impact, deliverables, benefits, annualCost, highPriority, hours, BCSubText, teamMembers, query
        doubles: onetime_benefit, annual_benefit
        Algorithm: First, we assign every variable to its corresponding field on the form (so string title is assigned to TitleText.Text.ToString(), for instance)
        Then, conditions are used to determine query; if not a strategic task, strategic task-specific fields will not have fields filled; if a due_date hasn't been chosen, then the due_date is set; else if not, then the due_date
        Is left out of the query. Else if it is a strat task, then strat task specific fields are also filled; same if-else clause is used to see if a due_date should be set or not
        NOTE: CAN BE IMPROVED IN FUTURE - would like nested if-else clause based on due_date to be removed
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string InsertData_NewContacts()
        {
            string title = TitleText.Text.ToString();
            string assigned_to = arr[2].ToString();
            string req_dept = RequestingDeptComboBox.SelectedItem.ToString();
            string req_name = RequestedbyText.Text.ToString();
            string opened_date = Startdatepicker.SelectedDate.ToString();
            string due_date;
            if (Planneddatepicker.Text.Length > 0)
            {
                due_date = Planneddatepicker.SelectedDate.ToString();
            }
            else
            {
                due_date = null;
            }

            string status = StatusComboBox.SelectedValue.ToString();
            string category = CategoryComboBox.SelectedValue.ToString();
            string BCTINumber;

            try
            {
                if (BCTItext1.Text.Length == 0)
                {
                    BCTINumber = "NULL";
                }
                else
                {
                    BCTINumber = Int32.Parse(BCTItext1.Text).ToString();
                }
            }

            catch
            {
                MessageBox.Show("BCTI Fields Must Contain Numbers Only");
                return null;
            }

            string sys_impact = SystemComboBox.SelectedValue.ToString();
            string priority_num;
            if (PriorityText.Text.Length == 0)
            {
                priority_num = "0";
            }
            else
            {
                priority_num = Int32.Parse(PriorityText.Text).ToString();
            }

            string supporting_details = SupportingDetailsText.Text.ToString();
            string internal_notes = InternalNotesText.Text.ToString();
            string control_enhancement = ControlEnhancementCheckBox.IsChecked.ToString();
            string process_improvement = ProcessImprovementCheckBox.IsChecked.ToString();
            string impact;
            if (ImpacttypeComboBox.Text.Length == 0)
            {
                impact = "";
            }
            else
            {
                impact = ImpacttypeComboBox.Text.ToString();
            }
            string cim_val = CIMValueAddedCheckBox.IsChecked.ToString();
            string bus_impact = BusinessImpactsText.Text.ToString();
            double onetime_benefit = string.IsNullOrWhiteSpace(OneTimeBenefitText.Text) ? 0 : Double.Parse(OneTimeBenefitText.Text.ToString().Replace(",", ""));
            double annual_benefit = string.IsNullOrWhiteSpace(AnnualBenefitText.Text) ? 0 : Double.Parse(AnnualBenefitText.Text.ToString().Replace(",", ""));

            //values only pertaining to strategic tasks

            string deliverables = Deliverables.Text.ToString(); 
            string benefits = BenefitsText.Text.ToString();
            double annualCost = string.IsNullOrWhiteSpace(AnnualCostSavings.Text) ? 0 : Double.Parse(AnnualCostSavings.Text.ToString().Replace(",", ""));
            string highPriority = HighPriority.IsChecked.ToString();
            string hours = string.IsNullOrWhiteSpace(HoursText.Text) ? "0" : HoursText.Text.ToString();
            string BCSubText = BCSub.IsChecked.ToString();
            string teamMembers = TeamMembers.Text.ToString();


            string query;
            //if strategic task then we add certain values specific to strat task like annual cost savings, high priority (hp), hours, and BcSub
            //if a due date is chosen by a user, then we add it to the database
            if (category != "Strategic Task")
            {
                if (due_date != null)
                {
                    query = "INSERT INTO New_Issues (Title, Assigned_To, Req_Dept, Req_Name, Opened_Date, Due_Date, [Status], " +
                "Category, TFS_BC_HDFS_Num, Sys_Impact, Deliverables, Priority_Number, Supporting_Details, Internal_Notes, " +
                "[Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, OneTimeBenefit, AnnualBenefit, WeeklyReview, ManagerReview) " +
                "VALUES ('" + title.Replace("'", "\''") + "', '" + assigned_to + "', '" + req_dept + "', '" + req_name + "', '" + opened_date + "', '" + due_date + "', '" + status +
                "', '" + category + "', " + BCTINumber + ", '" + sys_impact + "', '" + deliverables + "', " + priority_num + ", '" + supporting_details.Replace("'", "\''") + "', '" + internal_notes.Replace("'", "\''") +
                "', '" + control_enhancement + "', '" + process_improvement + "', '" + impact + "', '" + cim_val +
                "', '" + bus_impact.Replace("'", "\''") + "', " + onetime_benefit + ", '" + annual_benefit + "', 1, 0);";
                }
                else
                {
                    query = "INSERT INTO New_Issues (Title, Assigned_To, Req_Dept, Req_Name, Opened_Date, [Status], " +
                "Category, TFS_BC_HDFS_Num, Sys_Impact, Deliverables, Priority_Number, Supporting_Details, Internal_Notes, " +
                "[Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, OneTimeBenefit, AnnualBenefit, WeeklyReview, ManagerReview) " +
                "VALUES ('" + title.Replace("'", "\''") + "', '" + assigned_to + "', '" + req_dept + "', '" + req_name + "', '" + opened_date + "', '" + status +
                "', '" + category + "', " + BCTINumber + ", '" + sys_impact + "', '" + deliverables.Replace("'", "\''") + "', " + priority_num + ", '" + supporting_details.Replace("'", "\''") + "', '" + internal_notes.Replace("'", "\''") +
                "', '" + control_enhancement + "', '" + process_improvement + "', '" + impact + "', '" + cim_val +
                "', '" + bus_impact.Replace("'", "\''") + "', " + onetime_benefit + ", " + annual_benefit + ", 1, 0);";
                }
            }
            else
            {
                if (due_date != null)
                {
                    query = "INSERT INTO New_Issues (Title, Assigned_To, Req_Dept, Req_Name, Opened_Date, Due_Date, [Status], " +
                "Category, TFS_BC_HDFS_Num, Sys_Impact, Deliverables, Team_Members, Priority_Number, Supporting_Details, Internal_Notes, " +
                "[Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, OneTimeBenefit, AnnualBenefit, WeeklyReview, ManagerReview, Benefits, Annual_Cost_Savings, HP, Hours, BCSub) " +
                "VALUES ('" + title.Replace("'", "\''") + "', '" + assigned_to + "', '" + req_dept + "', '" + req_name + "', '" + opened_date + "', '" + due_date + "', '" + status +
                "', '" + category + "', " + BCTINumber + ", '" + sys_impact + "', '" + deliverables.Replace("'", "\''") + "', '" + teamMembers + "', " + "9999" + ", '" + supporting_details.Replace("'", "\''") + "', '" + internal_notes.Replace("'", "\''") +
                "', '" + control_enhancement + "', '" + process_improvement + "', '" + impact + "', '" + cim_val +
                "', '" + bus_impact.Replace("'", "\''") + "', " + onetime_benefit + ", " + annual_benefit + ", 1, 0, '" + benefits.Replace("'", "\''") + "', " + annualCost + ", '" + highPriority + "', " + hours + ", '" + BCSubText + "'); ";
                }
                else
                {
                    query = "INSERT INTO New_Issues (Title, Assigned_To, Req_Dept, Req_Name, Opened_Date, [Status], " +
                "Category, TFS_BC_HDFS_Num, Sys_Impact, Deliverables, Team_Members, Priority_Number, Supporting_Details, Internal_Notes, " +
                "[Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, OneTimeBenefit, AnnualBenefit, WeeklyReview, ManagerReview, Benefits, Annual_Cost_Savings, HP, Hours, BCSub) " +
                "VALUES ('" + title.Replace("'", "\''") + "', '" + assigned_to + "', '" + req_dept + "', '" + req_name + "', '" + opened_date + "', '" + status +
                "', '" + category + "', " + BCTINumber + ", '" + sys_impact + "', '" + deliverables.Replace("'", "\''") + "', '" + teamMembers + "', " + "9999" + ", '" + supporting_details.Replace("'", "\''") + "', '" + internal_notes.Replace("'", "\''") +
                "', '" + control_enhancement + "', '" + process_improvement + "', '" + impact + "', '" + cim_val +
                "', '" + bus_impact.Replace("'", "\''") + "', " + onetime_benefit + ", " + annual_benefit + ", 1, 0, '" + benefits.Replace("'", "\''") + "', " + annualCost + ", '" + highPriority + "', " + hours + ", '" + BCSubText + "'); ";
                }
            }

            return query;
        }

        /*Name: Michael Figueroa
        Function Name: GetIssueIDQuery
        Purpose: Getter method that retrieves the ID of the issue that was just entered
        Parameters: None
        Return Value: None
        Local Variables: string query2
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string GetIssueIDQuery()
        {
            string query2 = "select top 1 (ID) from New_Issues order by ID desc";
            return query2;
        }

        /*Name: Michael Figueroa
        Function Name: InsertData_History
        Purpose: This inserts the first status for the issue into the History table
        Parameters: int x (x is for TaskNum in History table)
        Return Value: string query3 (query used to insert first status into database)
        Local Variables: string query3
        Algorithm: ID is assigned using parameter int x, status_date is assigned using StatusDatePicker, sys_impact is assigned using System_ComboBox, status_note 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string InsertData_History(int x)
        {
            string ID = x.ToString();
            string status_date = StatusDatePicker.SelectedDate.ToString();
            string sys_impact = SystemComboBox.SelectedItem.ToString();

            string status_note;
            if (String.IsNullOrEmpty(StatusNoteTextBox.Text.ToString()))
            {
                status_note = "Added to database as a new Issue.";
            }
            else
            {
                status_note = StatusNoteTextBox.Text.ToString();
            }

            string comboind;
            if (HistoryStatusComboBox.SelectedIndex < 0)
            {
                comboind = "Item Not Assigned";
            }
            else
            {
                comboind = (HistoryStatusComboBox.SelectedValue).ToString();
            }

            string query3 = "Insert into History (TaskNum, EntryDate, Status, New_StatusNote) Values(" + ID + ", '" + status_date + "', '" + status_note + "', '" + comboind + "');";
            return query3;
        }


        /*Name: Michael Figueroa
        Function Name: CancelButton_Click
        Purpose: event handler for cancel button; exits the form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: ID is assigned using parameter int x, status_date is assigned using StatusDatePicker, sys_impact is assigned using System_ComboBox, status_note 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                MenuScreen mS = new MenuScreen(arr);
                mS.Show();
                this.Close();
            }
            else
            {
                NewRecord nR = new NewRecord(arr);
                nR.Show();
                nR.WindowState = WindowState.Maximized;
                this.Close();
            }
        }

       /*Name: Michael Figueroa
       Function Name: MoreInfoButton_Click
       Purpose: Event Handler for MoreI
       Parameters: Auto-Generated
       Return Value: None
       Local Variables: None
       Algorithm: When button is clicked, the Status stackpanel becomes visible 
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            MoreInfoButton.Visibility = Visibility.Hidden;
            StatusStackPanel.Visibility = Visibility.Visible;
        }

        /*Name: Michael Figueroa
        Function Name: Window_Loaded
        Purpose: Event Handler for Window load event
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: When button is clicked, the Status stackpanel becomes visible 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Helper.FillSystemComboBoxNoAll(SystemComboBox);
            SystemComboBox.Items.Add("Test");
            CategoryComboBox.Items.Add("BC/TI");
            CategoryComboBox.Items.Add("Defect");
            CategoryComboBox.Items.Add("HDFS");
            CategoryComboBox.Items.Add("Inquiry");
            CategoryComboBox.Items.Add("Issue");
            CategoryComboBox.Items.Add("Strategic Task");
            CategoryComboBox.Items.Add("Task");

            RequestingDeptComboBox.Items.Add("Americas F&A");
            RequestingDeptComboBox.Items.Add("Applications");
            RequestingDeptComboBox.Items.Add("Asia F&A");
            RequestingDeptComboBox.Items.Add("Brokerage");
            RequestingDeptComboBox.Items.Add("Bus Dev");
            RequestingDeptComboBox.Items.Add("Call Center");
            RequestingDeptComboBox.Items.Add("Canada");
            RequestingDeptComboBox.Items.Add("CIM");
            RequestingDeptComboBox.Items.Add("Cust Tech");
            RequestingDeptComboBox.Items.Add("Eur F&A");
            RequestingDeptComboBox.Items.Add("FR&P");
            RequestingDeptComboBox.Items.Add("GBS");
            RequestingDeptComboBox.Items.Add("Internal Audit");
            RequestingDeptComboBox.Items.Add("Marketing");
            RequestingDeptComboBox.Items.Add("Pricing/IAS");
            RequestingDeptComboBox.Items.Add("PUNE");
            RequestingDeptComboBox.Items.Add("Rev Rec");
            RequestingDeptComboBox.Items.Add("US F&A");
            RequestingDeptComboBox.Items.Add("Other - TBD");

            HistoryStatusComboBox.Items.Add("Item Not Assigned");
            HistoryStatusComboBox.Items.Add("Analysis in Progress");
            HistoryStatusComboBox.Items.Add("Coding in Progress");
            HistoryStatusComboBox.Items.Add("Testing in Progress");
            HistoryStatusComboBox.Items.Add("Pending Verification");
            HistoryStatusComboBox.Items.Add("Scheduled Implementation");
            HistoryStatusComboBox.Items.Add("Work Delayed");
            HistoryStatusComboBox.Items.Add("Waiting on CIM");
            HistoryStatusComboBox.Items.Add("CIM Knowledge");
            HistoryStatusComboBox.Items.Add("Waiting for Other Group");
            HistoryStatusComboBox.Items.Add("Resolved");

            StatusDatePicker.SelectedDate = DateTime.Today;

            StatusStackPanel.Visibility = Visibility.Hidden;
        }
        
        /*Name: Michael Figueroa
        Function Name: CategoryComboBox_SelectionChanged
        Purpose: Collapses and makes fields visible based on whether or not strategic task is chosen as a category
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: if a business case, then the approriate business case fields become visible; else if strat task, the strat task fields become visible - this is a method I can improve upon; code is admittedly messy 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedIndex == 0)
            {
                Deliverables.Visibility = Visibility.Collapsed;
                DeliverablesBlock.Visibility = Visibility.Collapsed;
                PriorityText.Visibility = Visibility.Visible;
                PriorityNum.Visibility = Visibility.Visible;
                HoursText.Visibility = Visibility.Collapsed;
                HourLabel.Visibility = Visibility.Collapsed;
                CIMValueAddedCheckBox.Visibility = Visibility.Visible;
                BCSub.Visibility = Visibility.Collapsed;
                TeamMembers.Visibility = Visibility.Collapsed;
                TeamMem.Visibility = Visibility.Collapsed;
                ControlEnhancementCheckBox.Visibility = Visibility.Visible;
                ProcessImprovementCheckBox.Visibility = Visibility.Visible;
                HighPriority.Visibility = Visibility.Collapsed;
                AnnualCostSavings.Visibility = Visibility.Collapsed;
                AnnCost.Visibility = Visibility.Collapsed;
                BenefitsText.Visibility = Visibility.Collapsed;
                BenefitsLabel.Visibility = Visibility.Collapsed;

                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("BC Approved");
                StatusComboBox.Items.Add("Active");
                StatusComboBox.Items.Add("Deferred");
                StatusComboBox.Items.Add("Dropped");
                StatusComboBox.Items.Add("Implemented");
                StatusComboBox.Items.Add("Pending");
                StatusComboBox.Items.Add("BC Submitted");

                StatusComboBox.SelectedValue = "Pending";

                ImpacttypeComboBox.Items.Clear();
                ImpacttypeComboBox.Items.Add("Cost Savings");
                ImpacttypeComboBox.Items.Add("Compliance");
                ImpacttypeComboBox.Items.Add("New Revenue");
                ImpacttypeComboBox.Items.Add("Quality");

            }

            else
            {
                if (CategoryComboBox.SelectedIndex == 5)
                {
                    Deliverables.Visibility = Visibility.Visible;
                    DeliverablesBlock.Visibility = Visibility.Visible;
                    PriorityText.Visibility = Visibility.Collapsed;
                    PriorityNum.Visibility = Visibility.Collapsed;
                    HoursText.Visibility = Visibility.Visible;
                    HourLabel.Visibility = Visibility.Visible;
                    CIMValueAddedCheckBox.Visibility = Visibility.Collapsed;
                    ControlEnhancementCheckBox.Visibility = Visibility.Collapsed;
                    BCSub.Visibility = Visibility.Visible;
                    TeamMembers.Visibility = Visibility.Visible;
                    TeamMem.Visibility = Visibility.Visible;
                    BenefitsText.Visibility = Visibility.Visible;
                    BenefitsLabel.Visibility = Visibility.Visible;
                    HighPriority.Visibility = Visibility.Visible;
                    AnnualCostSavings.Visibility = Visibility.Visible;
                    AnnCost.Visibility = Visibility.Visible;

                    StatusComboBox.Items.Clear();
                    StatusComboBox.Items.Add("Open");
                    StatusComboBox.Items.Add("Completed");
                    StatusComboBox.Items.Add("Not Assigned");
                    StatusComboBox.SelectedValue = "Open";
                }
                else
                {
                    Deliverables.Visibility = Visibility.Collapsed;
                    DeliverablesBlock.Visibility = Visibility.Collapsed;
                    PriorityText.Visibility = Visibility.Visible;
                    PriorityNum.Visibility = Visibility.Visible;
                    HoursText.Visibility = Visibility.Collapsed;
                    HourLabel.Visibility = Visibility.Collapsed;
                    CIMValueAddedCheckBox.Visibility = Visibility.Visible;
                    BCSub.Visibility = Visibility.Collapsed;
                    TeamMembers.Visibility = Visibility.Collapsed;
                    TeamMem.Visibility = Visibility.Collapsed;
                    ControlEnhancementCheckBox.Visibility = Visibility.Visible;
                    ProcessImprovementCheckBox.Visibility = Visibility.Visible;
                    HighPriority.Visibility = Visibility.Collapsed;
                    AnnualCostSavings.Visibility = Visibility.Collapsed;
                    AnnCost.Visibility = Visibility.Collapsed;
                    BenefitsText.Visibility = Visibility.Collapsed;
                    BenefitsLabel.Visibility = Visibility.Collapsed;
                    StatusComboBox.Items.Clear();
                    StatusComboBox.Items.Add("Active");
                    StatusComboBox.Items.Add("App Review");
                    StatusComboBox.Items.Add("Closed");
                    StatusComboBox.Items.Add("Pending");

                    StatusComboBox.SelectedValue = "Pending";
                }

                ImpacttypeComboBox.Items.Clear();
                ImpacttypeComboBox.Items.Add("Bad Bill");
                ImpacttypeComboBox.Items.Add("Not Billed Items");
                ImpacttypeComboBox.Items.Add("ISMT");
                ImpacttypeComboBox.Items.Add("Incentive Setup");
                ImpacttypeComboBox.Items.Add("Invoice Display");
                ImpacttypeComboBox.Items.Add("Reporting Issue");
                ImpacttypeComboBox.Items.Add("Compliance");
                ImpacttypeComboBox.Items.Add("Tech Request");
                ImpacttypeComboBox.Items.Add("Abend/Failure");
                ImpacttypeComboBox.Items.Add("Out of Balance");
                ImpacttypeComboBox.Items.Add("Quoting");
                ImpacttypeComboBox.Items.Add("Rate Fail");
                ImpacttypeComboBox.Items.Add("Other");
            }
        }
    }
}