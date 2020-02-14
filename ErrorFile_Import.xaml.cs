using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Packaging;
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
using Microsoft.SqlServer.Dts.Runtime;
using Application = Microsoft.SqlServer.Dts.Runtime.Application;
using Package = Microsoft.SqlServer.Dts.Runtime.Package;
using System.Windows.Forms;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for ErrorFile_Import.xaml
    /// </summary>
    public partial class ErrorFile_Import : Window
    {
        private string[] arr;
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionS"].ConnectionString;


        public ErrorFile_Import(string[] user_data)
        {
            InitializeComponent();

            arr = user_data;

        }

        public void executePckg()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filepath = ofd.FileName;
                StreamReader reader = new StreamReader(File.OpenRead(ofd.FileName));
                // StreamReader reader = new StreamReader(File.Open(@"C:\Users\GNH2GNJ\Documents\ErrorFile.txt", FileMode.Open));
            SqlConnection connector = new SqlConnection(connectionString);
            connector.Open();
            string line = "";
            while (!String.IsNullOrEmpty(line = reader.ReadLine()))
            {
                string hello = reader.ReadLine().ToString();

                System.Windows.Forms.MessageBox.Show(hello);
                SqlCommand impCmd = connector.CreateCommand();
                impCmd.CommandText = "INSERT INTO LAST_IMPORTED_RECORDS(TRACK_NUM) VALUES('"+hello+"');";
                impCmd.ExecuteNonQuery();
            }

            }

        }

        public void TrackNumCleaner()
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand cleaner = con.CreateCommand();
            cleaner.CommandText = "UPDATE LAST_IMPORTED_RECORDS SET TRACK_NUM = SUBSTRING(column1,37,18)";
            cleaner.ExecuteNonQuery();
            
            SqlCommand nullDelete = con.CreateCommand();
            nullDelete.CommandText = "DELETE FROM LAST_IMPORTED_RECORDS WHERE SUBSTRING(TRACK_NUM,1,1)<>'1' OR column1 IS NULL";
            nullDelete.ExecuteNonQuery();

            SqlCommand rename = con.CreateCommand();
            rename.CommandText = "sp_rename 'LAST_IMPORTED_RECORDS.column1', 'TRACK_NUM', 'COLUMN'";
            rename.ExecuteNonQuery();

            SqlCommand addColumns = con.CreateCommand();
            addColumns.CommandText = "ALTER TABLE LAST_IMPORTED_RECORDS ADD [ACCT_NUM] VARCHAR(50), [IMPORT_DATE] VARCHAR(50), [TRANS_SHIPPER] VARCHAR(50)" +
                "[ACCT_END_DT] VARCHAR(50), [ETT_STAT] VARCHAR(50), [DESCRIPTION] VARCHAR(50), [INTL_ACCT] VARCHAR(50)";
            addColumns.ExecuteNonQuery();

            SqlCommand acctNum = con.CreateCommand();
            acctNum.CommandText = "UPDATE LAST_IMPORTED_RECORDS SET [ACCT_NUM] = SUBSTRING([TRACK_NUM],4,6)";
            acctNum.ExecuteNonQuery();

            SqlCommand timeStamp = con.CreateCommand();
            timeStamp.CommandText = "UPDATE ERROR_FILE SET [IMPORT_DATE] = GETDATE()";
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            executePckg();

            TrackNumCleaner();

        }

        private void bindDataGrid()
        {
            SqlConnection newCon = new SqlConnection(connectionString);
            newCon.Open();
            SqlCommand newCmd = new SqlCommand();
            newCmd.CommandText = "SELECT TRACK_NUM, ACCT_NUM, IMPORT_DATE FROM LAST_IMPORTED_RECORDS";
            newCmd.Connection = newCon;
            SqlDataAdapter dA = new SqlDataAdapter(newCmd);
            DataTable dT = new DataTable("LAST_IMPORTED_RECORDS");
            dA.Fill(dT);

            PreviewGrid.ItemsSource = dT.DefaultView;

        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            bindDataGrid();
        }
    }
}
