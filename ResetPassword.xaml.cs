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
    /// 
    public partial class ResetPassword : Window
    {
//public member variable
        // holds the connection string to connect to DB 
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString; 
//private member variable 
        // int x ..... 
        private int x;  
        // arr - holds elements of the user ( First_Name, Last_Name, Manager, Director, [Group], Role, Systems)
        private string[] arr;


        // assigns the user the data passed from the user_data to resetpasswords member variable arr[];
        public ResetPassword(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
        }

        /*         
        Name: Mike Figueroa
        Function Name: PasswordReset()         
        Purpose: resets the users password given it meets the requirement
        Parameters: 
        Return Value:
        Local Variables: 
         * string resetQuery - query which will upate the users password in the DB
         * var Adid - will hold user adid content to pass in query
         * var Pass - holds the user password to pass in query 
        Algorithm: 
        Version: NA
        Date modified: NA
        Assistance Received:NA
        */
        private void PasswordReset()
        {
            // checks to see if length is greater than 6 if not, will show error 
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
                        
                        // assigning the value user types to the adid var 
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


        // calls passwordReset
        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                PasswordReset();
            }
        }

// function gets string password passed and returns the hashed version back
        public static string EncodePasswordToBase64(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

        /*         
Name: Mike Figueroa
Function Name: public bool AdidPass_IsValid()              
Purpose: determine if the password entered is valid or not 
Parameters: 
string password - contains users password
Return Value: returns false or true based on if password is valid
Local Variables: 
Algorithm: NA 
Version: NA
Date modified: NA
Assistance Received:NA
*/
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
        /*         
Name: Mike Figueroa
Function Name: private int ExecuteLogin_GetADIDPasswordCombos(SqlConnection con)             
Purpose: to encrytped passwords into sql
Parameters: 
string password - contains users password
Return Value: returns excrypted version of password 
Local Variables: 
    byte[] bytes - stores bytes of of password 
    byte[] inArray - stores the hashform in an array
    Version version - returns the assembly verion number
Algorithm: NA 
Version: NA
Date modified: NA
Assistance Received:NA
*/
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
