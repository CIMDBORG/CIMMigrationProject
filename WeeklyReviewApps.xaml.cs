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
    /// <summary>
    /// Interaction logic for WeeklyReviewApps.xaml
    /// </summary>
    public partial class WeeklyReviewApps : Window
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        public static DataTable firstIssue;
        private string[] arr;                           //holds login-based user data
        public static string[] systems;
        private string[] issue_data;                    //Holds the data about the issue, that will be used to populate the form when it loads
        private List<int> IDList;
        private DataRowView priorBySystemRow;           //holds data sent here by row that was clicked 
        private string sys;
        private bool include_300s;

        //This is a weekly review w/apps constructor for users that chose to do weekly review w/apps without choosing a system
        public WeeklyReviewApps(string[] user_data, bool include_pri_ovr_300, List<int> IDListOriginal)
        {
            InitializeComponent();
            systems = Helper.UsersSystems(user_data[7]);
            include_300s = include_pri_ovr_300;
            firstIssue = FillWeeklyRow(systems, include_300s);
            arr = user_data;
            IDList = IDListOriginal;
            priorBySystemRow = WeeklyReviewApps.FillWeeklyRow(systems, include_300s).DefaultView[0];
            SetTotalIssuesText();
            SetInitialIDTextBox();
            SelectIssueData(GetIssueID());
            FillInForm();
            BindDataGrid(GetIssueID());
            Helper.FillSystemComboBoxNoAll(SystemComboBox);
            SystemComboBox.Items.Add("Test");
            FillStatusComboBox();
            FillCategoryComboBox();
            FillImpactComboBox();
            SetMgrNotesRights();
            Updated.Visibility = Visibility.Collapsed;
        }

        //weekly review w/apps with a system chosen
        public WeeklyReviewApps(string[] user_data, string system, List<int> IDListOriginal, bool include_300_pri)
        {
            InitializeComponent();
            arr = user_data;
            IDList = IDListOriginal;
            sys = system;
            include_300s = include_300_pri;
            priorBySystemRow = WeeklyReviewApps.FillWeeklyRow(system, include_300_pri).DefaultView[0];
            firstIssue = FillWeeklyRow(system, include_300_pri);
            SetTotalIssuesText();
            SetInitialIDTextBox();
            SelectIssueData(GetIssueID());
            FillInForm();
            BindDataGrid(GetIssueID());
            Helper.FillSystemComboBoxNoAll(SystemComboBox);
            SystemComboBox.Items.Add("Test");
            FillStatusComboBox();
            FillImpactComboBox();
            FillCategoryComboBox();
            SetMgrNotesRights();
            Updated.Visibility = Visibility.Collapsed;
        }

        //weekly review for managers (wednesday meeting)
        public WeeklyReviewApps(string[] user_data, DataRowView priorRow, List<int> IDListOriginal)
        {
            InitializeComponent();
            arr = user_data;
            IDList = IDListOriginal;
            priorBySystemRow = priorRow;
            SystemComboBox.Items.Add("Test");
            SetTotalIssuesText();
            SetInitialIDTextBox();
            FillCategoryComboBox();
            FillStatusComboBox();
            FillImpactComboBox();
            SelectIssueData(GetIssueID());
            FillInForm();
            Helper.FillSystemComboBoxNoAll(SystemComboBox);
            BindDataGrid(GetIssueID());
            Updated.Visibility = Visibility.Collapsed;
        }

        //combobox fill methods
        private void FillStatusComboBox()
        {
            StatusComboBox.Items.Add("BC Approved");
            StatusComboBox.Items.Add("Active");
            StatusComboBox.Items.Add("Implemented");
            StatusComboBox.Items.Add("Deferred");
            StatusComboBox.Items.Add("Dropped");
            StatusComboBox.Items.Add("Pending");
            StatusComboBox.Items.Add("BC Submitted");
            StatusComboBox.Items.Add("Closed");
            StatusComboBox.Items.Add("App Review");
        }

        private void FillCategoryComboBox()
        {
            CategoryComboBox.Items.Add("BC/TI");
            CategoryComboBox.Items.Add("Defect");
            CategoryComboBox.Items.Add("HDFS");
            CategoryComboBox.Items.Add("Inquiry");
            CategoryComboBox.Items.Add("Issue");
            CategoryComboBox.Items.Add("Strategic Task");
            CategoryComboBox.Items.Add("Task");
        }
        
        private void FillImpactComboBox()
        {
            ImpacttypeComboBox.Items.Add("Cost Savings");
            ImpacttypeComboBox.Items.Add("Compliance");
            ImpacttypeComboBox.Items.Add("New Revenue");
            ImpacttypeComboBox.Items.Add("Reporting Issue");
            ImpacttypeComboBox.Items.Add("Quality");
            ImpacttypeComboBox.Items.Add("Bad Bill");
            ImpacttypeComboBox.Items.Add("Not Billed Items");
            ImpacttypeComboBox.Items.Add("ISMT");
            ImpacttypeComboBox.Items.Add("Incentive Setup");
            ImpacttypeComboBox.Items.Add("Invoice Display");
            ImpacttypeComboBox.Items.Add("Tech Request");
            ImpacttypeComboBox.Items.Add("Abend/Failure");
            ImpacttypeComboBox.Items.Add("Out of Balance");
            ImpacttypeComboBox.Items.Add("Quoting");
            ImpacttypeComboBox.Items.Add("Rate Fail");
            ImpacttypeComboBox.Items.Add("Other");
        }

        //these are the results of the weekly apps query
        //query builder based on two things: system chosen (if applicable), OR, if no system chosen, then the collection of all of the user's systems is chosen
        public static string GetWeeklyAppsQuery(string[] systems, bool include_300s)
        {
            if (include_300s)
            {
                string stringQuery = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                             "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                             "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                              "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                               "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (ManagerMeeting = 0) AND (Category != 'Strategic Task') AND (Sys_Impact = ";
                StringBuilder sb = new StringBuilder(stringQuery);
                for (int i = 0; i < systems.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("'" + systems[i] + "' ");
                    }
                    else
                    {
                        sb.Append("OR Sys_Impact = '" + systems[i] + "' ");
                    }
                }
                sb.Append(") ORDER BY Sys_Impact ASC, Priority_Number ASC;");

                return sb.ToString();
            }

            else
            {
                string stringQuery = "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                             "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                             "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                              "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                               "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Priority_Number < 300) AND (ManagerMeeting = 0) AND (Category != 'Strategic Task') AND (Sys_Impact = ";
                StringBuilder sb = new StringBuilder(stringQuery);
                for (int i = 0; i < systems.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("'" + systems[i] + "' ");
                    }
                    else
                    {
                        sb.Append("OR Sys_Impact = '" + systems[i] + "' ");
                    }
                }
                sb.Append(") ORDER BY Sys_Impact ASC, Priority_Number ASC;");

                return sb.ToString();
            }
        }

        //gets first issue in the report when no system is chosen; this is used when the form is not opened from a datagrid
        public static string GetFirstWeeklyAppsIssue(string[] systems, bool include_300s)
        {
            if (include_300s)
            {
                string stringQuery = "SELECT TOP 1 ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                             "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                             "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                              "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                               "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (ManagerMeeting = 0) AND (Category != 'Strategic Task') AND (Sys_Impact = ";
                StringBuilder sb = new StringBuilder(stringQuery);
                for (int i = 0; i < systems.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("'" + systems[i] + "' ");
                    }
                    else
                    {
                        sb.Append("OR Sys_Impact = '" + systems[i] + "' ");
                    }
                }
                sb.Append(") ORDER BY Sys_Impact ASC, Priority_Number ASC;");
                return sb.ToString();
            }

            else
            {
                string stringQuery = "SELECT TOP 1 ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                            "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                            "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                             "FROM New_Issues WHERE (Priority_Number < 300) AND (New_Issues.[Status] NOT LIKE '%closed%' " +
                              "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                               "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Priority_Number < 300) AND (ManagerMeeting = 0) AND (Category != 'Strategic Task') AND (Sys_Impact = ";
                StringBuilder sb = new StringBuilder(stringQuery);
                for (int i = 0; i < systems.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("'" + systems[i] + "' ");
                    }
                    else
                    {
                        sb.Append("OR Sys_Impact = '" + systems[i] + "' ");
                    }
                }
                sb.Append(") ORDER BY Sys_Impact ASC, Priority_Number ASC;");
                return sb.ToString();
            }
        }

        //gets first issue in the report when system is chosen; this is used when the form is not opened from a datagrid
        public static string GetFirstWeeklyAppsIssue(string system, bool include_300s)
        {
            if (include_300s)
            {
                return "SELECT TOP 1 ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                             "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                             "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                              "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                               "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'Strategic Task') AND (ManagerMeeting = 0) AND (Sys_Impact = '" + system + "') ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }
            else
            {
                return "SELECT TOP 1 ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                             "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                             "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                              "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                               "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'Strategic Task') AND (ManagerMeeting = 0) AND (Sys_Impact = '" + system + "') AND (Priority_Number < 300) ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }
        }

        //Weekly apps query when single system is chosen
        public static string GetWeeklyAppsQuery(string system, bool include_300s)
        {
            if (include_300s)
            {
                return "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                              "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                              "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                               "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                                "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                 "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'Strategic Task') AND (ManagerMeeting = 0) AND (Sys_Impact = '" + system + "') ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }

            else
            {
                return "SELECT ID, Sys_Impact, Priority_Number, Assigned_To AS [Owner], [Status], Category, Title, Supporting_Details AS Details, Bus_Impact, Internal_Notes, TFS_BC_HDFS_Num AS BID#, Impact, " +
                              "AnnualBenefit, OneTimeBenefit, Req_Dept AS ReqDept, Req_Name AS RequestedBy, Opened_Date AS InquiryDate, Due_Date AS PlannedDate, " +
                              "Completed_Date, DATEDIFF(day, Opened_Date, Completed_Date) AS #Days " +
                               "FROM New_Issues WHERE (New_Issues.[Status] NOT LIKE '%closed%' " +
                                "AND New_Issues.[Status] NOT LIKE '%implemented%' " +
                                 "AND New_Issues.[Status] NOT LIKE '%dropped%' AND New_Issues.[Status] NOT LIKE '%deferred%') AND (Category != 'Strategic Task') AND (ManagerMeeting = 0) AND (Priority_Number < 300) AND (Sys_Impact = '" + system + "') ORDER BY Sys_Impact ASC, Priority_Number ASC;";
            }
        }              

        public static DataTable FillWeeklyRow(string[] systems, bool include_300s)
        {
            string firstWeekly = GetFirstWeeklyAppsIssue(systems, include_300s);
            DataTable weeklyRow = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand recentCmd = new SqlCommand(firstWeekly, con);
                using (SqlDataAdapter sda = new SqlDataAdapter(recentCmd))
                {
                    sda.Fill(weeklyRow);
                }
            }
            return weeklyRow;
        }

        public static DataTable FillWeeklyRow(string system, bool include_300s)
        {
            string firstWeekly = GetFirstWeeklyAppsIssue(system, include_300s);
            DataTable weeklyRow = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand recentCmd = new SqlCommand(firstWeekly, con);
                using (SqlDataAdapter sda = new SqlDataAdapter(recentCmd))
                {
                    sda.Fill(weeklyRow);
                }
            }
            return weeklyRow;
        }

        public void BindDataGrid(string TaskNum)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    string query = "select History.ID, format(EntryDate, 'MM/dd/yyyy') as EntryDate, New_StatusNote as [Status], [Status] as Status_Note " +
                                   "from History where TaskNum = " + TaskNum + " AND New_StatusNote != 'Aging' order by History.EntryDate desc;";

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

        //this sets the index of the initial id textbox (displayed as "Issue X of _")
        private void SetInitialIDTextBox()
        {
            int issueID = Int32.Parse(GetIssueID());

            CurrentIssue.Text = (IDList.IndexOf(issueID) + 1).ToString();
        }



        public static DataRow FirstRow()
        {
            return firstIssue.Rows[0];
        }

        //this gets the initial issue id that is clicked on in the report grid 
        private string GetIssueID()
        {

            return priorBySystemRow["ID"].ToString();
        }


        private void SetTotalIssuesText()
        {
            TotalIssues.Text = "Of " + GetTotalNumIssues().ToString();
        }

        private void FillInForm()
        {
            TitleText.Text = issue_data[0].ToString();
            DateTime myStartDate;
            DateTime myDueDate;

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

            StatusComboBox.SelectedItem = issue_data[5].ToString();
            CategoryComboBox.SelectedItem = issue_data[6].ToString();
            BCTItext1.Text = issue_data[7].ToString();
            SystemComboBox.SelectedItem = issue_data[8].ToString();
            PriorityText.Text = issue_data[9].ToString();
            SupportingDetailsText.Text = issue_data[10].ToString();

            if (issue_data[13].ToString() == "True")
            {
                HotTopicCheckBox.IsChecked = true;
            }

            else
            {
                HotTopicCheckBox.IsChecked = false;
            }

            ManagerNotesText.Text = issue_data[14].ToString();           
            ImpacttypeComboBox.SelectedItem = issue_data[17].ToString();

            
            BusinessImpactsText.Text = issue_data[19].ToString();
          
            if (issue_data[22].ToString() == "True")
            {
                ManagerReviewCheckBox.IsChecked = true;
            }
            else
            {
                ManagerReviewCheckBox.IsChecked = false;
            }

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

            //if the user_update_bit is false, then the checkbox will be checked because no update has been made yet by the user
            if (issue_data[24].ToString() == "False" && issue_data[26].ToString() == "True")
            {
                UpdateRequiredCheckBox.IsChecked = true;
            }

            else
            {
                UpdateRequiredCheckBox.IsChecked = false;
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
        }

        private void SelectIssueData(string ID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    string query = "SELECT Title, Req_Dept, Req_Name, Opened_Date, Due_Date, [Status], Category, TFS_BC_HDFS_Num, Sys_Impact as [System], Priority_Number, " +
                                       "Supporting_Details, Internal_Notes, BC_Approved, Hot_Topic, ISNULL(Mgr_Notes, '') as Mgr_Notes, [Control], Proc_Imp, Impact, Cim_Val, Bus_Impact, " +
                                       "OneTimeBenefit, AnnualBenefit, ManagerReview, WeeklyReview, User_Update_Bit, Assigned_To, Manager_Update_Bit, Completed_Date, Hours, Annual_Cost_Savings, " +
                                       "Benefits, HP, Deliverables, BCSub, ManagerMeeting, CIM_Know FROM New_Issues WHERE ID=" + ID + ";";

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

        private void SetMgrNotesRights()
        {
            if(arr[6] == "User")
            {
                ManagerNotesText.IsReadOnly = true;
                ManagerReviewCheckBox.IsEnabled = false;
                UpdateRequiredCheckBox.IsEnabled = false;
                managerMeetingCheckBox.IsEnabled = false;
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
                Updated.Visibility = Visibility.Collapsed; //Mike Fig fix for update successful notification
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

        //this gets the total number of issues that the report contains; this is used for display purposes 
        private int GetTotalNumIssues()
        {
            return IDList.Count;
        }

        //event handler for forward arrow
        private void ForwardArrow_Click(object sender, RoutedEventArgs e)
        {
            //Submit();
            
            string current = CurrentIssue.Text.ToString();
            int currentID = Int32.Parse(current) - 1;

            if ((currentID + 1) < (IDList.Count))
            {
                currentID++;
                Updated.Visibility = Visibility.Collapsed; //Mike Fig fix for update successful notification
                CurrentIssue.Text = (currentID + 1).ToString();
                BindDataGrid(IDList[currentID].ToString());
                SelectIssueData(IDList[currentID].ToString());
               
                FillInForm();
            }
        }

        private void Submit()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    //Checks user role and updates appropriate values
                    //Manager can also make a user update as well; can still request an update from user using checkbox

                    string query = UpdateQuery();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                    SelectIssueData(GetCurrentID().ToString());
                    FillInForm();
                    BindDataGrid(GetCurrentID().ToString());
                   /*if (StatusComboBox.SelectedItem.ToString() == "Closed")
                    {
                        UpdatePriorityNums();
                    } */
                   Updated.Visibility = Visibility.Visible;
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

        private string UpdateQuery()
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
            return "UPDATE New_Issues SET [Status] = '" + StatusComboBox.SelectedItem.ToString() + "', Impact = '" + ImpacttypeComboBox.SelectedItem.ToString() + "', Sys_Impact = '" + SystemComboBox.SelectedItem.ToString() + "', Mgr_Notes = '" + ManagerNotesText.Text.ToString().Replace("'", "\''") +
                "', Supporting_Details = '" + SupportingDetailsText.Text.ToString().Replace("'", "\''") + "', Bus_Impact = '" + BusinessImpactsText.Text.ToString().Replace("'", "\''") + "', Title = '" + TitleText.Text.ToString().Replace("'", "\''") + "', " +
                "Priority_Number = '" + PriorityText.Text.ToString() + "', TFS_BC_HDFS_Num = '" + BCTItext1.Text.ToString() + "', Category = '" + CategoryComboBox.SelectedItem.ToString() + "', Opened_Date = '" + Startdatepicker.SelectedDate.ToString() + "', Due_Date = " + plannedDate + ", " +
                " Hot_Topic='" + HotTopicCheckBox.IsChecked.ToString() + "', [Control] = '" + ControlEnhancementCheckBox.IsChecked.ToString() + "', " + "Proc_Imp='" + ProcessImprovementCheckBox.IsChecked.ToString() +
                "', Cim_Val = '" + CIMValueAddedCheckBox.IsChecked.ToString() + "', ManagerMeeting= '" + managerMeetingCheckBox.IsChecked.ToString() + "', CIM_Know = '" + CIMKnowCheckBox.IsChecked.ToString() + "'" +
                ", ManagerReview = '" + ManagerReviewCheckBox.IsChecked.ToString() + "' WHERE ID = " + GetCurrentID() + ";";
        }                                                   
                                                            
        private void UpdatePriorityNums()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    int priorty_num = 101;

                    for (int i = 0; i < IDList.Count; i++)
                    {
                        if (i != GetCurrentIndex())
                        {
                            string updated = "UPDATE New_Issues SET Priority_Number = '" + priorty_num + "' WHERE ID = " + IDList[i] + ";";
                            SqlCommand cmd = new SqlCommand(updated, connection);
                            cmd.ExecuteNonQuery();
                            priorty_num++;
                        }
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

        //Gets current index in List
        private int GetCurrentID()
        {
            int current = Int32.Parse(CurrentIssue.Text.ToString()) - 1;
            return IDList[current];
        }       

        private int GetCurrentIndex()
        {
            return Int32.Parse(CurrentIssue.Text.ToString()) - 1;
        }

        private void AddStatus_Click(object sender, RoutedEventArgs e)
        {
            EditRecord_AddEditStatus addStatus = new EditRecord_AddEditStatus(this, GetCurrentID());
            addStatus.Show();
        }

        private void UpdatBtn_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }
    }
}