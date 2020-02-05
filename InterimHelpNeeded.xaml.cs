using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Shapes;

namespace Interim
{
    /// <summary>
    /// Interaction logic for HelpNeeded.xaml
    /// </summary>
    public partial class InterimHelpNeeded : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] issue_data;
        private string[] interim_statuses;
        private List<int> IDs;
        private string verificationDay = "Tue";

        public InterimHelpNeeded()
    {
        InitializeComponent();
        FillAssignedComboBox(AssignedCombobox);
        FillAssignedComboBox(AltComboBox);
        FillResultComboBox();
        FillDayCheckBox();
        FillStatusComboBoxes();
        AssignedCombobox.SelectedIndex = 0;
        IDs = GetScenarioIDs();
        CurrentIssue.Text = "1";
        TotalIssues.Text = "of " + IDs.Count;
    }

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


    //Get the IDs of the test cases assigned to one person
    private List<int> GetScenarioIDs()
    {
        DateTime currentDate = DateTime.Now;


        List<int> IDList = new List<int>();
            string query = "SELECT INTERIM_ID FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE " +
                     "AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC " +
                     "AND INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)  WHERE (INTERIM_NI_SHIP_NUM1_STAT = 'Help') OR (INTERIM_NI_SHIP_NUM2_STAT = 'Help') " +
                     "OR (INTERIM_BI_SHIP_NUM1_STAT = 'Help') OR " + "(INTERIM_BI_SHIP_NUM2_STAT = 'Help');";

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

                return IDList;

            }

            catch (Exception ex)
            {
                MessageBox.Show("No scenarios have been loaded for today");
                return null;
            }
            finally
            {
                con.Close();
            }
    }

    public int IDCount()
    {
        return IDs.Count;
    }

    private void BindResult()
    {
        string query;

        query = "SELECT INTERIM_CRITERIA_STATUS " +
            "FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC " +
            "AND INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_ID = " + GetID() + ";";

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

    private void AssignedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        IDs = GetScenarioIDs();
        CurrentIssue.Text = "1";
        TotalIssues.Text = "of " + IDs.Count;
        if (IDs.Count > 0)
        {
            SelectScenarioData(IDs[0].ToString());
            FillInForm();
            DayComboBox.SelectedIndex = 0;
            BindNotes();
            BindStatuses();
        }
        else
        {
            MessageBox.Show("No Scenarios Imported for Today");
        }
    }

    private void FillAssignedComboBox(ComboBox comboBox)
    {
        comboBox.Items.Add("Brandon");
        comboBox.Items.Add("Chris");
        comboBox.Items.Add("Carlos");
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

    private void FillResultComboBox()
    {
        ResultCombobox.Items.Add("Pass");
        ResultCombobox.Items.Add("Re-Verify");
        ResultCombobox.Items.Add("Can't Verify");
        ResultCombobox.Items.Add("Fail");
    }



    private void FillDayCheckBox()
    {
        DayComboBox.Items.Add("Tue");
        DayComboBox.Items.Add("Wed");
        DayComboBox.Items.Add("Thu");
        DayComboBox.Items.Add("Fri");
        DayComboBox.Items.Add("Sat");
    }

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

        if (issue_data[12].Length > 0)
        {
            AltComboBox.SelectedItem = issue_data[12];
        }
        else
        {
            AltComboBox.Text = null;
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

    private void SelectScenarioData(string ID)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
            try
            {
                string query = "SELECT INTERIM_BI_SHIP_NUM1, INTERIM_BI_TRACK_NUM1, INTERIM_BI_SHIP_NUM2, INTERIM_BI_TRACK_NUM2, INTERIM_NI_TRACK_NUM1, " +
                    "INTERIM_NI_SHIP_NUM1, INTERIM_NI_TRACK_NUM2" +
                ", INTERIM_NI_SHIP_NUM2, INTERIM_ID, INTERIM_TEST_CASES.INTERIM_CC, INTERIM_ASSIGNMENTS.INTERIM_SOURCE AS SOURCE, " +
                "INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA AS DESCRIPTION, INTERIM_ALT_AUD, INTERIM_DEFECT_NO FROM INTERIM_ASSIGNMENTS " +
                "INNER JOIN INTERIM_TEST_CASES ON (INTERIM_ASSIGNMENTS.INTERIM_SOURCE = INTERIM_TEST_CASES.INTERIM_BILL_TYPE) AND (INTERIM_TEST_CASES.INTERIM_CC = INTERIM_ASSIGNMENTS.INTERIM_CC) " +
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

    //checks whether a tracking number is a dup or not
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
            SelectScenarioData(IDs[currentID].ToString());
            FillInForm();
            BindNotes();
            BindStatuses();
        }
    }

    //event handler for forward arrow
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

    //allows user to jump to an issue by typing in a number
    private void CurrentIssue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {

        }
    }

    //get current test case ID
    private int GetID()
    {
        string current = CurrentIssue.Text.ToString();
        int currentIndex = Int32.Parse(current) - 1;
        int currentID = IDs[currentIndex];
        return currentID;
    }

    //update defect number
    private void Update_On_Content_Change(object sender, TextChangedEventArgs e)
    {

        string defectNumber = Defect.Text.ToString();

        string updateQuery = "UPDATE INTERIM_TEST_CASES SET INTERIM_DEFECT_NO = '" + defectNumber + "' WHERE INTERIM_ID = '" + GetID().ToString() + "';";

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

    //update notes
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

    //updates the alternate auditor if needed
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

    //these 4 event methods update the status of the shipper numbers
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

    //updates result of tracking number
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

    private void BindStatuses()
    {
        string query;

        query = "SELECT INTERIM_NI_SHIP_NUM1_STAT, INTERIM_NI_SHIP_NUM2_STAT, INTERIM_BI_SHIP_NUM1_STAT, INTERIM_BI_SHIP_NUM2_STAT " +
            "FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " +
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

    //bind notes to the form based on what day is chosen from the combobox
    private void BindNotes()
    {
        string query;
        string notesText;

        if (DayComboBox.SelectedItem.ToString() == "Tue")
        {
            query = "SELECT INTERIM_TUE_NOTES FROM INTERIM_HISTORY " +
                    "INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) " +
                    "AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION) WHERE INTERIM_ID = " + GetID() + ";";
        }

        else if (DayComboBox.SelectedItem.ToString() == "Wed")
        {
            query = "SELECT INTERIM_WED_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES " +
                "ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
                " WHERE INTERIM_ID = " + GetID() + ";";
        }

        else if (DayComboBox.SelectedItem.ToString() == "Thu")
        {
            query = "SELECT INTERIM_THU_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
                " WHERE INTERIM_ID = " + GetID() + ";";
        }
        else
        {
            query = "SELECT INTERIM_FRI_NOTES FROM INTERIM_HISTORY INNER JOIN INTERIM_TEST_CASES ON (INTERIM_TEST_CASES.INTERIM_BILL_TYPE = INTERIM_HISTORY.INTERIM_SOURCE AND INTERIM_TEST_CASES.INTERIM_CC = INTERIM_HISTORY.INTERIM_CC) AND (INTERIM_TEST_CASES.INTERIM_TEST_CASE_CRITERIA = INTERIM_HISTORY.INTERIM_DESCRIPTION)" +
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

        private void DayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
