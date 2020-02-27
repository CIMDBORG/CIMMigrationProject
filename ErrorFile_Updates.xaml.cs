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

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for ErrorFile_Updates.xaml
    /// </summary>
    public partial class ErrorFile_Updates : Window
    {
        private string[] arr;
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionS"].ConnectionString;

        /*Name: Brandon Cox
        Function Name: ErrorFile_Updates
        Purpose: Constructor for the ErrorFile_Updates form
        Parameters: string[] user_data
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public ErrorFile_Updates(string[] user_data)
        {
            InitializeComponent();
            
            arr = user_data;
        }

        /*Name: Brandon Cox
        Function Name: ComboBoxFiller
        Purpose: 
        Parameters: None
        Return Value: None
        Local Variables: DataTable dT
        Algorithm: Through standard sql procedure, account numbers are added to ComboBox AcctNum
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public void ComboBoxFiller()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand gather = conn.CreateCommand();
            gather.CommandText = "SELECT ACCT_NUM FROM NEWACCOUNTS WHERE TRANS_SHIPPER IS NULL";
            gather.ExecuteNonQuery();
            DataTable dT = new DataTable();
            SqlDataAdapter dA = new SqlDataAdapter(gather);
            dA.Fill(dT);
            foreach (DataRow dr in dT.Rows)
            {
                AcctNum.Items.Add(dr["ACCT_NUM"].ToString());
            }
            conn.Close();
        }

        /*Name: Brandon Cox
        Function Name: Window_Loaded
        Purpose: Event handler for window load event
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Calls ComboBoxFiller
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxFiller();
        }

        /*Name: Brandon Cox
        Function Name: SubmitBtn_Click
        Purpose: Event handler for SubmitBtn click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: string intL
        Algorithm: if IntlAcct checkbox is checked, then intL = "true"; else, equals "false. Then, using basic sql procedure, updateNewRecs query is
        executed, changes are saved, new ErrorFile_Updates form is opened
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection con = new SqlConnection(connectionString);
            string intL;
            try
            {
                con.Open();
                if(IntlAcct.IsChecked == true)
                {
                    intL = "true";
                }
                else
                {
                    intL = "false";
                }
                string updateNewRecs = "INSERT INTO NEWACCOUNTS (TRANS_SHIPPER, ACCT_END_DT, ETT_STAT, " +
                    "DESCRIPTION, ) VALUES('"+this.TransShip.Text+"', " +
                    "'"+this.SourceBox.Text+"','"+this.ShipName.Text+"','"+this.AcctEndDt.Text+"','"+this.EttStat.Text+"','"+this.IssDesc.Text+"')";
                SqlCommand sqlCmd = new SqlCommand(updateNewRecs, con);
                sqlCmd.Parameters.AddWithValue("@INTL_ACCT", intL);
                sqlCmd.ExecuteNonQuery();
                MessageBox.Show("Saved");
                ErrorFile_Updates errU = new ErrorFile_Updates(arr);
                con.Close();
                this.Close();
                errU.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*Name: Brandon Cox
        Function Name: CancelBtn_Click
        Purpose: Event handler for CancelBtn click
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: MessageBoxResult messageBoxResult
        Algorithm: messageBoxResult determines whether or not the current ErrorFileUpdates form is closed
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are You Sure?", "Cancel Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.OK)
            {
                ErrorFileMenu userM = new ErrorFileMenu(arr);
                userM.Show();
                this.Close();
            }
        }

        /*Name: Brandon Cox
        Function Name: AcctNum_SelectionChanged
        Purpose: Event handler for when AcctNum combobox selection changes
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: DataTable dT
        Algorithm: Result of gather query is used to fill DataTable dT; then foreach DataRow in dT, TrackNum.Text and ImpDt.Text are set
        accordingly
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        private void AcctNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand gather = conn.CreateCommand();
            gather.CommandType = CommandType.Text;
            gather.CommandText = "SELECT TRACK_NUM, IMPORT_DATE FROM NEWACCOUNTS WHERE TRANS_SHIPPER IS NULL AND ACCT_NUM='"+AcctNum.SelectedItem.ToString()+"';";
            gather.ExecuteNonQuery();
            DataTable dT = new DataTable();
            SqlDataAdapter dA = new SqlDataAdapter(gather);
            dA.Fill(dT);
            foreach (DataRow dR in dT.Rows)
            {
                TrackNum.Text = dR["TRACK_NUM"].ToString();
                ImpDt.Text = dR["IMPORT_DATE"].ToString();
            }

            conn.Close();
        }
    }
}