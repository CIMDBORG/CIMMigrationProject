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

        //These constructors are specific to what kind of forms they are opened from
        //Constructor one and constructor 2 are opened from a regular datagrid, and the form begins on the issue that was clicked on the datagrid  
        //Constructors 3 and 4 are opened from Weekly Review with Apps; the difference between them is that 3 is opened with Weekly Review with Apps without a system filter, while 4 consist of one singular system

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

        public EditRecord(string[] user_data, DataRowView priorRow, List<int> IDListOriginal)
        {
            InitializeComponent();
            page = new Page();
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

        //this gets the initial issue id that is clicked on in the report grid 
        private string GetIssueID()
        {

            return priorBySystemRow["ID"].ToString();
        }

        //this gets the total number of issues that the report contains; this is used for display purposes 
        private int GetTotalNumIssues()
        {
            return IDList.Count;
        }

        //this sets the total number of issues in the edit record form arrow selection menu; displays the total number of issues on report in edit view (so issue "_ of X")
        private void SetTotalIssuesText()
        {
            TotalIssues.Text = "Of " + GetTotalNumIssues().ToString();
        }


        //this sets the index of the initial id textbox (displayed as "Issue X of _")
        private void SetInitialIDTextBox()
        {
            int issueID = Int32.Parse(GetIssueID());

            CurrentIssue.Text = (IDList.IndexOf(issueID) + 1).ToString();
        }

        //Gets current index in List
        private int GetCurrentID()
        {
            int current = Int32.Parse(CurrentIssue.Text.ToString()) - 1;
            return IDList[current];
        }

        //Runs when the window is loaded. Prepares comboboxes and checks user's role to set content visibilities
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



        // Changes content of Impact type combobox based on user's selection of Category.
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Sets visibility of deliverables textbox based on whether or not strategic task is chosen from the category combobox

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

            //Sets combobox options based on the category chosen; only managers are able to edit record statuses to BC Approved or BC Submitted in a Business Case issue

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

        //validates user input in these fields, as they must be ints and not strings
        //Used for error checking in int fields such as priority number and BC/TI number

        private bool IsInt()
        {
            string priority_Number = PriorityText.Text.ToString();
            string bcTi = BCTItext1.Text.ToString();
            string oneTime = OneTimeBenefitText.Text.ToString();
            string annualBenefit = AnnualBenefitText.Text.ToString();

            string[] intArray = new String[] { priority_Number, bcTi, oneTime, annualBenefit };

            bool success = true;
            int number = 0;
            for (int i = 0; i < intArray.Length; i++)
            {
                if (!(Int32.TryParse(intArray[i], out number)))
                {
                    success = false;
                }
            }

            if (success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //This is the query that is run when a user edits an issue
        //Also where empty values on form are handled if needed
        //PlannedDate set to NULL to avoid dates being set to 1/1/1900 if empty
        //Query is changed based on whether the issue is a strategic task or not
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

        //this returns today's date; this is used to append the date to the date string, seperated by a delimiter (;) in sqlserver userUpdate or managerUpdate columns
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

        //Query that runs for a manager update
        //Manager can request a user update by using the checkbox
        //Text is written to the original supp detail, original title, and original bus impacts columns in order to force the user to make a change for it to count as an update
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

            //this sets userUpdateBit to 0, which is meant to signify that the user has not responded to the manager update request yet
            if (UpdateRequiredCheckBox.IsChecked.ToString() == "True")
            {
                setUserUpdateBit = "False";
            }
            //else, it is true that the user has responded to an update
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


        //this controls whether the user has made a significant enough change to allow an update to be made
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

        //tells us whether or not the user logged in is a manager or not.
        private bool IsManager()
        {
            if (arr[6] == "Manager" || arr[6] == "Program Manager")
            {
                return true;
            }
            return false;
        }

        //Event handler for submit button
        //Checks if user is manager/not Paul. If so, then the manager update query is run, if not, user update query is

        private void SubmitIssueButton_Click(object sender, RoutedEventArgs e)
        {
            //Issue ID 
            // Then, we set all the date fields to handle all values, including nulls
            string ID = GetCurrentID().ToString();
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    //Checks user role and updates appropriate values
                    //Manager can also make a user update as well; can still request an update from user using checkbox
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



        // Asks user to confirm that they wish to leave the form if they click 'Cancel'
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit form? All information entered will be cleared.", "Cancel Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.Close();
            }
        }



        // Logic that sets an issue's status to Closed or Implemented based on if the user selects a completion date
        private void CompDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompDatePicker.Text.Length != 0)
            {
                StatusComboBox.SelectedIndex = 2;
            }
        }



        // DESCRIPTION: Logic that sets an issue's completion date to the current date if the user changes the status to 'Closed' or 'Implemented'
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //I preset that "Implemented" for BC's and "Closed" for all other categories are both at index 2
            if (StatusComboBox.SelectedIndex == 2)
            {
                CompDatePicker.SelectedDate = DateTime.Today;
            }
            else
            {
                CompDatePicker.SelectedDate = null;
            }
        }



        //*******************************************************************
        // DESCRIPTION: Function that runs the SELECT query in SQL server to pull the necessary issue data to populate Edit form. 
        //              Stores the results of the query in our string[] class variable issue_data
        //*******************************************************************
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



        // Populates the fields of the Edit form using the string[] issue_data. Includes appropriate parsing/error handling for certain fields.
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

            //if the user_update_bit is false, then the checkbox will be checked because no update has been made yet by the user
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

        // Passes this Window and prioritization by system row to a new AddEditStatus window, then displays it.
        private void AddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            EditRecord_AddEditStatus addStatus = new EditRecord_AddEditStatus(this, GetCurrentID());
            addStatus.Show();
        }



        //*******************************************************************
        // Runs the SELECT query in SQL server to pull the necessary history data for particular issue, specified by TaskNum input.
        //  Fills in the DataGrid on this window with the results of this query.
        //  Displays EntryDate, Status, and StatusNote.
        //New_Issues Table
        // INPUT:       string TaskNum : accepts the TaskNumber for the SELECT query. TaskNum in the History table is equivalent to ID in .
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


        //*******************************************************************
        // DESCRIPTION: Runs when a row of Report datagrid is double-clicked. This pulls the data from that row and opens an AddEditRecord window,
        //                  passing that data along in the constructor so it can auto-populate upon loading.
        //              Also passes this window itself to that form, so that this EditRecord Window can update once the status is edited,
        //                  as well as PBS DataRowView.
        //*******************************************************************
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

        //event handler for back arrow
        //subtract one for the current issue id text, subtract current index from list
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


        //event handler for forward arrow
        private void ForwardArrow_Click(object sender, RoutedEventArgs e)
        {
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

        //allows user to jump to an issue by typing in a number
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

    }
}