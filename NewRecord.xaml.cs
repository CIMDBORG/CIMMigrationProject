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

namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	NewRecord is the form for logging a new issue into the Issues DB.
    //                  The form contains various textboxes, comboboxes, checkboxes and datepickers which collect
    //                      all the information needed for a new issue. There is also the option to add a new status note.
    //*******************************************************************
    public partial class NewRecord : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;               //local variable to store login-based user data
        private int IDnum;                      //local variable to store issue ID number

        // DESCRIPTION: Constructor, which Takes in user data as input and auto-populates certain fields as the form is loaded, including ADID and name.
        public NewRecord(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;
            Startdatepicker.SelectedDate = DateTime.Today;
        }

        // runs on Submit button click, which then inserts data to New_Contacts and History
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

        // Pulls the data from fields on the form to create the query string and then executes the query
        private string InsertData_NewContacts()
        {
            string title = TitleText.Text.ToString();
            string assigned_to;
            
                assigned_to = arr[2].ToString();

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



        // Pulls the ID number of the issue that was just entered, so that it can be used to add a new status to the History Table (as taskNum). SqlConnection as input.
        private string GetIssueIDQuery()
        {
            string query2 = "select top 1 (ID) from New_Issues order by ID desc";
            return query2;
        }



        // Takes in ID number of newly-inserted issue to return the string that will be the query that will insert the issue history information into the history table
        // Logic included for default values if the user leaves certain fields blank.
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



        // DESCRIPTION: Triggered on "Cancel" button click. Has the user confirm that they want to exit the window
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit form? All information entered will be cleared.", "Cancel Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.Close();
            }
        }



        // Triggered on "Add New Status" button click. Hides the button and displays fields to add a Status.
        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            MoreInfoButton.Visibility = Visibility.Hidden;
            StatusStackPanel.Visibility = Visibility.Visible;
        }


        // DESCRIPTION: Triggered when NewRecord Window loads. String parses the systems of a user, and autofills comboboxes.
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

        // Delimits a user's systems by '/' and adds the systems to the ComboBox in the form
        private void FillSystemComboBox(string systemString)
        {
            char delimiter = '/';
            string[] sys = systemString.Split(delimiter);

            int len = sys.Length;
            for (int x = 0; x < len; x++)
            {
                SystemComboBox.Items.Add(sys[x]);
            }
            SystemComboBox.Items.Add("CIM");
        }



        // Implements logic that displays Status options only available to specific Categories (BC's vs. non-BC's)
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