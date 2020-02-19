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

        public ErrorFile_Updates(string[] user_data)
        {
            InitializeComponent();
            
            arr = user_data;
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxFiller();
        }

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