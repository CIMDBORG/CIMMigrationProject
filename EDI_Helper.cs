using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp2
{
    class EDI_Helper
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;//SQL Conenction string in app.config

        /*Name: Michael Figueroa
        Function Name: Fill_Table
        Purpose: Fills DataTable table with results from string query
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: None
        Algorithm: Uses standard SQL procedure in order to fill DataTable table with results of string query
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public static void Fill_Table(DataTable table, string query)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    //fill report DataGrid with the query generated
                    using (sda)
                    {
                        sda.Fill(table);
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }
        }

        /*Name: Michael Figueroa
       Function Name: Get_Chkbox_Str
       Purpose: Getter that returns the ToString value of ComboBox combobox
       Parameters: ComboBox combobox
       Return Value: None
       Local Variables: None
       Algorithm: None
       Date modified: Prior to 1/1/20
       Assistance Received: N/A
       Version: 2.0.0.4
       */
        public static string Get_Chkbox_Str(ComboBox combobox)
        {
            return combobox.SelectedItem.ToString();
        }
    }
}