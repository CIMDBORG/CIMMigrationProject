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
        public static string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

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

        public static string Get_Chkbox_Str(ComboBox combobox)
        {
            return combobox.SelectedItem.ToString();
        }
    }
}