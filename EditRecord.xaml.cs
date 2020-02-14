using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using WpfApp2;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	The EditRecord Window allows for the user to edit information about an Issue in the database.
    //                  Takes login-based data and prioritization by system row DataRowView from PrioritizationBySystem (PBS) Page.
    //                  Upon loading, the fields of the form auto-populate based on data that is currently stored for the Issue.
    //                  When the user is finished making changes, they have the ability to click "Submit," which will update edited fields in the database
    //                  This form is also role-driven, so certain functionalities are only available to Managers and not Users.
    //                  Users may also add or edit a status note. To Add, click the "Add Status" button. To edit, double click the row user wishes to edit.
    //                          Will take the user to an AddEditStatus window to make these changes
    //*******************************************************************
    public partial class EditRecord : Window
    {
        public String connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private DataRowView priorBySystemRow;           //holds data sent here by row that was clicked 
        private string[] arr;                           //holds login-based user data
        private string[] issue_data;                    //Holds the data about the issue, that will be used to populate the form when it loads
        private Page page;      //Holds the parent prioritization by system page currenty open in the application, which will be updated when the issue is edited.
        private List<int> IDList;

        //*******************************************************************
        // DESCRIPTION: Initializes the EditRecord window, using login-based and PBS data. It calls other functions that fill in the form with existing data.
        //              Also takes as parameter the parent PBS page, where updates will be visible after the edits are made.
        //
        // INPUT:       PrioritizeBySystemPage prioritizeBySystemPage : PBS page currently open in the app. Edits will be made visible on this page after they are submitted.
        //              string[] user_data : login-based user data
        //              DataRowView prioritizationBySystemResultRow : row of PBS table that is sent after clicking 'Edit' button
        //*******************************************************************

        /*Name: Michael Figueroa
        Function Name: EditRecord
        Purpose: Edit Record Constructor for Edit/View DataGrid button click event
        Parameters: Page priorPage, string[] user_data, DataRowView priorRow, List<int> IDListOriginal
        Return Value: N/A
        Local Variables: None
        Algorithm: Assigns global variables based on values passed by parameters in the constructor, calls methods, then collapses the blue updated label
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */

        public EditRecord(Page priorPage, string[] user_data, DataRowView priorRow, List<int> IDListOriginal)
        {
            InitializeComponent();
            page = new Page();
            page = priorPage;
            arr = user_data;
            IDList = IDListOriginal;
            priorBySystemRow = priorRow;

            SetTotalIssuesText();
            SetInitialIDTextBox();
            SelectIssueData(GetIssueID());
            FillInForm();
            BindDataGrid(GetIssueID());
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: EditRecord
        Purpose: Edit Record Constructor for both the Update Required and Aging Report Warning buttons
        Parameters: string[] user_data, DataRowView priorRow, List<int> IDListOriginal
        Return Value: N/A
        Local Variables: None
        Algorithm: Assigns global variables based on values passed by parameters in the constructor, calls methods, then collapses the blue updated label
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */

        public EditRecord(string[] user_data, DataRowView priorRow, List<int> IDListOriginal)
        {
            InitializeComponent();
            arr = user_data;
            IDList = IDListOriginal;
            priorBySystemRow = priorRow;

            SetTotalIssuesText();
            SetInitialIDTextBox();
            SelectIssueData(GetIssueID());
            FillInForm();
            BindDataGrid(GetIssueID());
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
       Function Name: GetIssueID
       Purpose: Getter method that retrieves the number of IDs contained in the report the issue was pulled from
       Parameters: string[] user_data, DataRowView priorRow, List<int> IDListOriginal
       Return Value: Returns ID number of the issue
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */

        private string GetIssueID()
        {

            return priorBySystemRow["ID"].ToString();
        }

        /*Name: Michael Figueroa
        Function Name: GetTotalNumIssues
        Purpose: Getter method that retrieves the total number of issues that are in the report the Edit Record form was accessed from
        Parameters: None
        Return Value: Returns count of List<int> IDList
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */

        private int GetTotalNumIssues()
        {
            return IDList.Count;
        }

       /*Name: Michael Figueroa
       Function Name: SetTotalIssuesText
       Purpose: Setter method that sets TotalIssues.Text
       Parameters: None
       Return Value: None
       Local Variables: None
       Algorithm: Uses GetTotalNumIssues() in order to set TotalIssues.Text
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */

        private void SetTotalIssuesText()
        {
            TotalIssues.Text = "Of " + GetTotalNumIssues().ToString();
        }


       /*Name: Michael Figueroa
       Function Name: SetInitialIDTextBox
       Purpose: Setter Method
       Parameters: string[] user_data, DataRowView priorRow, List<int> IDListOriginal
       Return Value: N/A
       Local Variables: None
       Algorithm: Assigns global variables based on values passed by parameters in the constructor, calls methods, then collapses the blue updated label
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */

        private void SetInitialIDTextBox()
        {
            int issueID = Int32.Parse(GetIssueID());

            CurrentIssue.Text = (IDList.IndexOf(issueID) + 1).ToString();
        }

        /*Name: Michael Figueroa
        Function Name: GetCurrentID()
        Purpose: Getter method that retrieves the ID of the current issue being displayed on screen
        Parameters: None
        Return Value: The ID of the current issue being displayed on screen
        Local Variables: None
        Algorithm: Assigns global variables based on values passed by parameters in the constructor, calls methods, then collapses the blue updated label
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
  
        private int GetCurrentID()
        {
            int current = Int32.Parse(CurrentIssue.Text.ToString()) - 1;
            return IDList[current];
        }

       /*Name: Michael Figueroa
       Function Name: Window_Loaded
       Purpose: Event handler that runs when he Edit form first loads
       Parameters: Auto-generated
       Return Value: None
       Local Variables: None
       Algorithm: Adds to Category, Requesting Dept comboboxes, then sets permissions based on whether a user is a Manager or a regular User
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
            RequestingDeptComboBox.Items.Add("External Cust.");
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


            if (arr[6] == "User")
            {
                ManagerReviewCheckBox.IsEnabled = false;
                UpdateRequiredCheckBox.IsEnabled = false;
                ManagerNotesText.IsReadOnly = true;
            }
        }

        /*Name: Michael Figueroa
        Function Name: CategoryComboBox_SelectionChanged
        Purpose: Event handler that runs every time the category combobox selection changes
        Parameters: Auto-generated
        Return Value: None
        Local Variables: None
        Algorithm: If category is strategic task (SekectedIndex = 5), then the appropriate strategic task fields collapse/become visible; 
        otherwise the form displays the fields appropriate to all other issues. Lastly, calls CurrentStatus
        If the category is BC/TI, then the status combobox is filled with BC Specific fields; else, the normal selection of statuses is chosen
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedIndex == 5)
            {
                PriorityNum.Visibility = Visibility.Collapsed;
                PriorityText.Visibility = Visibility.Collapsed;
                DeliverablesTextBox.Visibility = Visibility.Visible;
                DeliverablesLabel.Visibility = Visibility.Visible;
                ControlEnhancementCheckBox.Visibility = Visibility.Collapsed;
                CIMValueAddedCheckBox.Visibility = Visibility.Collapsed;
                HotTopicCheckBox.Visibility = Visibility.Collapsed;
                BCSub.Visibility = Visibility.Visible;
                InternalNotes.Visibility = Visibility.Collapsed;
                InternalNotesText.Visibility = Visibility.Collapsed;
                HoursText.Visibility = Visibility.Visible;
                HourLabel.Visibility = Visibility.Visible;
                Benefits.Visibility = Visibility.Visible;
                BenefitsText.Visibility = Visibility.Visible;
                AnnCost.Visibility = Visibility.Visible;
                AnnualCost.Visibility = Visibility.Visible;
                Implemented.Visibility = Visibility.Visible;
                Presented.Visibility = Visibility.Visible;
                TeamMembers.Visibility = Visibility.Visible;
                TeamMem.Visibility = Visibility.Visible;
                HighPriority.Visibility = Visibility.Visible;
                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("Open");
                StatusComboBox.Items.Add("Completed");
                StatusComboBox.Items.Add("Not Assigned");
            }
            else
            {
                DeliverablesTextBox.Visibility = Visibility.Collapsed;
                DeliverablesLabel.Visibility = Visibility.Collapsed;
                ControlEnhancementCheckBox.Visibility = Visibility.Visible;
                ProcessImprovementCheckBox.Visibility = Visibility.Visible;
                CIMValueAddedCheckBox.Visibility = Visibility.Visible;
                HotTopicCheckBox.Visibility = Visibility.Visible;
                BCSub.Visibility = Visibility.Collapsed;
                HoursText.Visibility = Visibility.Collapsed;
                HourLabel.Visibility = Visibility.Collapsed;
                InternalNotes.Visibility = Visibility.Visible;
                InternalNotesText.Visibility = Visibility.Visible;
                Benefits.Visibility = Visibility.Collapsed;
                BenefitsText.Visibility = Visibility.Collapsed;
                AnnCost.Visibility = Visibility.Collapsed;
                AnnualCost.Visibility = Visibility.Collapsed;
                Implemented.Visibility = Visibility.Collapsed;
                Presented.Visibility = Visibility.Collapsed;
                TeamMembers.Visibility = Visibility.Collapsed;
                TeamMem.Visibility = Visibility.Collapsed;
                HighPriority.Visibility = Visibility.Collapsed;
                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("Active");
                StatusComboBox.Items.Add("App Review");
                StatusComboBox.Items.Add("Closed");
                StatusComboBox.Items.Add("Pending");
            }

            if (CategoryComboBox.SelectedIndex == 0)
            {            
                    StatusComboBox.Items.Clear();
                    StatusComboBox.Items.Add("BC Approved");
                    StatusComboBox.Items.Add("Active");
                    StatusComboBox.Items.Add("Implemented");
                    StatusComboBox.Items.Add("Deferred");
                    StatusComboBox.Items.Add("Dropped");
                    StatusComboBox.Items.Add("Pending");
                    StatusComboBox.Items.Add("BC Submitted");
                    StatusComboBox.Items.Add("Closed");

                    ImpacttypeComboBox.Items.Clear();
                    ImpacttypeComboBox.Items.Add("Cost Savings");
                    ImpacttypeComboBox.Items.Add("Compliance");
                    ImpacttypeComboBox.Items.Add("New Revenue");
                    ImpacttypeComboBox.Items.Add("Reporting Issue");
                    ImpacttypeComboBox.Items.Add("Quality");
            }

            else
            {
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
            Helper.CurrentStatus(GetCurrentID().ToString(), StatusComboBox);
        }

        /*Name: Michael Figueroa
        Function Name: IsInt
        Purpose: Checks whether or not a value is an Int
        Parameters: None
        Return Value: bool
        Local Variables: string priority_Number, string bcTi, string oneTime, string annual benefit, string[] intArray, int number
        Algorithm: priority number, BCTI, one time benefit, and annual benefit values are retrieved from form, then put into a string array
        The for loop checks whether any of those values are not numbers using TryParse; if the parse is not successful, the value is not a mumber and
        method returns false; if parse is successful, it returns true
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private bool IsInt()
        {
            string priority_Number = PriorityText.Text.ToString();
            string bcTi = BCTItext1.Text.ToString();
            string oneTime = OneTimeBenefitText.Text.ToString();
            string annualBenefit = AnnualBenefitText.Text.ToString();

            string[] intArray = new String[] { priority_Number, bcTi, oneTime, annualBenefit };

            int number = 0;
            for (int i = 0; i < intArray.Length; i++)
            {
                if (!(Int32.TryParse(intArray[i], out number)))
                {
                    return false;
                }
            }
            return true;
        }

        /*Name: Michael Figueroa
        Function Name: UserUpdateQuery
        Purpose: Update query for regular users
        Parameters: string ID
        Return Value: the user update query
        Local Variables: string plannedDate, string compdate, string BCTI, string updatedDate, string query
        Algorithm: Sets plannedDate, compDate variables to null if input is empty, else sets it to date user chooses, 
        sets oneTime benefit and annualbenefit, sets BCTI to null is input is empty, annualcost fields to 0 if fields are empty,  
        Then, updates the list of dates on which updates have been made by user.
        The query is variable is then set; if the issue is a strategic task, then those fields specific to strategic tasks are updated
        else, non-strategic task fields are updated only.
        If the user has been prompted to update the issue, the user_update_bit is set to 1, meaning an update has been made
        if they have not been prompted to update the issue, the user_update_bit is not effected at all
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string UserUpdateQuery(string ID)
        {
            string plannedDate;
            if (Planneddatepicker.Text.Length == 0)
            {
                plannedDate = "NULL";
            }
            else
            {
                plannedDate = "'" + Planneddatepicker.SelectedDate.ToString() + "'";
            }

            if (OneTimeBenefitText.Text.Length == 0)
            {
                OneTimeBenefitText.Text = "0";
            }

            if (AnnualBenefitText.Text.Length == 0)
            {
                AnnualBenefitText.Text = "0";
            }

            if (AnnualCost.Text.Length == 0)
            {
                AnnualCost.Text = "0";
            }

            string compDate;
            if (CompDatePicker.Text.Length == 0)
            {
                compDate = "NULL";
            }

            else
            {
                compDate = "'" + CompDatePicker.SelectedDate.ToString() + "'";
            }

            string BCTI;
            if (BCTItext1.Text.ToString().Length == 0)
            {
                BCTI = "NULL";
            }
            else
            {
                BCTI = BCTItext1.Text.ToString();
            }


            string updatedDate = GetUpdatedDateString();
            string query;

            if (CategoryComboBox.Text.ToString() != "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                query = "UPDATE New_Issues SET Title = '" + TitleText.Text.Replace("'", "\''") + "', User_Update_Bit = 1, User_Update = '" + updatedDate + "', Req_Dept = '" + RequestingDeptComboBox.SelectedItem.ToString() + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num=" + BCTI + ", " +
                                    "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", " + "Supporting_Details='" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "Internal_Notes='" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', " + "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " +
                                    "Mgr_Notes='" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                                    "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString().Replace(",", "") + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString().Replace(",", "") + ", New_Title = '" + TitleText.Text.Replace("'", "\''") + "', New_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") +
                                    "', New_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "', CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                                    "WHERE ID = " + ID + ";";
            }

            else if(CategoryComboBox.Text.ToString() == "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                query = "UPDATE New_Issues SET Title = '" + TitleText.Text.Replace("'", "\''") + "', User_Update_Bit = 1, User_Update = '" + updatedDate + "', Req_Dept = '" + RequestingDeptComboBox.SelectedItem.ToString() + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num=" + BCTI + ", " +
                                    "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Supporting_Details='" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " + "Hours=" + HoursText.Text.ToString() + ", " + "Benefits='" + BenefitsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "HP='" + HighPriority.IsChecked.ToString() + "', " +"Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                                    "Mgr_Notes='" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " + "Imp='" + Implemented.IsChecked.ToString() + "', " + "BCSub='" + BCSub.IsChecked.ToString() + "', " +
                                    "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Presented='" + Presented.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString().Replace(",", "") + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString().Replace(",", "") + ", New_Title = '" + TitleText.Text.Replace("'", "\''") + "', New_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") +
                                    "', New_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', Team_Members = ' " + TeamMembers.Text.ToString() + "', Assigned_To = '" + Owner.Text.ToString() + "' " + ", " + "Annual_Cost_Savings=" + AnnualCost.Text.ToString().Replace(",", "") + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                                     " WHERE ID = " + ID + ";";
            }

            else if(CategoryComboBox.Text.ToString() != "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "False")
            {
                query = "UPDATE New_Issues SET Title = '" + TitleText.Text.Replace("'", "\''") + "', Req_Dept = '" + RequestingDeptComboBox.SelectedItem.ToString() + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num=" + BCTI + ", " +
                                    "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", " + "Supporting_Details='" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "Internal_Notes='" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', " + "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " +
                                    "Mgr_Notes='" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                                    "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString().Replace(",", "") + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString().Replace(",", "") + ", New_Title = '" + TitleText.Text.Replace("'", "\''") + "', New_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") +
                                    "', New_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "', CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                                    "WHERE ID = " + ID + ";";
            }

            else
            {
                query = "UPDATE New_Issues SET Title = '" + TitleText.Text.Replace("'", "\''") + "', Req_Dept = '" + RequestingDeptComboBox.SelectedItem.ToString() + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num=" + BCTI + ", " +
                                    "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Supporting_Details='" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " + "Hours=" + HoursText.Text.ToString() + ", " + "Benefits='" + BenefitsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "HP='" + HighPriority.IsChecked.ToString() + "', Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                                    "Mgr_Notes='" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " + "Imp='" + Implemented.IsChecked.ToString() + "', " + "BCSub='" + BCSub.IsChecked.ToString() + "', " +
                                    "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Presented='" + Presented.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                                    "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString() + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString() + ", New_Title = '" + TitleText.Text.Replace("'", "\''") + "', New_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") +
                                    "', New_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', Team_Members = ' " + TeamMembers.Text.ToString() + "', Assigned_To = '" + Owner.Text.ToString() + "' " + ", " + "Annual_Cost_Savings=" + AnnualCost.Text.ToString().Replace(",", "") + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                                     " WHERE ID = " + ID + ";";
            }
                return query;
        }

        /*Name: Michael Figueroa
        Function Name: GetUpdatedDateString
        Purpose: This is for use in UserUpdateQuery and ManagerUpdateQuery methods; this updates the Manager_Update and User_Update
        SQL fields for the particular issue being edited
        Parameters: None
        Return Value: string containing the list of dates on which updates have been made, separated by a delimiter
        Local Variables: string todaysDate, StringBuilder updateDateBuilder
        Algorithm: Today's date is retrieved using DateTime.Now; then query is run; reader checks if the user is a Manager or a 
        regular User; if manager, the Manager_Update field is updated, else if user, then the User_Update field is updated.
        The update is done by checking if the field is null; if it is not null, the stringbuilder appends the current date to the 
        value retrieved by the query, and then returned. Else if the values are null, then todaysDate is the return value
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string GetUpdatedDateString()
        {
            string todaysDate = DateTime.Now.ToString("M/d/yyyy");
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    StringBuilder updateDateBuilder;
                    string query = "SELECT Manager_Update, User_Update FROM New_Issues WHERE ID = " + GetIssueID();
                    using (SqlCommand IDCmd = new SqlCommand(query, con))
                    using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            if (IsManager())
                            {
                                if (!reader2.IsDBNull(0))
                                {
                                    updateDateBuilder = new StringBuilder(reader2.GetString(0).ToString());
                                    updateDateBuilder.Append(";" + todaysDate);
                                    return updateDateBuilder.ToString();
                                }

                                else
                                {
                                    return todaysDate;
                                }
                            }

                            else
                            {
                                if (!reader2.IsDBNull(1))
                                {
                                    updateDateBuilder = new StringBuilder(reader2.GetString(1).ToString());
                                    updateDateBuilder.Append(";" + todaysDate);
                                    return updateDateBuilder.ToString();
                                }

                                else
                                {
                                    return todaysDate;
                                }
                            }
                        }
                        reader2.Close();
                    }
                }


                catch (Exception)
                {
                    MessageBox.Show("No date retrieved");
                    return null;
                }

            return null;
        }

        /*Name: Michael Figueroa
        Function Name: ManagerUpdateQuery
        Purpose: This is to set the update query from a manager perspective
        Parameters: string IDs
        Return Value: string containing the list of dates on which updates have been made, separated by a delimiter
        Local Variables: string todaysDate, StringBuilder updateDateBuilder
        Algorithm: Sets plannedDate, compDate variables to null if input is empty, else sets it to date user chooses, 
        sets oneTime benefit and annualbenefit, sets BCTI to null is input is empty, annualcost fields to 0 if fields are empty, 
        sets userUpdateBit to 0 is the user update box is checked, else, userUpdateBit is set to true, signifying no update is to be made
        Then, updates the list of dates on which updates have been made by user
        The query variable is then set; if the issue is a strategic task, then those fields specific to strategic tasks are updated
        else, non-strategic task fields are updated only.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string ManagerUpdateQuery(string ID)
        {
            string plannedDate;
            if (Planneddatepicker.Text.Length == 0)
            {
                plannedDate = "NULL";
            }
            else
            {
                plannedDate = "'" + Planneddatepicker.SelectedDate.ToString() + "'";
            }

            if (OneTimeBenefitText.Text.Length == 0)
            {
                OneTimeBenefitText.Text = "0";
            }

            if (AnnualBenefitText.Text.Length == 0)
            {
                AnnualBenefitText.Text = "0";
            }

            if (AnnualCost.Text.Length == 0)
            {
                AnnualCost.Text = "0";
            }

            string compDate;
            if (CompDatePicker.Text.Length == 0)
            {
                compDate = "NULL";
            }

            else
            {
                compDate = "'" + CompDatePicker.SelectedDate.ToString() + "'";
            }

            string updatedDate = GetUpdatedDateString();
            string setUserUpdateBit;

            if (UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                setUserUpdateBit = "False";
            }
            else
            {
                setUserUpdateBit = "True";
            }
            string BCTI;
            if (BCTItext1.Text.ToString().Length == 0)
            {
                BCTI = "NULL";
            }
            else
            {
                BCTI = BCTItext1.Text.ToString();
            }

            string query;
            if (CategoryComboBox.Text.ToString() != "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                query = "UPDATE New_Issues SET Manager_Update = '" + updatedDate + "', Manager_Update_Bit = 1, User_Update_Bit = '" + setUserUpdateBit + "', Mgr_Notes = '" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " +
                           "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                            "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num='" + BCTI + "', " +
                            "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", Deliverables = '" + DeliverablesTextBox.Text.ToString() + "', " +
                            "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                            "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Title = '" + TitleText.Text.Replace("'", "\''") + "', Supporting_Details = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                            "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                     "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString().Replace(",", "") + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString().Replace(",", "") + ", " + "Original_Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', " +
                     "Original_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                     "Original_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Internal_Notes = '" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', " +
                     "ManagerReview = '" + ManagerReviewCheckBox.IsChecked.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "' " + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                            "WHERE ID = " + ID + ";";
            }
            else if(CategoryComboBox.Text.ToString() == "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                query = "UPDATE New_Issues SET Manager_Update = '" + updatedDate + "', Manager_Update_Bit = 1, User_Update_Bit = '" + setUserUpdateBit + "', Mgr_Notes = '" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num='" + BCTI + "', " +
                             "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", Deliverables = '" + DeliverablesTextBox.Text.ToString() + "', " +
                             "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                             "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', Supporting_Details = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                             "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Presented='" + Presented.IsChecked.ToString() + "', " +
                      "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString().Replace(",", "") + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString().Replace(",", "") + ", " + "Original_Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', " +
                      "Original_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " + "Imp='" + Implemented.IsChecked.ToString() + "', " + "BCSub='" + BCSub.IsChecked.ToString() + "', " +
                      "Original_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Hours=" + HoursText.Text.ToString() + ", " + "Benefits='" + BenefitsText.Text.ToString().Replace("'", "\''") + "', " + "Internal_Notes = '" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', " +
                      "Team_Members = ' " + TeamMembers.Text.ToString() + "', ManagerReview = '" + ManagerReviewCheckBox.IsChecked.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "' " + ", " + "Annual_Cost_Savings=" + AnnualCost.Text.ToString() + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                             " WHERE ID = " + ID + ";";
            }

            else if (CategoryComboBox.Text.ToString() != "Strategic Task" && UpdateRequiredCheckBox.IsChecked.ToString() == "False")
            {
                query = "UPDATE New_Issues SET Mgr_Notes = '" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " +
                           "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                            "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num=" + BCTI + ", " +
                            "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", Deliverables = '" + DeliverablesTextBox.Text.ToString() + "', " +
                            "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                            "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Title = '" + TitleText.Text.Replace("'", "\''") + "', Supporting_Details = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                            "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " +
                     "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString() + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString() + ", " + "Original_Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', " +
                     "Original_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                     "Original_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Internal_Notes = '" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', " +
                     "ManagerReview = '" + ManagerReviewCheckBox.IsChecked.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "' " + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                            "WHERE ID = " + ID + ";";
            }

            else
            { 
                query = "UPDATE New_Issues SET Mgr_Notes = '" + ManagerNotesText.Text.ToString().Replace("'", "\''") + "', " +
                            "Req_Name = '" + RequestedbyText.Text.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", Completed_Date = " + compDate + ", " +
                             "[Status]='" + StatusComboBox.SelectedItem.ToString() + "', " + "Category='" + CategoryComboBox.SelectedItem.ToString() + "', " + "TFS_BC_HDFS_Num='" + BCTI + "', " +
                             "Sys_Impact='" + SystemComboBox.SelectedItem.ToString() + "', " + "Priority_Number=" + PriorityText.Text.ToString() + ", Deliverables = '" + DeliverablesTextBox.Text.ToString() + "', " +
                             "Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', " + "[Control]='" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() + "', " +
                             "Impact='" + ImpacttypeComboBox.SelectedItem.ToString() + "', " + "Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', Supporting_Details = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " +
                             "Cim_Val='" + CIMValueAddedCheckBox.IsChecked.ToString() + "', " + "Bus_Impact='" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Presented='" + Presented.IsChecked.ToString() + "', " +
                      "OneTimeBenefit=" + OneTimeBenefitText.Text.ToString() + ", " + "AnnualBenefit=" + AnnualBenefitText.Text.ToString() + ", " + "Original_Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', " +
                      "Original_Sup_Dtls = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', " + "Imp='" + Implemented.IsChecked.ToString() + "', " + "BCSub='" + BCSub.IsChecked.ToString() + "', " +
                      "Original_Bus_Imp = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', " + "Hours=" + HoursText.Text.ToString() + ", " + "Benefits='" + BenefitsText.Text.ToString().Replace("'", "\''") + "', " + "Internal_Notes = '" + InternalNotesText.Text.ToString().Replace("'", "\''") + "', Assigned_To = '" + Owner.Text.ToString() + "', " +
                      "Team_Members = ' " + TeamMembers.Text.ToString() + "', ManagerReview = '" + ManagerReviewCheckBox.IsChecked.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "' " + ", " + "Annual_Cost_Savings=" + AnnualCost.Text.ToString() + ", CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "' " +
                             " WHERE ID = " + ID + ";";
            }

            return query;
        }

        /*Name: Michael Figueroa
        Function Name: UserUpdateReady
        Purpose: Determines whether a user has made an update; this is used for when a manager has requested an update to an issue
        Parameters: None
        Return Value: bool
        Local Variables: string query
        Algorithm: Original title, supplementary details, business impacts, Manager_Update_Bit, User_Update_Bit
        are all selected in a query. 
        If Manager_Update_Bit is null, then returns true; else if both Manager_Update_Bit and User_Update_Bit are true or both false, then returns true;
        If title, sup details, or bus impacts have been edited (are different to original columns), then returns true
        else returns false and the user is notified that they must make an edit before updating the issue
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private bool UserUpdateReady()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();

                    string query = "SELECT Original_Title, Original_Sup_Dtls, Original_Bus_Imp, Manager_Update_Bit, User_Update_Bit FROM New_Issues WHERE ID = " + GetIssueID();
                    //if the original fields are null, that means no update has been required, and the user is free to edit as they wish. 
                    using (SqlCommand IDCmd = new SqlCommand(query, con))
                    using (SqlDataReader reader2 = IDCmd.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            if (reader2.IsDBNull(0))
                            {
                                return true;
                            }

                            else if ((reader2.GetBoolean(3) == true && reader2.GetBoolean(4) == true) || (reader2.GetBoolean(3) == false && reader2.GetBoolean(4) == false))
                            {
                                return true;
                            }
                            else if ((reader2.GetString(0).ToString() != TitleText.Text.ToString()) || (reader2.GetString(1).ToString() != SupportingDetailsText.Text.ToString()) || (reader2.GetString(2).ToString() != BusinessImpactsText.Text.ToString()))
                            {
                                return true;
                            }

                            else
                            {
                                return false;
                            }
                        }
                        reader2.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MessageBox.Show("Error with SqlReader, Please Contact Developer");
                    return false;
                }
                finally
                {
                    con.Close();
                }
            return true;
        }

        /*Name: Michael Figueroa
        Function Name: IsManager
        Purpose: Determines whether user is manager
        Parameters: None
        Return Value: bool
        Local Variables: None
        Algorithm: if arr[] = "Manager" 
        are all selected in a query. 
        If Manager_Update_Bit is null, then returns true; else if both Manager_Update_Bit and User_Update_Bit are true or both false, then returns true;
        If title, sup details, or bus impacts have been edited (are different to original columns), then returns true
        else returns false and the user is notified that they must make an edit before updating the issue
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private bool IsManager()
        {
            if (arr[6] == "Manager" || arr[6] == "Program Manager")
            {
                return true;
            }
            return false;
        }


        /*Name: Michael Figueroa
        Function Name: SubmitIssue
        Purpose: This executes the update query. 
        Parameters: None
        Return Value: bool
        Local Variables: string ID, string managerQuery
        Algorithm: first, string ID is assigned using GetCurrentID. Then, if the current user logged in is a manager and not assigned_to the issue being edited, 
        then the ManageUpdateQuery is executed, and the form is re-binded
        with updated info; else, user update is executed and the form is re-binded with updates
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SubmitIssue()
        {
            string ID = GetCurrentID().ToString();
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();

                    if ((IsManager()) && (issue_data[25] != arr[2]))
                    {
                        string managerQuery = ManagerUpdateQuery(ID);
                        SqlCommand managerCmd = new SqlCommand(managerQuery, connection);
                        managerCmd.ExecuteNonQuery();
                        SelectIssueData(GetCurrentID().ToString());
                        FillInForm();
                        BindDataGrid(GetCurrentID().ToString());
                        Updated.Visibility = Visibility.Visible;

                    }
                    else if (UserUpdateReady())
                    {
                        string userQuery = UserUpdateQuery(ID);
                        SqlCommand userCmd = new SqlCommand(userQuery, connection);
                        userCmd.ExecuteNonQuery();
                        SelectIssueData(GetCurrentID().ToString());
                        FillInForm();
                        BindDataGrid(GetCurrentID().ToString());
                        Updated.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Update Failed: Must Make Changes to Title, Supplementary Details, Or Business Impacts");
                    }
                }

                //Exception handling
                catch (NullReferenceException)
                {
                    if (ImpacttypeComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Please Choose an Impact Type from the Dropdown Menu");
                    }
                    if (PriorityText.Text.Length == 0)
                    {
                        MessageBox.Show("Please Enter a Priority Number");
                    }
                    if (RequestingDeptComboBox.SelectedItem == null)
                    {
                        MessageBox.Show("Please choose a Requesting Dept");
                    }
                    if(CategoryComboBox.SelectedItem.ToString() == "Strategic Task" && HoursText.Text.Length == 0)
                    {
                        MessageBox.Show("Hours Field Required for Strategic Tasks");
                    }
                }


                catch (SqlException ex)
                {
                    if (!IsInt())
                    {
                        MessageBox.Show("Priority Number, BC/TI, One-Time Benefit and Annual Benefit must all be integer values");
                        MessageBox.Show(ex.ToString());
                    }

                    else
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }


                finally
                {
                    connection.Close();
                }
        }


        /*Name: Michael Figueroa
        Function Name: CancelButton_Click
        Purpose: Event handler for cancel button that asks user if they want to exist form
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: MessageBoxResult messageBoxResult
        Algorithm: MessageBox pops up asking if user wants to exit form; if they click ok, they exit and none of the info on the form saves, else, nothing happens
        are all selected in a query. 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit form? All information entered will be cleared.", "Cancel Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

        /*Name: Michael Figueroa
        Function Name: CompDatePicker_SelectedDateChanged
        Purpose: Event handler for when the CompDatePicker changes (This is only for when the CompDate is filled)
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: If a date is selected in the Comp Date field on the form, then the status is changed to "Closed"
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A - I would like someone to revise this at some point
        */
        private void CompDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompDatePicker.Text.Length != 0)
            {
                StatusComboBox.SelectedIndex = 2;
            }
        }



        /*Name: Michael Figueroa
        Function Name: CompDatePicker_SelectedDateChanged
        Purpose: Event handler for when the CompDatePicker changes
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: "Implemented" for BC's and "Closed" for all other categories are both at index 2; if statusComboBox is index 2, then the CompDatePicker is today
        else - the comp date picker field is null
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A - I would like someone to revise this at some point
        */
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusComboBox.SelectedIndex == 2)
            {
                CompDatePicker.SelectedDate = DateTime.Today;
            }
            else
            {
                CompDatePicker.SelectedDate = null;
            }
        }



        /*Name: Michael Figueroa
        Function Name: SelectIssueData
        Purpose: Fills out the global variable string[] issue_data with the appropriate data of the particular ID
        Parameters: string ID
        Return Value: None
        Local Variables: string query, string data[], int cols
        Algorithm: query is assigned, then the reader goes through the query in a for loop equal to the length of how many cols there are; approriate data is written into the array
        (Title, Req_Dept, Req_Name, etc. etc.) - the reader is then closed.
        IF there is an exception: Error message is displayed, issue_data remains empty
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SelectIssueData(string ID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {

                    string query = "SELECT Title, Req_Dept, Req_Name, Opened_Date, Due_Date, [Status], Category, TFS_BC_HDFS_Num, Sys_Impact, Priority_Number, " +
                                       "Supporting_Details, Internal_Notes, BC_Approved, Hot_Topic, ISNULL(Mgr_Notes, '') as Mgr_Notes, [Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, " +
                                       "OneTimeBenefit, AnnualBenefit, ManagerReview, WeeklyReview, User_Update_Bit, Assigned_To, Manager_Update_Bit, Completed_Date, Hours, Annual_Cost_Savings, " +
                                       "Benefits, HP, Deliverables, BCSub, ManagerMeeting, CIM_Know, Team_Members FROM New_Issues WHERE ID=" + ID + ";";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();
                    int cols = reader.FieldCount;
                    string[] data = new string[cols];
                    while (reader.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            data[x] = reader.GetValue(x).ToString();
                        }

                    }
                    reader.Close();

                    connection.Close();

                    issue_data = data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                    string[] data = new string[25];
                    issue_data = data;
                }
                finally
                {
                    connection.Close();
                }
        }



        /*Name: Michael Figueroa
        Function Name: FillInForm
        Purpose: Fills out the XAML EditRecord form with the information specific to the current issue
        Parameters: None
        Return Value: None
        Local Variables: string query, string data[], int cols, DateTime myStartDate, DateTime myDueDate, DateTime myCompDate
        Algorithm: Fills in the appropriate fields on the EditRecord.xaml form using the issue_data array filled in SelectIssueData()
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void FillInForm()
        {
            TitleText.Text = issue_data[0].ToString();
            RequestingDeptComboBox.SelectedItem = issue_data[1].ToString();
            RequestedbyText.Text = issue_data[2].ToString();

            //Parses strings containing dates
            DateTime myStartDate;
            DateTime myDueDate;
            DateTime myCompDate;

            if (DateTime.TryParse(issue_data[3], out myStartDate))
            {
                Startdatepicker.SelectedDate = myStartDate;
            }

            else
            {
                Startdatepicker.SelectedDate = null;
            }

            if (DateTime.TryParse(issue_data[4], out myDueDate))
            {
                Planneddatepicker.SelectedDate = myDueDate;
            }
            else
            {
                Planneddatepicker.SelectedDate = null;
            }

            if (DateTime.TryParse(issue_data[27], out myCompDate))
            {
                CompDatePicker.SelectedDate = myCompDate;
            }
            else
            {
                CompDatePicker.SelectedDate = null;
            }

            StatusComboBox.SelectedItem = issue_data[5].ToString();
            CategoryComboBox.SelectedItem = issue_data[6].ToString();
            BCTItext1.Text = issue_data[7].ToString();
            SystemComboBox.SelectedItem = issue_data[8].ToString();
            PriorityText.Text = issue_data[9].ToString();
            SupportingDetailsText.Text = issue_data[10].ToString();
            InternalNotesText.Text = issue_data[11].ToString();

            if (issue_data[13].ToString() == "True")
            {
                HotTopicCheckBox.IsChecked = true;
            }

            else
            {
                HotTopicCheckBox.IsChecked = false;
            }

            ManagerNotesText.Text = issue_data[14].ToString();

            if (issue_data[15].ToString() == "True")
            {
                ControlEnhancementCheckBox.IsChecked = true;
            }

            else
            {
                ControlEnhancementCheckBox.IsChecked = false;
            }

            if (issue_data[16].ToString() == "True")
            {
                ProcessImprovementCheckBox.IsChecked = true;
            }

            else
            {
                ProcessImprovementCheckBox.IsChecked = false;
            }

            ImpacttypeComboBox.SelectedItem = issue_data[17].ToString();

            if (issue_data[18].ToString() == "True")
            {
                CIMValueAddedCheckBox.IsChecked = true;
            }

            else
            {
                CIMValueAddedCheckBox.IsChecked = false;
            }

            BusinessImpactsText.Text = issue_data[19].ToString();
            OneTimeBenefitText.Text = issue_data[20].ToString();
            AnnualBenefitText.Text = issue_data[21].ToString();

            if (issue_data[22].ToString() == "True")
            {
                ManagerReviewCheckBox.IsChecked = true;
            }
            else
            {
                ManagerReviewCheckBox.IsChecked = false;
            }

            if (issue_data[23] == "True")
            {
                WeeklyReviewChk.IsChecked = true;
            }

            if (issue_data[24].ToString() == "False" && issue_data[26].ToString() == "True")
            {
                UpdateRequiredCheckBox.IsChecked = true;
            }

            else
            {
                UpdateRequiredCheckBox.IsChecked = false;
            }

            Owner.Text = issue_data[25].ToString();

            HoursText.Text = issue_data[28].ToString();
            AnnualCost.Text = issue_data[29].ToString();
            BenefitsText.Text = issue_data[30].ToString();
            if (issue_data[31].ToString() == "True")
            {
                HighPriority.IsChecked = true;
            }
            else
            {
                HighPriority.IsChecked = false;
            }
            DeliverablesTextBox.Text = issue_data[32].ToString();
            if (issue_data[33].ToString() == "True")
            {
                BCSub.IsChecked = true;
            }
            else
            {
                BCSub.IsChecked = false;
            }

            if (issue_data[34].ToString() == "True")
            {
                managerMeetingCheckBox.IsChecked = true;
            }
            else
            {
                managerMeetingCheckBox.IsChecked = false;
            }

            if (issue_data[35].ToString() == "True")
            {
                CIMKnowCheckBox.IsChecked = true;
            }

            else
            {
                CIMKnowCheckBox.IsChecked = false;
            }

            TeamMembers.Text = issue_data[36].ToString();
        }

        /*Name: Michael Figueroa
        Function Name: AddStatusButton_Click
        Purpose: Event handler for Add Status button
        Parameters: Auto Generated
        Return Value: None
        Local Variables: None
        Algorithm: If the Add Status button is clicked, then the EditRecord_AddEditStatus form is shown
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void AddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            EditRecord_AddEditStatus addStatus = new EditRecord_AddEditStatus(this, GetCurrentID());
            addStatus.Show();
        }



        /*Name: Michael Figueroa
        Function Name: BindDataGrid
        Purpose: Binds the History DataGrid which shows the status history for the current issue
        Parameters: string TaskNum
        Return Value: None
        Local Variables: string query, DataTable dt
        Algorithm: query is assigned based on the issue ID number (the TaskNum variable here is equivalent to the Issue ID), the DataTable is filled using the query, and then the
        DataGrid ItemSource (ItemSource is basically the information that will binded to the front-end) is set equal to the information from DataTable dt
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        //*******************************************************************
        public void BindDataGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    string query = "select History.ID, format(EntryDate, 'MM/dd/yyyy') as EntryDate, New_StatusNote as [Status], [Status] as Status_Note " +
                                   "from History where TaskNum = " + TaskNum + " AND New_StatusNote != 'Aging' order by History.ID desc;";

                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        sda.Fill(dt);
                    }
                    Report.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
        }


        /*Name: Michael Figueroa
        Function Name: Report_MouseDoubleClick
        Purpose: DoubleClick even handler for the history form that allows user to edit status
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataGrid dg, DataRowView dataRow
        Algorithm: When a specific row on the History table is double-clicked, the editStatus screen comes up.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Report_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataGrid dg = (DataGrid)sender;
                DataRowView dataRow = dg.SelectedItem as DataRowView;

                if (dataRow != null)
                {
                    //Pass this window, priorBySystem DataRowView, and the DataRowView that was generated from the double-click to a new AddEditStatus window
                    EditRecord_AddEditStatus editStatus = new EditRecord_AddEditStatus(this, GetCurrentID(), dataRow);

                    editStatus.Show();
                    editStatus.Topmost = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /*Name: Michael Figueroa
        Function Name: BackArrow_Click
        Purpose: Event Handler that allows user to scroll through the issues (in a backwards manner) in the report the EditRecord form was accessed from
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string current, int currentID
        Algorithm: currentID is the index of the List<int> IDLIst array; if the currentID value subtracted by 1 is greater or equal to zero, then the currentID is decremented by 1,
        The History DataGrid and Form are all re-binded with the ID of the previous issue in the report; else, nothing happens, in order to avoid and indexOutofBounds exception in
        IDList array.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void BackArrow_Click(object sender, RoutedEventArgs e)
        {
            string current = CurrentIssue.Text.ToString();
            int currentID = Int32.Parse(current) - 1;
            if ((currentID - 1) >= 0)
            {
                currentID--;

                CurrentIssue.Text = (currentID + 1).ToString();
                BindDataGrid(IDList[currentID].ToString());
                SelectIssueData(IDList[currentID].ToString());
                FillInForm();
            }
        }

        /*Name: Michael Figueroa
        Function Name: ForwardArrow_Click
        Purpose: Event Handler that allows user to scroll through the issues (in a forwards manner) in the report the EditRecord form was accessed from
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string current, int currentID
        Algorithm: currentID is the index of the List<int> IDLIst array; if the currentID value subtracted by 1 is greater or equal to zero, then the currentID is decremented by 1,
        The History DataGrid and Form are all re-binded with the ID of the previous issue in the report; else, nothing happens, in order to avoid and indexOutofBounds exception in
        IDList array.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ForwardArrow_Click(object sender, RoutedEventArgs e)
        {
           // SubmitIssue();

            string current = CurrentIssue.Text.ToString();
            int currentID = Int32.Parse(current) - 1;

            if ((currentID + 1) < (IDList.Count))
            {
                currentID++;

                CurrentIssue.Text = (currentID + 1).ToString();
                BindDataGrid(IDList[currentID].ToString());
                SelectIssueData(IDList[currentID].ToString());
                
                FillInForm();
            }
        }

        /*Name: Michael Figueroa
        Function Name: CurrentIssue_KeyDown
        Purpose: Event Handler that allows user to use enter button to jump issues on the report (so user can go from the first issue to the ninth issue, for instance)
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string current, int currentID
        Algorithm: currentID is the index of the List<int> IDLIst array; if the currentID value is less than the length of the IDList List, then the re-binds using the informaton with
        issue ID of IDList[currentID]
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CurrentIssue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string current = CurrentIssue.Text.ToString();
                int currentID = Int32.Parse(current) - 1;
                if (currentID < IDList.Count)
                {
                    CurrentIssue.Text = (currentID + 1).ToString();
                    BindDataGrid(IDList[currentID].ToString());
                    SelectIssueData(IDList[currentID].ToString());
                    FillInForm();
                }
                else
                {
                    MessageBox.Show("There Are Only " + GetTotalNumIssues() + " Issues in This Report, Please Enter A Valid Value");
                }
            }
        }

        /*Name: Brandon Cox
          Function Name: UpdtBtn_Click
          Purpose: Event Handler that saves updates the user made to the current issue
          Parameters: 
          Return Value: None
          Local Variables: 
          Algorithm: calls SubmitIssue()
             Version: 2.0.0.4
            Date modified: Prior to 1/1/20
            Assistance Received: N/A
          */

        private void UpdtBtn_Click(object sender, RoutedEventArgs e)
        {
            SubmitIssue();
        }
    }
}