using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPassword : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private int x;
        private string[] arr;

        public ResetPassword(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }


        private void PasswordReset()
        {
                if (NewPasswordText.Password.ToString().Length > 6)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                        try
                        {
                            string resetQuery = "UPDATE New_Contacts SET Password = @Pass WHERE ADID = @ADID";
                            con.Open();
                            SqlCommand cmd1 = new SqlCommand(resetQuery, con);
                            var Adid = new SqlParameter("@ADID", SqlDbType.VarChar, 50);
                            var Pass = new SqlParameter("@Pass", SqlDbType.VarChar, 100);
                            Adid.Value = ADIDtext.Text.ToString();
                            Pass.Value = EncodePasswordToBase64(NewPasswordText.Password.ToString());
                            cmd1.Parameters.Add(Adid);
                            cmd1.Parameters.Add(Pass);
                            cmd1.ExecuteNonQuery();
                            MessageBox.Show("Password Reset Successful!");
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error:" + ex.Message);
                        }
                        finally
                        {
                            con.Close();
                        }
                }

                else
                {
                    MessageBox.Show("New Password Must Be Longer Than 6 Characters, Please Try Again");
                }                   
        }

        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                PasswordReset();
            }
        }

        public static string EncodePasswordToBase64(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

        // Opens a SQL connection. Returns true if there is exactly 1 valid ADID/pw combination
        public bool AdidPass_IsValid()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    x = ExecuteLogin_GetADIDPasswordCombos(con);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            return (x == 1);
        }

        //*******************************************************************
        // Takes an open SQL connection as input, and queries the New_Contacts table using SQL parameters for security.
        // Returns the number of valid ADID/PW combos as an int based on user input of ADID and Password.
        //*******************************************************************
        private int ExecuteLogin_GetADIDPasswordCombos(SqlConnection con)
        {
            string query1 = "select count(*) from New_Contacts where ADID = @ADID  and Password = @Pass";
            SqlCommand cmd1 = new SqlCommand(query1, con);

            var Adid = new SqlParameter("@ADID", SqlDbType.VarChar, 50);
            var Pass = new SqlParameter("@Pass", SqlDbType.VarChar, 100);
            Adid.Value = ADIDtext.Text.ToString();
            cmd1.Parameters.Add(Adid);
            cmd1.Parameters.Add(Pass);

            int y = 0;
            SqlDataReader reader1 = cmd1.ExecuteReader();
            while (reader1.Read())
            {
                y = reader1.GetInt32(0);
            }
            reader1.Close();
            return y;
        }

        private void Cancelbutton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Submitbutton_Click(object sender, RoutedEventArgs e)
        {
            PasswordReset();
        }
    }
}