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
using WpfApp2;

namespace WpfApp1
{
    //*******************************************************************
    // DESCRIPTION: 	This window is for the user to add or edit a status for an open item. This form is navigated to directly from the EditRecord form,
    //                      by either clicking the "Add a status" button or by double-clicking the row of an existing status (to edit it).
    //                  Each of these options has a different AddEditStatus constructor and runs different code.
    //                  Regardless of if the status is new or is being edited, on a successful submit, this window will close, and the updates will be displayed in 
    //                      EditRecord, as we passed the parent Editrecord window itself to this window.
    //*******************************************************************
    public partial class EditRecord_AddEditStatus : Window
    {
        public String connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private DataRowView pbsRow;     //DataRowView variable that stores the row from Prioritization by system
        private DataRowView histRow;    //DataRowView variable that stores the row from the History data that is displayed on the edit form. It is sent here on row double-click.
        private bool isNewStatus;       //Local boolean value that checks if the user is trying to add a new status (true) or edit an existing status (false)
        private EditRecord form;        //Holds the parent EditRecord window currently Open in the applicaiton, which will be updated once the status is added/edited
        private WeeklyReviewApps weeklyReviewForm; 
        private int IDnum;                      //local variable to store issue ID number
        private bool isWeekly; //bool that determines whether this form is being accessed from a WeeklyReviewApps form
        private int currentID;    //DataRowView variable that stores the row from the History data that is displayed on the edit form. It is sent here on row double-click.


