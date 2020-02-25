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
    /// 
    /*PLS NOTE: WeeklyReviewApps form has two fold use: it is used for when the users have their weekly review with apps as the name suggests, and it is also used for the weekly meeting (WeeklyReview form)
      with supervisors and managers
     */ 
    public partial class WeeklyReviewApps : Window
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//ConnectionString comes from App.config
        public static DataTable firstIssue; //DataTable storing the first issue in WeeklyReviewApps
        private string[] arr;                           //holds login-based user data
        public static string[] systems; //used to pull the report using all user's systems
        private string[] issue_data;                    //Holds the data about the issue, that will be used to populate the form when it loads
        private List<int> IDList; //ID List containing every ID number from each item in DataRowView priorRow
        private DataRowView priorBySystemRow;           //holds data sent here by row that was clicked 
        private string sys; //stores system user would like to use in report
        private bool include_300s; //bool that determines whether or not priority numbers over 300 are included

        /*Name: Michael Figueroa
        Function Name: WeeklyReviewApps
        Purpose: This is a weekly review w/apps constructor for users that 
        chose to do weekly review w/apps without choosing a system
        Parameters: string[] user_data, bool include_pri_ovr_300, List<int> IDListOriginal
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
        /*Name: Michael Figueroa
        Function Name: WeeklyReviewApps
        Purpose: This is a weekly review w/apps constructor with a system chosen
        Parameters: string[] user_data, bool include_pri_ovr_300, List<int> IDListOriginal, bool include_300_pri
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: WeeklyReviewApps
        Purpose: This is a weekly review w/apps constructor specifically for when this form is accessed through
        edit button click event on WeeklyReview.xaml.cs
        Parameters: string[] user_data, DataRowView priorRow, List<int> IDListOriginal
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBox
        Purpose: Fills Status combobox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 - May no longer be needed, Mike may want to get rid of it idk 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillCategoryComboBox
        Purpose: Fills Category combobox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 - May no longer be needed, Mike may want to get rid of it idk 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: FillImpactComboBox
        Purpose: Fills Impact combobox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None 
        Date modified: Prior to 1/1/20 - May no longer be needed, Mike may want to get rid of it idk 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: GetWeeklyAppsQuery
        Purpose: GetWeeklyAppsQuery when no is clicked upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click
        Parameters: string[] systems, bool include_300s
        Return Value: string
        Local Variables: stringQuery, stringbuilder sb
        Algorithm: if include_300s is true, then there is no condition in WHERE clause for priority_number; sb appends the values from
        string[] systems onto stringQuery, and then sb.ToString() is returned; else, there is condition in WHERE clause that excludes priority_numbers over 300 from the results, sb appends string[] systems
        values onto stringQuery, and sb.ToString() is returned.
        then the collection of all of the user's systems
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        //Weekly apps query when single system is chosen
        /*Name: Michael Figueroa
        Function Name: GetWeeklyAppsQuery
        Purpose: GetWeeklyAppsQuery when single system is chosen when Yes is clicked upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click
        Parameters: string system, bool include_300s
        Return Value: string
        Local Variables: stringQuery, stringbuilder sb
        Algorithm: if include_300s is true, then there is no condition in WHERE clause for priority_number; sb appends the values from; else, Priority_Numbers above 300 are excluded; Sys_Impact is set
        to string system
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: GetFirstWeeklyAppsIssue
        Purpose: GetFirstWeeklyAppsIssue retrieves the first issue when no is clicked upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click
        Parameters: string[] systems, bool include_300s
        Return Value: string
        Local Variables: stringQuery, stringbuilder sb
        Algorithm: if include_300s is true, then there is no condition in WHERE clause for priority_number; sb appends the values from
        string[] systems onto stringQuery, and then sb.ToString() is returned; else, there is condition in WHERE clause that excludes priority_numbers over 300 from the results, 
        sb appends string[] systems
        values onto stringQuery, and sb.ToString() is returned.
        then the collection of all of the user's systems
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: GetFirstWeeklyAppsIssue 
        Purpose: GetFirstWeeklyAppsIssue retrieves the first issue when YES is clicked upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click - note that this method has been overloaded
        Parameters: string system, bool include_300s
        Return Value: string
        Local Variables: None
        Algorithm: if include_300s is true, then there is no condition in WHERE clause for priority_number; else, there is condition in WHERE clause that excludes priority_numbers over 300 from the results
        Sys_impact in WHERE clause is set equal to string system
        then the collection of all of the user's systems
        Date modified: Prior to 1/1/20 
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
       Function Name: FillWeeklyRow 
       Purpose: FillWeeklyRow returns a DataTable containing the first WeeklyReviewApps item from GetFirstWeeklyAppsIssue - this method in particular is used
       when the user does not specify a system to filter by upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click - note that this method has been overloaded
       Parameters: string[] systems, bool include_300s
       Return Value: DataTable weeklyRow
       Local Variables: string firstWeekly, DataTable weeklyRow
       Algorithm: firsyWeekly is assigned by calling GetFirstWeeklyAppsIssue, then weeklyRow is filled through Sql Command constructed using firstWeekly, then
       weeklyRow is returned.
       Date modified: Prior to 1/1/20 
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
       Function Name: FillWeeklyRow 
       Purpose: FillWeeklyRow returns a DataTable containing the first WeeklyReviewApps item from GetFristWeeklyAppsIssue - this method in particular is used
       when the user does specify a system to filter by upon WeeklyReviewApps button click on the menu screen (MessageBoxResult messageBoxResult in UserMenu_Window 
        WeeklyReviewApps_Click - note that this method has been overloaded
       Parameters: string[] systems, bool include_300s
       Return Value: DataTable weeklyRow
       Local Variables: string firstWeekly, DataTable weeklyRow
       Algorithm: firsyWeekly is assigned by calling GetFirstWeeklyAppsIssue, then weeklyRow is filled through Sql Command constructed using firstWeekly, then
       weeklyRow is returned.
       Date modified: Prior to 1/1/20 
       Assistance Received: N/A
       */
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

        /*Name: Michael Figueroa
       Function Name: SetInitialIDTextBox
       Purpose: Setter Method - sets CurrentIssue.Text
       Parameters: None
       Return Value: N/A
       Local Variables: None
       Algorithm: int parses GetIssueID value, then returns the value of that + 1
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
       Function Name: FirstRow
       Purpose: Getter method; returns first value in firstIssues array
       Parameters: None
       Return Value: DataRow
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        public static DataRow FirstRow()
        {
            return firstIssue.Rows[0];
        }

        /*Name: Michael Figueroa
       Function Name: GetIssueID
       Purpose: this gets the initial issue id that is clicked on in the report grid
       Parameters: None
       Return Value: string
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

        /*Name: Michael Figueroa
        Function Name: SetMgrNotesRights
        Purpose: Sets who can edit managerNotesText, ManagerReview, UpdateRequired, and managerMeeting checkboxes
        Parameters: None
        Return Value: None
        Local Variables: none
        Algorithm: if arr[6] is equal to "User", then the user cannot edit Manager notes, manager review update required and manager meeting checkboxes.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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
                Updated.Visibility = Visibility.Collapsed; //Mike Fig fix for update successful notification
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

        /*Name: Michael Figueroa
        Function Name: Submit
        Purpose: This executes the update query. 
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Opens SqlConnection, then executes string query, calls SelectIssueData, FillInForm, and BindDataGrid. Updates Updated to visible in order to let
        user know the issue has been updated
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Michael Figueroa
        Function Name: UpdateQuery
        Purpose: Returns string that will be used in method Submit()
        Parameters: None
        Return Value: string
        Local Variables: string plannedDate
        Algorithm: is PlannedDate has not been chosen by user, then plannedDate = null, else, it is equal to ToString value.
        Returns values user has chosen as an UPDATE query.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
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

        /*Name: Brandon Cox
        Function Name: UpdateQuery
        Purpose: Updates priority numbers after user has closed current issue
        Parameters: None
        Return Value: None
        Local Variables: int priority_num
        Algorithm: for loop: if int i is not equal to GetCurrentIndex(), then the priority_number is updated, and priority_num is incremented by 1; 
        else, nothing happens. - this method needs to be finished
        Version: 2.0.0.4
        Date modified: February 2020
        Assistance Received: Michael Figueroa suggests the following:
        the current if statement is fine; however, I suggest we make a int array full of the possible priority numbers and update through that; 
        this array would hold values from 101-109,201-209, 301-309, and 401-409. This way, you keep from going into 110, 111, and etc.
        int[] priority_num_values = {101,102,103,104,105,106,107,108,109,201,202, etc. <pseudocode>} 
                    for (int i = 0; i < IDList.Count; i++)
                    {
                        if (i != GetCurrentIndex())
                        {
                            string updated = "UPDATE New_Issues SET Priority_Number = '" + priority_num_values[i] + "' WHERE ID = " + IDList[i] + ";";
                            SqlCommand cmd = new SqlCommand(updated, connection);
                            cmd.ExecuteNonQuery();
                            priorty_num++;
                        }
                    }
        I think this will work and the method will execute faster without extra condition statements
        */
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
        Function Name: GetCurrentIndex()
        Purpose: Getter method that retrieves the Index from IDList array of the current issue being displayed on screen
        Parameters: None
        Return Value: The current index of array IDList of the current issue being displayed on screen
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private int GetCurrentIndex()
        {
            return Int32.Parse(CurrentIssue.Text.ToString()) - 1;
        }

        /*Name: Michael Figueroa
        Function Name: AddStatus_Click
        Purpose: Event handler for Add Status button
        Parameters: Auto Generated
        Return Value: None
        Local Variables: None
        Algorithm: If the Add Status button is clicked, then the EditRecord_AddEditStatus form is shown
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void AddStatus_Click(object sender, RoutedEventArgs e)
        {
            EditRecord_AddEditStatus addStatus = new EditRecord_AddEditStatus(this, GetCurrentID());
            addStatus.Show();
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
        private void UpdatBtn_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }
    }
}