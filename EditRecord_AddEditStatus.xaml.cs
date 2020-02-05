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
        private bool isWeekly;
        private int currentID;    //DataRowView variable that stores the row from the History data that is displayed on the edit form. It is sent here on row double-click.


        //*******************************************************************
        // DESCRIPTION: Constructor for AddEditStatus that can only be called when "Add a Status" is clicked in the EditRecord form.
        //              Does not pre-populate any fields aside from today's date.
        //              Sets isNewStatus to true.
        //
        // INPUT:       EditRecord editRecord: this is the parent EditRecord Window on which the "Add a status" button was clicked.
        //              DataRowView priorBySystemRow: this is the prioritizationBysystem row that contains the data used to query for & populate the parent EdiRecord form
        //*******************************************************************
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


        //*******************************************************************
        // DESCRIPTION: Constructor for AddEditStatus that can only be called when an existing row in the datagrid of the EditRecord form is double-clicked.
        //              Does not pre-populate any fields aside from today's date.
        //              Sets isNewStatus to false.
        //
        // INPUT:       EditRecord editRecord: this is the parent EditRecord Window on which the "Add a status" button was clicked.
        //              DataRowView priorBySystemRow: this is the prioritizationBysystem row that contains the data used to query for & populate the parent EdiRecord form
        //              DataRowView statusDataRow: this is the row in EditRecord form that was double-clicked on to call this constructor and pre-populate this form.
        //*******************************************************************
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


        // Asks user to confirm that they wish to leave the form if they click 'Cancel'
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Exit form? All information entered will be cleared.", "Cancel Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                this.Close();
            }
        }



        //*******************************************************************
        // DESCRIPTION: Runs when the user clicks the 'Submit' button. Checks if isNewStatus is true or false.
        //              If isNewStatus is true, it will run an insert query into the History table since the user is adding a new status.
        //              if isNewStatus is false, it will run an update query to the History table since the user is editing an existing status.
        //*******************************************************************
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

        private string GetStatus()
        {
            return HistoryStatusComboBox.SelectedItem.ToString();
        }

        private void Update_Planned_Date(string TaskNum)
        {
            string query = "UPDATE New_Issues SET Due_Date = GETDATE() WHERE ID = " + TaskNum + ";";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            SqlCommand dateUpdate = new SqlCommand(query, con);
            dateUpdate.ExecuteNonQuery();
        }

        // Runs the Insert query to the history table for the new status being added
        private string Insert_HistoryTable()
        {              
            try
            {
                string TaskNum = currentID.ToString();
                string date = StatusDatePicker.SelectedDate.ToString();
                string stusnt = StatusNoteText.Text;

                string query = "INSERT INTO History (TaskNum, EntryDate, [Status], New_StatusNote) " +
                            "VALUES (" + TaskNum + ", '" + date + "','" + stusnt.Replace("'", "\''") + "','" + GetStatus() + "');";


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
        
        // Runs an uodate query to the history table to make changes to the already-existing status
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