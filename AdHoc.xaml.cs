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
    /// Interaction logic for AdHoc.xaml
    /// </summary>
    public partial class AdHoc : Page
    {
        //local variable to store the row of data in the from a specific row in the Report DataGrid
        //ConnectionString comes from App.config
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private DataRowView reportRow;       
        private string customQuery;
        private string[] arr;

        public AdHoc(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;

        }

        //This is where the query is generated
        //First, we determine where the query starts
        //then, we append the appropriate conditions to the query based on what criteria are checked off (we add sys_impact to the where clause if systems are checked, same with 
        //Category and etc.

        private string ConstructCustomQuery()
        {
            string query = "SELECT ID, Priority_Number, Sys_Impact, [Status], Title, Supporting_Details, Internal_Notes, Mgr_Notes, Assigned_To FROM New_Issues WHERE " + DetermineQueryStart();

            //the assigned_to textbox text is Parsed here, allowing the manager to filter by more than one user

            string[] assigned = ParseString();

            StringBuilder sb = new StringBuilder(query);

            //if assigned_to is more than one person, then we append OR keyword in the query in order to keep syntax correct
            if (Assigned_ToCheckBox.IsChecked == true)
            {
                for(int i = 0; i < assigned.Length; i++)
                {
                    if(i != 0)
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

            //Get list of all categories that are checked, and then add to the query based on that
            if (CategoryCheckBox.IsChecked == true)
            {
                List<string> categoryFilters = CategoryQuery();

                for (int i = 0; i < categoryFilters.Count; i++)
                {
                    if((DetermineQueryStart() != "(Category = ") && (i == 0))
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


        //Table is filled using the data from the customQuery
        private void FillReportTable(DataTable table)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(ConstructCustomQuery(), con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
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

        //Determine where the query should start after the WHERE keyword
        private string DetermineQueryStart()
        {
            if(Assigned_ToCheckBox.IsChecked == true)
            {
                return "(Assigned_To = ";
            }
            else if(StatusCheckBox.IsChecked == true)
            {
                return "";
            }
            else if(CategoryCheckBox.IsChecked == true)
            {
                return "(Category = ";
            }
            else if(SystemCheckBox.IsChecked == true)
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

        //Sets Visibility of the checkboxes that are under assigned_to, status, category, and system 
        private void SetVisibility(CheckBox checkBox, StackPanel stack)
        {
            if(checkBox.IsChecked == true)
            {
                stack.Visibility = Visibility.Visible;
            }
            else
            {
                stack.Visibility = Visibility.Collapsed;

            }
        }

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

        //Assigned_To textbox is parsed into an array of strings in order to be able to generate a report of more than one person
        private string[] ParseString()
        {
            string assignedToString = Assigned_To_Text.Text.ToString();
            char[] delimiter = new char[] { '/', ';', ',', ' '};
            string[] assignedTo = assignedToString.Split(delimiter);
            return assignedTo;
        }

       
        private List<string> StatusFilters()
        {
            List<string> statusesChosen = new List<string>();
            statusesChosen.Add("Implemented");
            statusesChosen.Add("Dropped");
            statusesChosen.Add("Closed");
            statusesChosen.Add("Deferred");
            return statusesChosen;
        }

        //Both system and category filter methods check the system checkboxes and add whichever ones are checked to the report
        //System has two stackpanels, thus we have to check both SystemStack and SystemStackTwo to see which systems shall be included in the report

        private List<string> SystemFilter()
        {
            List<string> systemFilters = new List<string>();
            var children = LogicalTreeHelper.GetChildren(SystemsStack);

            foreach (var item in children)
            {
                var checkBox = item as CheckBox;
                if(checkBox.IsChecked == true)
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

        //If the user would like to generate a report with closed or open, then this would be done here
        //statuses list is a list of implemented, dropped, deferred, closed
        //If assigned_to is also checked, then we need to add the AND keyword before beginning the query to avoid sql error
        private string StatusQuery()
        {
            List<string> statuses;
            statuses = StatusFilters();
            if(OpenedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == true)
            {
                return "AND (New_Issues.[Status] != '" + statuses[0] + "' AND New_Issues.[Status] != '" + statuses[1] + "' AND New_Issues.[Status] != '" + statuses[2] + "' AND New_Issues.[Status] != '" + statuses[3] + "') ";
            }

            else if(ClosedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == true)
            {
                return "AND (New_Issues.[Status] = '" + statuses[0] + "' OR New_Issues.[Status] = '" + statuses[1] + "' OR New_Issues.[Status] = '" + statuses[2] + "' OR New_Issues.[Status] = '" + statuses[3] + "') ";
            }

            else if (OpenedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == false)
            {
                return " (New_Issues.[Status] != '" + statuses[0] + "' AND New_Issues.[Status] != '" + statuses[1] + "' AND New_Issues.[Status] != '" + statuses[2] + "' AND New_Issues.[Status] != '" + statuses[3] + "') "; 
            }
            else if(ClosedCheckBox.IsChecked == true && Assigned_ToCheckBox.IsChecked == false)
            {
                return " (New_Issues.[Status] = '" + statuses[0] + "' OR New_Issues.[Status] = '" + statuses[1] + "' OR New_Issues.[Status] = '" + statuses[2] + "' OR New_Issues.[Status] = '" + statuses[3] + "') ";
            }
            else
            {
                return null;
            }
        }

        //Sets checkbox value = false if the user accidently leaves it checked AND the user has not selected an option below it i.e system or category
        private void SetCheckboxes()
        {
            if(SystemFilter().Count == 0)
            {
                SystemCheckBox.IsChecked = false;
            }
            if(CategoryQuery().Count == 0)
            {
                CategoryCheckBox.IsChecked = false;
            }
            if(String.IsNullOrWhiteSpace(Assigned_To_Text.ToString()))
            {
                Assigned_ToCheckBox.IsChecked = false;
            }
            if(StatusQuery() == null)
            {
                StatusCheckBox.IsChecked = false;
            }
        }

        //Button collapses the report wizard, and brings up the report that the user wanted generated
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.Visibility = Visibility.Collapsed;
            DataTable report = new DataTable();
            SetCheckboxes();
            FillReportTable(report);
            Back.Visibility = Visibility.Visible;
            GenerateReport.Visibility = Visibility.Collapsed;
        }

        //This leads to edit record, allows user to scroll through each status in the report using arrows
        private void EditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                reportRow = (DataRowView)((Button)e.Source).DataContext;
                List<int> IDArray = Helper.FillIDList(customQuery);

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, reportRow, IDArray);
                editRecord.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //Rest of these methods set visibility of the checkboxes under Assigned_To, Status, Category, and System
        private void StatusCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(StatusCheckBox, StatusCheckBoxes);
        }

        private void Assigned_ToCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(Assigned_ToCheckBox, Assigned_To_Text);
        }

        private void CategoryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(CategoryCheckBox, CategoryStack);
        }

        private void SystemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetVisibility(SystemCheckBox, SystemsStack);
            SetVisibility(SystemCheckBox, SystemStackTwo);
        }
        //Manager clicks this to go back to the report wizard
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.Visibility = Visibility.Visible;
            Report.Visibility = Visibility.Collapsed;
            Back.Visibility = Visibility.Collapsed;
            GenerateReport.Visibility = Visibility.Visible;
        }

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

        private void UpdateNeeded_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ManagerReview_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}