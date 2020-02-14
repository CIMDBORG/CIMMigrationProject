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
using Microsoft.Office.Interop.Outlook;
using WpfApp1;
using Exception = System.Exception;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for EDI_Add_Company.xaml
    /// </summary>
    public partial class EDI_Add_Company : Window
    {
        string companyName;
        string companyAddress;
        string companyContactName;
        string companyPhone;
        string companyEmailOne;
        string companyEmailTwo;
        string companyEmailThree;
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

        public EDI_Add_Company()
        {
            InitializeComponent();
        }

        private void AssignValues()
        {
            companyName = CompanyName.Text.ToString();
            companyAddress = CompanyAddress.Text.ToString();
            companyContactName = CompanyContactName.Text.ToString();
            companyPhone = CompanyContactName.Text.ToString();
            companyEmailOne = Customer_Email_1.Text.ToString();
            companyEmailTwo = Customer_Email_2.Text.ToString();
            companyEmailThree = Customer_Email_3.Text.ToString();
        }

        public string GetAddCompanyQuery()
        {
            AssignValues();
            return "INSERT INTO EDI_COMPANY (COMPANY_NAME, COMPANY_ADDRESS, COMPANY_CONTACT_NAME, " +
                   "COMPANY_PHONE_NUMBER, COMPANY_EMAIL_ONE, COMPANY_EMAIL_TWO, COMPANY_EMAIL_THREE) VALUES ('" + companyName.Replace("'", "\''") + "', '" + companyAddress + "', '" +
                   companyContactName + "', '" + companyPhone + "', '" + companyEmailOne + "', '" + companyEmailTwo + "', '" + companyEmailThree + "');";
        }

        //Event handler for Add Company button
        //This also prompts the user to add a product to a company upon click
        //If Yes is chosen, then the user is taken to the Add Product screen, with the created company automatically selected from the dropdown
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //Check if the Company Name field is empty; if so, user will be notified that it is a required field
            if (String.IsNullOrEmpty(CompanyName.Text.ToString()))
            {
                MessageBox.Show("Company Name Field Cannot Be Empty");
            }
            else
            {
                string query = GetAddCompanyQuery();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();
                        SqlCommand addCmd = new SqlCommand(query, con);
                        addCmd.ExecuteNonQuery();
                        MessageBoxResult addProduct = MessageBox.Show("Company Added, Would You Like To Add a Product for this Company?", "Add Product", MessageBoxButton.YesNo);
                        if (addProduct == MessageBoxResult.Yes)
                        {
                            EDI_Search_Products addProdForm = new EDI_Search_Products();
                            addProdForm.Show();
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
                this.Close();
            }
        }
    }
}