        /*Name: Michael Figueroa
        Function Name: EditRecord_AddEditStatus
        Purpose: EditRecord_AddEditStatus Constructor for
        Parameters: EditRecord editRecord, int ID
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public EditRecord_AddEditStatus(EditRecord editRecord, int ID)
        {
            InitializeComponent();
            
            form = editRecord;
            currentID = ID;
            StatusDatePicker.SelectedDate = DateTime.Today;
            isWeekly = false;

            isNewStatus = true;
            
            Fill_HistoryStatusComboBox();
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: EditRecord_AddEditStatus
        Purpose: EditRecord_AddEditStatus Constructor when accessed from WeeklyReviewApps form
        Parameters: WeeklyReviewApps weeklyReview, int ID
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public EditRecord_AddEditStatus(WeeklyReviewApps weeklyReview, int ID)
        {
            InitializeComponent();

            weeklyReviewForm = weeklyReview;
            currentID = ID;
            StatusDatePicker.SelectedDate = DateTime.Today;
            isWeekly = true;

            isNewStatus = true;

            Fill_HistoryStatusComboBox();
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: EditRecord_AddEditStatus
        Purpose: EditRecord_AddEditStatus Constructor when accessed from a double-click on the history datagrid on the EditRecord form
        Parameters: EditRecord editRecord, int ID, DataRowView statusDataRow
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public EditRecord_AddEditStatus(EditRecord editRecord, int ID, DataRowView statusDataRow)
        {
            InitializeComponent();
            
            form = editRecord;
            histRow = statusDataRow;
            currentID = ID;
            isWeekly = false;
            
            isNewStatus = false;
            DeleteIssueButton.Visibility = Visibility.Visible;

            Fill_HistoryStatusComboBox();
            
            HistoryStatusComboBox.SelectedValue = histRow["Status"].ToString();
            StatusNoteText.Text = histRow["Status_Note"].ToString();

            if (!DateTime.TryParse(histRow["EntryDate"].ToString(), out DateTime myDate))
            {
            }
            else
            {
                StatusDatePicker.SelectedDate = myDate;
            }
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: EditRecord_AddEditStatus
        Purpose: EditRecord_AddEditStatus Constructor when accessed from a double-click on the history datagrid on the WeeklyReviewApps form
        Parameters: WeeklyReviewApps weeklyReview, int ID, DataRowView statusDataRow
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public EditRecord_AddEditStatus(WeeklyReviewApps weeklyReview, int ID, DataRowView statusDataRow)
        {
            InitializeComponent();

            weeklyReviewForm = weeklyReview;
            currentID = ID;
            histRow = statusDataRow;
            isWeekly = true;

            isNewStatus = true;
            isNewStatus = false;
            DeleteIssueButton.Visibility = Visibility.Visible;
            Fill_HistoryStatusComboBox();

            HistoryStatusComboBox.SelectedValue = histRow["Status"].ToString();
            StatusNoteText.Text = histRow["Status_Note"].ToString();

            if (!DateTime.TryParse(histRow["EntryDate"].ToString(), out DateTime myDate))
            {
            }
            else
            {
                StatusDatePicker.SelectedDate = myDate;
            }
            Updated.Visibility = Visibility.Collapsed;
        }

        /*Name: Michael Figueroa
        Function Name: Fill_HistoryStatusComboBox
        Purpose: Fills HistoryStatusComboBox
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Fill_HistoryStatusComboBox()
        {
            HistoryStatusComboBox.Items.Add("Item Not Assigned");
            HistoryStatusComboBox.Items.Add("Analysis in Progress");
            HistoryStatusComboBox.Items.Add("Coding in Progress");
            HistoryStatusComboBox.Items.Add("Testing in Progress");
            HistoryStatusComboBox.Items.Add("Pending Verification");
            HistoryStatusComboBox.Items.Add("Scheduled Implementation");
            HistoryStatusComboBox.Items.Add("Work Delayed");
            HistoryStatusComboBox.Items.Add("Waiting on CIM");
            HistoryStatusComboBox.Items.Add("Waiting for Other Group");
            HistoryStatusComboBox.Items.Add("Resolved");
        }

        /*Name: Michael Figueroa
        Function Name: CancelButton_Click
        Purpose: Event Handler for cancel button
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: MessageBoxResult messageBoxResult
        Algorithm: If OK is selected, then the form is closed
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
        Function Name: SubmitIssueButton_Click
        Purpose: Event Handler for Submit button
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: insertQuery
        Algorithm: If this is a new status, then insertQuery is initialzied and the SQL connection is established; try-catch block; if not accessed from weeklyReviewApps, then SQL insertQuery is executed and form.BindDataGrid
        is called using the TaskNum of this issue (form being an EditRecord object); else, BindDataGrid is called using the weeklyReviewForm object 
        Else, if not a new status, updateQuery is executed instead on the already existing History record
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SubmitIssueButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNewStatus)
            {
                string insertQuery = Insert_HistoryTable();
                SqlConnection con = new SqlConnection(connectionString);

                try
                {
                    if (!isWeekly)
                    {
                        con.Open();

                        SqlCommand issuesCmd = new SqlCommand(insertQuery, con);
                        issuesCmd.ExecuteNonQuery();

                        Updated.Visibility = Visibility.Visible;
                        this.Close();
                        string TaskNum = currentID.ToString();
                        form.BindDataGrid(TaskNum);
                    }

                    else
                    {
                        con.Open();

                        SqlCommand issuesCmd = new SqlCommand(insertQuery, con);
                        issuesCmd.ExecuteNonQuery();

                        Updated.Visibility = Visibility.Visible;
                        this.Close();
                        string TaskNum = currentID.ToString();
                        weeklyReviewForm.BindDataGrid(TaskNum);
                    }
                }

                catch
                {
                    MessageBox.Show("Please Try Again");
                }

                finally
                {
                    con.Close();
                }
            }
            
            else
            {
                string updateQuery = Update_HistoryTable();
                SqlConnection con = new SqlConnection(connectionString);

                try
                {
                    con.Open();

                    SqlCommand issuesCmd = new SqlCommand(updateQuery, con);
                    issuesCmd.ExecuteNonQuery();

                    Updated.Visibility = Visibility.Visible;
                    this.Close();
                    string TaskNum = currentID.ToString();
                }

                catch
                {
                    MessageBox.Show("Please Try Again");
                }

                finally
                {
                    con.Close();
                }
            }
        }

        /*Name: Michael Figueroa
       Function Name: GetStatus
       Purpose: Getter that retrieves the current Status chosen from the HistoryStatusComboBox
       Parameters: None
       Return Value: None
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private string GetStatus()
        {
            return HistoryStatusComboBox.SelectedItem.ToString();
        }

        /*Name: Michael Figueroa
       Function Name: Update_Planned_Date
       Purpose: Sets planned date to today's date; this is used when Status is changed to resolved
       Parameters: string TaskNum
       Return Value: None
       Local Variables: query
       Algorithm: Sets Due_Date to today's date for the specified New_Issues record
       Version: 2.0.0.4
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       */
        private void Update_Planned_Date(string TaskNum)
        {
            string query = "UPDATE New_Issues SET Due_Date = GETDATE() WHERE ID = " + TaskNum + ";";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            SqlCommand dateUpdate = new SqlCommand(query, con);
            dateUpdate.ExecuteNonQuery();
        }

