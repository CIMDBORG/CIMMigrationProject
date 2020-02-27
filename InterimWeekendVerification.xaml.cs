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

//PLEASE NOTE: the majority of the methods in this class should be in a separate helper class
namespace Interim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class InterimWeekendVerification : Window
    {
        //ConnectionString comes from App.config
        //issue_date is used to collect information from query that fills out the form; this includes tracking numbers, current tracking number statuses (pass, audit, invalid)
        //List of test case IDs assigned to one particular person is stored in an int list
        //verificationDay keeps track of what day of notes is updated/shown on form. By default, this is the first day of verification (tuesday)
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] issue_data;
        private string[] interim_statuses;
        private List<int> IDs;
        private string verificationDay;

        /*Name: Michael Figueroa
        Function Name: InterimWeekendVerification
        Purpose: InterimWeekendVerification Constructor
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: Calls FillAssignedComboBox twice (once with AssignedComboBox and again with AltComboBox as parameter), calls FillResultComboBOx, FillDayCheckBox,
        FillStatusComboBoxes, sets AssignedComboBox index to 0 and calls GetScenario IDs to fill List IDs
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public InterimWeekendVerification()
        {
            InitializeComponent();
            FillAssignedComboBox(AssignedCombobox);
            FillAssignedComboBox(AltComboBox);
            FillResultComboBox();
            FillDayCheckBox();
            FillStatusComboBoxes();
            AssignedCombobox.SelectedIndex = 0;
            IDs = GetScenarioIDs();
            if (IDCount() > 0)
            {
                DayComboBox.SelectedIndex = 0;
                CurrentIssue.Text = "1";
                TotalIssues.Text = "of " + IDs.Count;
                SelectScenarioData(IDs[0].ToString());
                FillInForm();
            }
        }
        /*Name: Michael Figueroa
        Function Name: IDCount
        Purpose: Getter that returns the count of List<int> IDList
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        public int IDCount()
        {
            return IDs.Count;
        }
        
        /*Name: Michael Figueroa
        Function Name: FillStatusComboBoxes
        Purpose: Fills all status comboBoxes
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: None
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void FillStatusComboBoxes()
        {
            IncStatusComboBoxOne.Items.Add("Audit");
            IncStatusComboBoxOne.Items.Add("Invalid");
            IncStatusComboBoxOne.Items.Add("Help");

            IncStatusComboBoxTwo.Items.Add("Audit");
            IncStatusComboBoxTwo.Items.Add("Invalid");
            IncStatusComboBoxTwo.Items.Add("Help");

            NIStatusComboBoxOne.Items.Add("Audit");
            NIStatusComboBoxOne.Items.Add("Invalid");
            NIStatusComboBoxOne.Items.Add("Help");

            NIStatusComboBoxTwo.Items.Add("Audit");
            NIStatusComboBoxTwo.Items.Add("Invalid");
            NIStatusComboBoxTwo.Items.Add("Help");
        }

        /*Name: Michael Figueroa
        Function Name: FillStatusComboBoxes
        Purpose: Get the IDs of the test cases assigned to one person
        Parameters: None
        Return Value: N/A
        Local Variables: List<int> IDList, string query, string queryTwo
        Algorithm: string query is used to create cmd; SqlDataReader is used to read through each record that string query produces, and adds the ID of each record into IDList;
        The same thing is done with queryTwo, which checks to see if the user has been alt assigned to any individual test cases.
        If IDList is empty after all of that, then the catch block will catch the exception and tell the user that no test cases have been assigned for that particular day
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private List<int> GetScenarioIDs()
        {
            DateTime currentDate = DateTime.Now;

            List<int> IDList = new List<int>();
            string query = "Select INTERIM_TEST_CASES.[INTERIM_ID] AS INTERIM_ID from INTERIM_TEST_CASES INNER JOIN INTERIM_ASSIGNMENTS " +
                "ON(INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_ASSIGNMENTS.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
                "WHERE (INTERIM_ASSIGNMENTS.INTERIM_SAT_ASSIGN LIKE '%" + AssignedCombobox.SelectedItem.ToString() + "%' " +
                "OR INTERIM_TEST_CASES.INTERIM_ASSIGNED_NAME LIKE '%" + AssignedCombobox.SelectedItem.ToString() + "%')" +
                " AND (INTERIM_TYPE = 'Weekly');";

            string queryTwo = "Select INTERIM_TEST_CASES.[INTERIM_ID] AS INTERIM_ID from INTERIM_TEST_CASES WHERE(INTERIM_TEST_CASES.INTERIM_ASSIGNED_NAME LIKE '%" + AssignedCombobox.SelectedItem.ToString() + "%') AND (INTERIM_TEST_CASES.INTERIM_TYPE = 'Weekly');";

            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    
                        con.Open();
                        SqlCommand cmd = new SqlCommand(query, con);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    IDList.Add(reader.GetInt32(0));
                                }
                            }
                            reader.Close();
                        }

                        SqlCommand cmdTwo = new SqlCommand(queryTwo, con);
                        using (SqlDataReader readerTwo = cmdTwo.ExecuteReader())
                        {
                            while (readerTwo.Read())
                            {
                                for (int i = 0; i < readerTwo.FieldCount; i++)
                                {
                                    IDList.Add(readerTwo.GetInt32(0));
                                }
                            }
                            readerTwo.Close();
                        }

                        return IDList;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No scenarios have been assigned yet");
                    return null;
                }
                finally
                {
                    con.Close();
                }
        }

        /*Name: Michael Figueroa
          Function Name: AssignedCombobox_SelectionChanged
          Purpose: Event handler for when someone choses a new name from AssignedCombobox
          Parameters: Auto-Generated
          Return Value: N/A
          Local Variables: None
          Algorithm: Re-Populates List<int> IDs by calling GetScenarioIDs
          If IDs.Count > 0, then SelectScenarioData, FillInForm, DayComboBox index is set to 0, BindNotes, and BindStatuses are called
          else, the exception is handled and the user is told that no scenarios have been imported for today
          Version: 2.0.0.4
          Date modified: 1/7/20
          Assistance Received: N/A
          */
        private void AssignedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IDs = GetScenarioIDs();
            if (IDCount() > 0)
            {
                CurrentIssue.Text = "1";
                TotalIssues.Text = "of " + IDs.Count;
                SelectScenarioData(IDs[0].ToString());
                FillInForm();
                DayComboBox.SelectedIndex = 0;
                BindNotes();
                BindStatuses();
            }
            else
            {
                MessageBox.Show("No Weekend Scenarios Available at this Time");
            }
        }

        /*Name: Michael Figueroa
       Function Name: FillAssignedComboBox
       Purpose: Fills AssignedComboBox and also can be used to fill AltComboBox
       Parameters: ComboBox combobox
       Return Value: N/A
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void FillAssignedComboBox(ComboBox comboBox)
        {
            comboBox.Items.Add("Carlos");
            comboBox.Items.Add("Chris");
            comboBox.Items.Add("Brandon");
            comboBox.Items.Add("Dom");
            comboBox.Items.Add("Ellen");
            comboBox.Items.Add("Jan-Marie");
            comboBox.Items.Add("Jeff");
            comboBox.Items.Add("Ken");
            comboBox.Items.Add("Mike");
            comboBox.Items.Add("Morty");
            comboBox.Items.Add("Nick");
            comboBox.Items.Add("Pawel");
            comboBox.Items.Add("Sam");
            comboBox.Items.Add("Tau");
        }

        /*Name: Michael Figueroa
       Function Name: FillResultComboBox
       Purpose: Fills ResultCombobox 
       Parameters:  None
       Return Value: N/A
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void FillResultComboBox()
        {
            ResultCombobox.Items.Add("Pass");
            ResultCombobox.Items.Add("Re-Verify");
            ResultCombobox.Items.Add("Can't Verify");
            ResultCombobox.Items.Add("Fail");
        }

        /*Name: Michael Figueroa
       Function Name: FillDayCheckBox
       Purpose: Fills DayComboBox 
       Parameters:  None
       Return Value: N/A
       Local Variables: None
       Algorithm: None
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void FillDayCheckBox()
        {
            DayComboBox.Items.Add("Tue");
            DayComboBox.Items.Add("Wed");
            DayComboBox.Items.Add("Thu");
            DayComboBox.Items.Add("Fri");
            DayComboBox.Items.Add("Help");
        }

        /*Name: Michael Figueroa
        Function Name: FillInForm
        Purpose: Fills in the form with the data from issue_data (which is an array filled in SelectScenarioData method)
        Parameters: None
        Return Value: N/A
        Local Variables: None
        Algorithm: if issue_data[12] is not empty, then  AltComboBox.SelectedItem = issue_data[12]; else AltComboBox.SelectedItem = null
        if issue_data[13] is not null, then Defect.Text = issue_data[13]; else, Defect.Text is set to null
        Then calls DetermineDups and BindResult
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void FillInForm()
        {
            IncShipNumOne.Text = issue_data[0];
            IncTrkNumOne.Text = issue_data[1];
            NonIncShipNumOne.Text = issue_data[5];
            NonIncTrkNumOne.Text = issue_data[4];
            IncShipNumTwo.Text = issue_data[2];
            IncTrkNumTwo.Text = issue_data[3];
            NonIncShipNumTwo.Text = issue_data[7];
            ID.Text = "ID: " + issue_data[8];
            CC.Text = issue_data[9];
            NonIncTrkNumTwo.Text = issue_data[6];
            Source.Text = "Source: " + issue_data[10];
            Description.Text = issue_data[11];

            if (issue_data[12] != null)
            {
                AltComboBox.SelectedItem = issue_data[12];
            }
            else
            {
                AltComboBox.SelectedItem = null;
            }
            if (issue_data[13] != null)
            {
                Defect.Text = issue_data[13];
            }
            else
            {
                Defect.Text = null;
            }
            DetermineDups();
            BindResult();
        }

        /*Name: Michael Figueroa
       Function Name: BindResult
       Purpose: Binds INTERIM_CRITERIA_STATUS for record INTERIM_ID = GetID in INTERIM_TEST_CASES table to ResultComboBox
       Parameters: None
       Return Value: N/A
       Local Variables: string query, string result, int cols, string[] data
       Algorithm: string query is used to construct SqlCommand which then is read using SqlDataReader. string query will only produce one record since INTERIM_ID is a unique
       identifier. Using the reader, INTERIM_CRITERIA_STATUS from the one record is added to data[]; after the while loop, string result is set equal to data[0], and 
       ResultComboBox selected item is set equal to string result - if data[0] is null, then ResultComboBox will also be set as null
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void BindResult()
        {
            string query;

            query = "SELECT INTERIM_CRITERIA_STATUS " +
                "FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " + 
                "WHERE INTERIM_ID = " + GetID() + ";";

            string result;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    {
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
                        if (data[0].Length > 0)
                        {
                            result = data[0];
                            ResultCombobox.SelectedItem = result;
                        }
                        else
                        {
                            ResultCombobox.Text = null;
                        }
                    }

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
        }

        /*Name: Michael Figueroa
        Function Name: SelectScenarioData
        Purpose: Fills issue_date array
        Parameters: string ID
        Return Value: N/A
        Local Variables: None
        Algorithm: string query is initialized, then, using standard SQL Procedure, the query results are read in the while loop, and added to data[x]; issue_date is set equal to data
        Version: 2.0.0.4
        Date modified: 1/7/20
        Assistance Received: N/A
        */
        private void SelectScenarioData(string ID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    string query = "SELECT INTERIM_BI_SHIP_NUM1, INTERIM_BI_TRACK_NUM1, INTERIM_BI_SHIP_NUM2, INTERIM_BI_TRACK_NUM2, INTERIM_NI_TRACK_NUM1, " +
                        "INTERIM_NI_SHIP_NUM1, INTERIM_NI_TRACK_NUM2" +
                    ", INTERIM_NI_SHIP_NUM2, INTERIM_ID, INTERIM_TEST_CASES.INTERIM_CC, INTERIM_HISTORY.INTERIM_SOURCE AS SOURCE, " +
                    "INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA AS DESCRIPTION, INTERIM_ALT_AUD, INTERIM_DEFECT_NO FROM INTERIM_HISTORY " +
                    "INNER JOIN INTERIM_TEST_CASES ON (INTERIM_HISTORY.INTERIM_DESCRIPTION = INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA) AND (INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " +
                    "WHERE INTERIM_TEST_CASES.INTERIM_ID = '" + ID + "';";

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
       Function Name: DetermineDups
       Purpose: checks whether a tracking number is a dup or not
       Parameters: None
       Return Value: N/A
       Local Variables: incTrkNumOneCnt, int incTrkNumTwoCnt, int niTrkNumOneCnt, int niTrkNumTwoCnt, int cols
       Algorithm: The amount of times each tracking number iappears in INTEIRM_TEST_CASES is found
       using incTrackingNumCountOne, int incTrkNumTwoCnt, int niTrkNumOneCnt, and int niTrkNumTwoCnt; if there is an instance where one of those tracking numbers appears
       multiple times, then is marked as a dup
       Version: 2.0.0.4
       Date modified: 1/7/20
       Assistance Received: N/A
       */
        private void DetermineDups()
        {
            int incTrkNumOneCnt;
            int incTrkNumTwoCnt;
            int niTrkNumOneCnt;
            int niTrkNumTwoCnt;

            string incTrackingNumCountOne = "SELECT COUNT(INTERIM_BI_TRACK_NUM1) FROM INTERIM_TEST_CASES " +
                                        "WHERE INTERIM_BI_TRACK_NUM1 = '" + IncTrkNumOne.Text.ToString() + "' AND INTERIM_TEST_CASE_CRITERIA = '" + Description.Text.ToString() + "';";

            string incTrackingNumCountTwo = "SELECT COUNT(INTERIM_BI_TRACK_NUM2) FROM INTERIM_TEST_CASES " +
                                        "WHERE INTERIM_BI_TRACK_NUM2 = '" + IncTrkNumTwo.Text.ToString() + "' AND INTERIM_TEST_CASE_CRITERIA = '" + Description.Text.ToString() + "';";

            string niTrackingNumCountOne = "SELECT COUNT(INTERIM_NI_TRACK_NUM1) FROM INTERIM_TEST_CASES " +
                                        "WHERE INTERIM_NI_TRACK_NUM1 = '" + NonIncTrkNumOne.Text.ToString() + "'AND INTERIM_TEST_CASE_CRITERIA = '" + Description.Text.ToString() + "';";

            string niTrackingNumCountTwo = "SELECT COUNT(INTERIM_NI_TRACK_NUM2) FROM INTERIM_TEST_CASES " +
                                        "WHERE INTERIM_NI_TRACK_NUM2 = '" + NonIncTrkNumTwo.Text.ToString() + "'AND INTERIM_TEST_CASE_CRITERIA = '" + Description.Text.ToString() + "';";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand command1 = new SqlCommand(incTrackingNumCountOne, connection);
                    SqlCommand command2 = new SqlCommand(incTrackingNumCountTwo, connection);
                    SqlCommand command3 = new SqlCommand(niTrackingNumCountOne, connection);
                    SqlCommand command4 = new SqlCommand(niTrackingNumCountTwo, connection);

                    SqlDataReader reader1 = command1.ExecuteReader();
                    int cols = reader1.FieldCount;
                    while (reader1.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            incTrkNumOneCnt = reader1.GetInt32(0);
                            if ((incTrkNumOneCnt > 1) && !(IncTrkNumOne.Text.StartsWith("X")))
                            {
                                IncTrkNumOneDup.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                IncTrkNumOneDup.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                    reader1.Close();

                    SqlDataReader reader2 = command2.ExecuteReader();
                    while (reader2.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            incTrkNumTwoCnt = reader2.GetInt32(0);
                            if (incTrkNumTwoCnt > 1 && !(IncTrkNumTwo.Text.StartsWith("X")))
                            {
                                IncTrkNumTwoDup.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                IncTrkNumTwoDup.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                    reader2.Close();

                    SqlDataReader reader3 = command3.ExecuteReader();
                    while (reader3.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            niTrkNumOneCnt = reader3.GetInt32(0);
                            if (niTrkNumOneCnt > 1 && !(NonIncTrkNumOne.Text.StartsWith("X")))
                            {
                                NonIncTrkNumOneDup.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                NonIncTrkNumOneDup.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                    reader3.Close();

                    SqlDataReader reader4 = command4.ExecuteReader();
                    while (reader4.Read())
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            niTrkNumTwoCnt = reader4.GetInt32(0);
                            if (niTrkNumTwoCnt > 1 && !(NonIncTrkNumTwo.Text.StartsWith("X")))
                            {
                                NonIncTrkNumTwoDup.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                NonIncTrkNumTwoDup.Visibility = Visibility.Hidden;
                            }
                        }
                    }

                    reader4.Close();

                    connection.Close();
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
         Function Name: BackArrow_Click
         Purpose: Event Handler that allows user to scroll through the test cases (in a backwards manner)
         Parameters: Auto-Generated
         Return Value: None
         Local Variables: string current, int currentID
         Algorithm: if currentID - 1 >= 0, Calls SelectScenarioData, FillInForm, BindNotes, BindStatuses
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
                SelectScenarioData(IDs[currentID].ToString());
                FillInForm();
                BindNotes();
                BindStatuses();
            }
        }

        /*Name: Michael Figueroa
        Function Name: ForwardArrow_Click
        Purpose: Event Handler that allows user to scroll through the test cases (in a forwards manner)
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string current, int currentID
        Algorithm: if currentID + 1 < IDs.Count, Calls SelectScenarioData, FillInForm, BindNotes, BindStatuses
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void ForwardArrow_Click(object sender, RoutedEventArgs e)
        {
            string current = CurrentIssue.Text.ToString();
            int currentID = Int32.Parse(current) - 1;

            if ((currentID + 1) < (IDs.Count))
            {
                currentID++;

                CurrentIssue.Text = (currentID + 1).ToString();
                SelectScenarioData(IDs[currentID].ToString());
                FillInForm();
                BindNotes();
                BindStatuses();
            }
        }

        /*Name: Michael Figueroa
        Function Name: CurrentIssue_KeyDown
        Purpose: allows user to jump to an issue by typing in a number
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string current, int currentID
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CurrentIssue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

            }
        }

        /*Name: Michael Figueroa
        Function Name: GetID
        Purpose: get current test case ID
        Parameters: None
        Return Value: int currentID
        Local Variables: string current, int currentIndex, int currentID
        Algorithm: None
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private int GetID()
        {
            string current = CurrentIssue.Text.ToString();           
            int currentIndex = Int32.Parse(current) - 1;
            int currentID = IDs[currentIndex];
            return currentID;
        }

        /*Name: Michael Figueroa
        Function Name: Update_On_Content_Change
        Purpose: Event handler for Defect TextBox - updates defect number
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string defectNumber, string updateQuery
        Algorithm: using basic sql prodcedure, executes updateQuery, then calls SelectScenarioData and FillInForm to refresh the test case data
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Update_On_Content_Change(object sender, TextChangedEventArgs e)
        {

            string defectNumber = Defect.Text.ToString();

            string updateQuery = "UPDATE INTERIM_TEST_CASES SET INTERIM_DEFECT_NO = '" + defectNumber + "' WHERE INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(updateQuery, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    FillInForm();
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
        Function Name: Notes_TextChanged
        Purpose: Event handler for Notes TextBox - updates notes based on what day of the week it is
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string notesQuery
        Algorithm: using basic sql prodcedure, executes notesQuery, then calls SelectScenarioData and FillInForm to refresh the test case data
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            string notesQuery = "UPDATE INTERIM_HISTORY SET INTERIM_" + verificationDay + "_NOTES = '" + Notes.Text.ToString().Replace("'", "\''") + "' FROM INTERIM_HISTORY " +
                                "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_ID = '" + GetID().ToString() + "';";
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(notesQuery, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    FillInForm();
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
            Function Name: AltComboBox_SelectionChanged
            Purpose: Event handler for AltComboBox selection changed
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: string altAud
            Algorithm: is AltComboBox is not null, altAud is set equal to the selected AltComboBox value; then, standard sql procedure executes the query and SelectScenarioData and
            FillInForm are called to refresh test case data
            Version: 2.0.0.4
            Date modified: Prior to 1/1/20
            Assistance Received: N/A
            */
        private void AltComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string altAud;
            if (AltComboBox.SelectedItem != null)
            {
                altAud = AltComboBox.SelectedItem.ToString();
            }
            else
            {
                altAud = "";
            }

            string altQuery = "UPDATE INTERIM_TEST_CASES SET INTERIM_ALT_AUD = '" + altAud + "' WHERE INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(altQuery, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    FillInForm();
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
            Function Name: IncStatusComboBoxOne_SelectionChanged
            Purpose: Event handler for IncStatusComboBoxOne selection changed
            Parameters: Auto-Generated
            Return Value: None
            Local Variables: string incStatComboBoxOne, string incStatusOne
            Algorithm: if IncStatusComboBoxOne is not null, incStatComboBoxOne is set equal to the selected IncStatusComboBoxOne value; 
            then, standard sql procedure executes the query and SelectScenarioData and FillInForm are called to refresh test case data
            Version: 2.0.0.4
            Date modified: Prior to 1/1/20
            Assistance Received: N/A
            */
        private void IncStatusComboBoxOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string incStatComboBoxOne;
            if (IncStatusComboBoxOne.SelectedItem != null)
            {
                incStatComboBoxOne = IncStatusComboBoxOne.SelectedItem.ToString();
            }
            else
            {
                incStatComboBoxOne = "";
            }

            string incStatusOne = "UPDATE INTERIM_HISTORY SET INTERIM_BI_SHIP_NUM1_STAT = '" + incStatComboBoxOne + "' FROM INTERIM_HISTORY " +
                                    "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                    "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_TEST_CASES.INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(incStatusOne, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    BindStatuses();
                    FillInForm();
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
           Function Name: IncStatusComboBoxTwo_SelectionChanged
           Purpose: Event handler for IncStatusComboBoxTwo selection changed
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: string incStatComboBoxTwo, string incStatusTwo
           Algorithm: if incStatComboBoxTwo is not null, incStatComboBoxTwo is set equal to the selected incStatComboBoxTwo value; 
           then, standard sql procedure executes the query and SelectScenarioData, BindStatuses and FillInForm are called to refresh test case data
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void IncStatusComboBoxTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string incStatComboBoxTwo;
            if (IncStatusComboBoxTwo.SelectedItem != null)
            {
                incStatComboBoxTwo = IncStatusComboBoxTwo.SelectedItem.ToString();
            }
            else
            {
                incStatComboBoxTwo = "";
            }

            string incStatusTwo = "UPDATE INTERIM_HISTORY SET INTERIM_BI_SHIP_NUM2_STAT = '" + incStatComboBoxTwo + "' FROM INTERIM_HISTORY " +
                                    "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                    "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_TEST_CASES.INTERIM_ID = " + GetID() + ";";


            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(incStatusTwo, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    BindStatuses();
                    FillInForm();
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
           Function Name: NIStatusComboBoxOne_SelectionChanged
           Purpose: Event handler for NIStatusComboBoxOne_SelectionChanged selection changed
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: string NIStatComboBoxOne, string NIStatusOne
           Algorithm: if NIStatusComboBoxOne is not null, NIStatComboBoxOne is set equal to the selected NIStatusComboBoxOne value; 
           then, standard sql procedure executes the query and SelectScenarioData, BindStatuses and FillInForm are called to refresh test case data
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void NIStatusComboBoxOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string NIStatComboBoxOne;
            if (NIStatusComboBoxOne.SelectedItem != null)
            {
                NIStatComboBoxOne = NIStatusComboBoxOne.SelectedItem.ToString();
            }
            else
            {
                NIStatComboBoxOne = "";
            }

            string NIStatusOne = "UPDATE INTERIM_HISTORY SET INTERIM_NI_SHIP_NUM1_STAT = '" + NIStatComboBoxOne + "' FROM INTERIM_HISTORY " +
                                   "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                   "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_TEST_CASES.INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(NIStatusOne, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    BindStatuses();
                    FillInForm();
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
           Function Name: NIStatusComboBoxTwo_SelectionChanged
           Purpose: Event handler for NIStatusComboBoxTwo selection changed
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: string NIStatComboBoxTwo, string NIStatusOne
           Algorithm: if NIStatusComboBoxTwo is not null, NIStatComboBoxTwo is set equal to the selected NIStatusComboBoxOne value; 
           then, standard sql procedure executes the query and SelectScenarioData, BindStatuses and FillInForm are called to refresh test case data
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void NIStatusComboBoxTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string NIStatComboBoxTwo;
            if (NIStatusComboBoxTwo.SelectedItem != null)
            {
                NIStatComboBoxTwo = NIStatusComboBoxTwo.SelectedItem.ToString();
            }
            else
            {
                NIStatComboBoxTwo = "";
            }

            string NIStatusTwo = "UPDATE INTERIM_HISTORY SET INTERIM_NI_SHIP_NUM2_STAT = '" + NIStatComboBoxTwo + "' FROM INTERIM_HISTORY " +
                                    "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                    "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_TEST_CASES.INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(NIStatusTwo, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    BindStatuses();
                    FillInForm();
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
           Function Name: ResultCombobox_SelectionChanged
           Purpose: Event handler for ResultCombobox selection changed - updates the result of the scenario
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: string result, string scenarioResult
           Algorithm: if ResultCombobox is not null, result is set equal to the selected ResultCombobox value; 
           then, standard sql procedure executes the query and SelectScenarioData, BindStatuses and FillInForm are called to refresh test case data
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void ResultCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string result;
            if (ResultCombobox.SelectedItem != null)
            {
                result = ResultCombobox.SelectedItem.ToString();
            }
            else
            {
                result = "";
            }

            string scenarioResult = "UPDATE INTERIM_HISTORY SET INTERIM_CRITERIA_STATUS = '" + result.Replace("'", "\''") + "' FROM INTERIM_HISTORY " +
                                    "INNER JOIN INTERIM_TEST_CASES ON(INTERIM_HISTORY.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) " +
                                    "AND(INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_TEST_CASES.INTERIM_ID = " + GetID() + ";";
            using (SqlConnection connection = new SqlConnection(connectionString))
                try
                {
                    connection.Open();
                    SqlCommand managerCmd = new SqlCommand(scenarioResult, connection);
                    managerCmd.ExecuteNonQuery();
                    SelectScenarioData(GetID().ToString());
                    BindStatuses();
                    FillInForm();
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
           Function Name: BindStatuses
           Purpose: Binds Statuses for each tracking number to form
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: string query, int cols, string[] data, 
           Algorithm: if ResultCombobox is not null, result is set equal to the selected ResultCombobox value;
           then, standard sql procedure executes the query and SelectScenarioData, BindStatuses and FillInForm are called to refresh test case data
           if interim_statuses[0] != null, then NIStatusComboBoxOne.Text = interim_statuses[0]
           else, NIStatusComboBoxOne.Text = null
           same goes for NIStatusComboBoxTwo, IncStatusComboBoxOne, IncStatusComboBoxOne
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void BindStatuses()
        {
            string query;

            query = "SELECT INTERIM_NI_SHIP_NUM1_STAT, INTERIM_NI_SHIP_NUM2_STAT, INTERIM_BI_SHIP_NUM1_STAT, INTERIM_BI_SHIP_NUM2_STAT " +
                "FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON ( INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " +
                "AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_ID = " + GetID() + ";";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    {
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
                        interim_statuses = data;

                        if (interim_statuses[0] != null)
                        {
                            NIStatusComboBoxOne.Text = interim_statuses[0];
                        }
                        else
                        {
                            NIStatusComboBoxOne.Text = null;
                        }
                        if (interim_statuses[1] != null)
                        {
                            NIStatusComboBoxTwo.Text = interim_statuses[1];
                        }
                        else
                        {
                            NIStatusComboBoxTwo.Text = null;
                        }
                        if (interim_statuses[2] != null)
                        {
                            IncStatusComboBoxOne.Text = interim_statuses[2];
                        }
                        else
                        {
                            IncStatusComboBoxOne.Text = null;
                        }
                        if (interim_statuses[3] != null)
                        {
                            IncStatusComboBoxTwo.Text = interim_statuses[3];
                        }
                        else
                        {
                            IncStatusComboBoxTwo.Text = null;
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
        }

        /*Name: Michael Figueroa
          Function Name: BindNotes
          Purpose: Binds Notes for each tracking number to form
          Parameters: Auto-Generated
          Return Value: None
          Local Variables: string query, string notesText
          Algorithm: query is defined based on what option is chosen from DayComboBox, then standard Sql procedure reads information into data[] array - if data[0] is not null,
          the Notes textbox is filled with the information from data[0]
          Version: 2.0.0.4
          Date modified: Prior to 1/1/20
          Assistance Received: N/A
          */
        private void BindNotes()
        {
            string query;
            string notesText;

            if (DayComboBox.SelectedItem.ToString() == "Tue")
            {
                query = "SELECT INTERIM_TUE_NOTES FROM INTERIM_HISTORY " +
                        "INNER JOIN INTERIM_TEST_CASES ON ( INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " +
                        "AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_ID = " + GetID() + ";";
            }

            else if (DayComboBox.SelectedItem.ToString() == "Wed")
            {
                query = "SELECT INTERIM_WED_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES " +
                    "ON ( INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
                    " WHERE INTERIM_ID = " + GetID() + ";";
            }

            else if (DayComboBox.SelectedItem.ToString() == "Thu")
            {
                query = "SELECT INTERIM_THU_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON ( INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
                    " WHERE INTERIM_ID = " + GetID() + ";";
            }
            else
            {
                query = "SELECT INTERIM_FRI_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON ( INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
                    "WHERE INTERIM_ID = " + GetID() + ";";
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    {
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

                        if (data[0] != null)
                        {
                            notesText = data[0];
                            Notes.Text = notesText;
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
        }

        //updates verificationDay variable based on day chosen on dropdown
        /*Name: Michael Figueroa
           Function Name: DayComboBox_SelectionChanged_1
           Purpose: Event handler 
           Parameters: Auto-Generated
           Return Value: None
           Local Variables: None
           Algorithm: if DayComboBox.SelectedItem.ToString()  is not null, verification day is changed to that value and BindNotes is called; else, verificationDay is set to an empty string
           Version: 2.0.0.4
           Date modified: Prior to 1/1/20
           Assistance Received: N/A
           */
        private void DayComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (DayComboBox.SelectedItem.ToString() != null)
            {
                verificationDay = DayComboBox.SelectedItem.ToString().ToUpper();
                BindNotes();
            }
            else
            {
                verificationDay = "";
            }
        }
    }
}