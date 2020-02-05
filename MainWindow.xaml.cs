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
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Reflection;
using WpfApp2;
using System.Deployment.Application;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private int x;
        private string[] user_data;

             /*         
             Name: Mike Figueroa
             Function Name: Mainwindow()           
             Purpose: constructor for Main window class   
             Parameters: NA                        
             Return Value: NA
             Local Variables: 
                       Version version returns the assembly verion number
             Algorithm: NA 
             Version: NA
             Date modified: NA
             Assistance Received:NA
             */
        public MainWindow()
        {
            InitializeComponent();
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = "Version: " + version.ToString();
        }

        /*         
        Name: Mike Figueroa
        Function Name: void Cancelbutton_click(object sender, RoutedEventArgs e)              
        Purpose: closes the application down (used in the main login screen)
        Parameters: ( auto-generated )
        Return Value: closes app
        Local Variables: na
        Algorithm: NA 
        Version: NA
        Date modified: NA
        Assistance Received:NA
        */
        private void Cancelbutton_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

            /*    
       Name: Mike Figueroa
       Function Name: void Submitbutton_Click(object sender, RoutedEventArgs e)
       Purpose: login button in the main menu(used in the main login screen)
       Parameters: ( auto-generated )
       Return Value: either user logs in or fails
       Local Variables: na
       Algorithm: NA 
       Version: NA
       Date modified: NA
       Assistance Received:NA
       */

        private void Submitbutton_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }


        /*     
        Name: Mike Figueroa
        Function Name: void Text_KeyDown(object sender, KeyEventArgs e)
        Purpose: event handler! for entry key..google it(used in the main login screen)
        Parameters: ( auto-generated )
        Return Value: either user logs in or fails
        Local Variables: na
        Algorithm: NA 
        Version: NA
        Date modified: NA
        Assistance Received:NA
        */
        // Checks if the user presses Return (Enter) key in ADID or Password box, which then triggers AttemptLogin to start login verification.
        private void Text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AttemptLogin();
            }
        }
        /*         
        Name: Mike Figueroa
        Function Name: static string EncodePasswordToBase64(string password)              
        Purpose: to encrytped passwords into sql
        Parameters: 
        string password - contains users password
        Return Value: returns excrypted version of password 
        Local Variables: 
        byte[] bytes - stores bytes of of password 
        byte[] inArray - stores the hashform in an array
        Version version returns the assembly verion number
        Algorithm: NA 
        Version: NA
        Date modified: NA
        Assistance Received:NA
        */
        public static string EncodePasswordToBase64(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

        /*          
        Name: Mike Figueroa
        Function Name: void AttemptLogin() 
        Purpose: validates user credientials resulting in user logging in or being displayed with an error message.
        Parameters: NA
        Return Value: NA
        Local Variables: 
        Algorithm: 
        1.looks to see if password length == to 4 
                      1a. if password length == 4 -- applicatoin calls ResetPassword followed by exiting login screen
                      1B. take user new password and assign to user_data
        2. else if user password != 4 characters long
                      2b. checks to see if password is valid by calling AdidPass_IsValid()

        3c. if not valid, show error message and exit screen
        Version: NA
        Date modified: NA
        Assistance Received:NA
        */
        
        private void AttemptLogin()
        {

            if (Passwordtext.Password.ToString().Length == 4)
            {
                ResetPassword reset = new ResetPassword(user_data);
                this.Close();
                reset.Show();
                user_data = FillUserData();
                ResetPassword resetPassword = new ResetPassword(user_data);
                this.Close();
                resetPassword.Show();
            }

            else
            {
                if (AdidPass_IsValid())
                {
                    user_data = FillUserData();

                    // check later 
                    if (isEDI(user_data))
                    {
                        MenuScreen menuScreen = new MenuScreen(user_data);
                        this.Close();
                        menuScreen.Show();
                    }
                    else
                    {
                        MenuScreen menuScreen = new MenuScreen(user_data);
                        this.Close();
                        menuScreen.Show();
                    }
                }


                else
                {
                    MessageBox.Show("Login failed. Try again.");
                    ADIDtext.Clear();
                    Passwordtext.Clear();
                }
            }
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

        /*          
Name:
Function Name:
Purpose: 
Parameters:
Return Value: 
Local Variables: 
Algorithm: 
Version: 
Date modified: 
Assistance Received: 
*/
        private int ExecuteLogin_GetADIDPasswordCombos(SqlConnection con)
        {
            string query1 = "select count(*) from New_Contacts where ADID = @ADID  and Password = @Pass";
            SqlCommand cmd1 = new SqlCommand(query1, con);

            var Adid = new SqlParameter("@ADID", SqlDbType.VarChar, 50);
            var Pass = new SqlParameter("@Pass", SqlDbType.VarChar, 100);
            Adid.Value = ADIDtext.Text.ToString();
            Pass.Value = EncodePasswordToBase64(Passwordtext.Password.ToString());
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

        private string[] ReturnSysArr(string systemString)
        {
            char delimiter = '/';
            string[] sys = systemString.Split(delimiter);
            return sys;
        }

        private bool isEDI(string[] user_data)
        {
            string[] sys = ReturnSysArr(user_data[7]);
            for (int i = 0; i < sys.Length; i++)
            {
                if (sys[i] == "EDI")
                    return true;
            }
            return false;
        }

        //*******************************************************************
        // Queries the New_Contacts table and pulls several data fields on particular user.
        // Returns a string[] containing the data on the user.
        //*******************************************************************
        private string[] FillUserData()
        {
            string[] query_results;
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    string query2 = "select top 1 ADID, First_Name, Last_Name, Manager, Director, [Group], Role, Systems from New_Contacts where ADID = @ADID and Password = @Pass";
                    SqlCommand cmd2 = new SqlCommand(query2, con);

                    var Adid2 = new SqlParameter("@ADID", SqlDbType.VarChar, 50);
                    var Pass2 = new SqlParameter("@Pass", SqlDbType.VarChar, 100);
                    Adid2.Value = ADIDtext.Text.ToString();
                    Pass2.Value = EncodePasswordToBase64(Passwordtext.Password.ToString());
                    cmd2.Parameters.Add(Adid2);
                    cmd2.Parameters.Add(Pass2);

                    SqlDataReader reader2;
                    reader2 = cmd2.ExecuteReader();

                    query_results = PullDataFromReader(reader2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    query_results = new string[1];
                }
                finally
                {
                    con.Close();
                }

            return query_results;
        }



        // Reads from a SqlDataReader and stores each field as an element of a string[], and returns that string[] when it is finished reading.
        private string[] PullDataFromReader(SqlDataReader reader2)
        {
            int cols = reader2.FieldCount;
            string[] reader_data = new string[cols];
            while (reader2.Read())
            {
                for (int i = 0; i < cols; i++)
                {
                    reader_data[i] = reader2.GetValue(i).ToString();
                }
            }
            reader2.Close();
            return reader_data;
        }
    }
}