      /*Name: Michael Figueroa
      Function Name: Insert_HistoryTable
      Purpose: Runs the Insert query to the history table for the new status being added
      Parameters: string TaskNum
      Return Value: None
      Local Variables: TaskNum, date, stusnt, query
      Algorithm: Sets Due_Date to today's date for the specified New_Issues record if the Status is set to Scheduled Implementation, then DataGrid is re-binded by calling BindDataGrid (if the status is not accessed through weekly
      form, then the BindDataGrid from the edit form is used, else, the weeklyReviewApps one is used
      Catch block: if not a null reference exception, then user is notified they must choose a status from dropdown
      Version: 2.0.0.4
      Date modified: Prior to 1/1/20
      Assistance Received: N/A
      */
        private string Insert_HistoryTable()
        {              
            try
            {
                string TaskNum = currentID.ToString();
                string date = StatusDatePicker.SelectedDate.ToString();
                string stusnt = StatusNoteText.Text;

                string query = "INSERT INTO History (TaskNum, EntryDate, [Status], New_StatusNote) " +
                            "VALUES (" + TaskNum + ", '" + date + "','" + stusnt.Replace("'", "\''") + "','" + GetStatus() + "');";

                if (GetStatus() == "Scheduled Implementation")
                {
                    Update_Planned_Date(TaskNum);
                }

                if (!isWeekly)
                {
                    form.BindDataGrid(TaskNum);
                }

                else
                {
                    weeklyReviewForm.BindDataGrid(TaskNum);
                }

                return query;
            }
            
            catch(NullReferenceException ex)
            {
                MessageBox.Show(ex.ToString());
            }
           catch(Exception)
            {
                MessageBox.Show("Please Choose Status From Menu");
            }
            return null;
         }

        /*Name: Michael Figueroa
        Function Name: Update_HistoryTable
        Purpose: Runs the Update query to the history table for the current status being edited
        Parameters: none
        Return Value: string
        Local Variables: TaskNum, date, stusnt, histID, query
        Algorithm: assigns values to local variables, then, Sets Due_Date to today's date for the specified New_Issues record if the Status is set to Scheduled Implementation, then DataGrid is re-binded by calling 
        BindDataGrid (if the status is not accessed through weekly form, then the BindDataGrid from the edit form is used, else, the weeklyReviewApps one is used) 
        Catch block: if not a null reference exception, then user is notified they must choose a status from dropdown
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private string Update_HistoryTable()
        {
                    string TaskNum = currentID.ToString();
                    string date = StatusDatePicker.SelectedDate.ToString();
                    string stusnt = StatusNoteText.Text;
                    string histID = histRow["ID"].ToString();
            try
            {
                string query = "UPDATE History SET EntryDate='" + date + "', [Status]='" + stusnt.Replace("'", "\''") + "', New_StatusNote='" + GetStatus() + "' " +
                               "WHERE ID=" + histID + " AND TaskNum=" + TaskNum;

                //Automatically changes Planned Date to current date if scheduled implementation is chosen as status
                //CHanges back to null otherwise

                if (GetStatus() == "Scheduled Implementation")
                {
                    Update_Planned_Date(TaskNum);
                }

                if (!isWeekly)
                {
                    form.BindDataGrid(TaskNum);
                }

                else
                {
                    weeklyReviewForm.BindDataGrid(TaskNum);
                }

                return query;
            }

            catch
            {
                MessageBox.Show("Must Choose a Status");
                return null;
            }
        }


        /*Name: Michael Figueroa
        Function Name: DeleteIssueButton_Click
        Purpose: Deletes current status chosen
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: TaskNum, query
        Algorithm: Routine SQL execution, then if not accessed through weekly, BindDataGrid EditRecord method used, else, the one from weekly is used.
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void DeleteIssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string TaskNum = currentID.ToString();
                string query = "DELETE FROM History WHERE ID = " + histRow["ID"] + ";";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand issuesCmd = new SqlCommand(query, con);
                    issuesCmd.ExecuteNonQuery();

                    MessageBox.Show("Status Deleted");
                    con.Close();
                    this.Close();
                    if (!isWeekly)
                    {
                        form.BindDataGrid(TaskNum);
                    }
                    else
                    {
                        weeklyReviewForm.BindDataGrid(TaskNum);
                    }
                }               
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.ToString());
            }
        }
    }
}