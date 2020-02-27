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
        //The following variables speak for themselves
        string companyName;
        string companyAddress;
        string companyContactName;
        string companyPhone;
        string companyEmailOne;
        string companyEmailTwo;
        string companyEmailThree;
        //Sql connection string found in App.xaml
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

        /*Name: Michael Figueroa
        Function Name: EDI_Add_Company
        Purpose: Constructor for the EDI_Add_Company form
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified:  1/2020
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public EDI_Add_Company()
        {
            InitializeComponent();
        }

        /*Name: Michael Figueroa
        Function Name: AssignValues
        Purpose: Assigns values to variables for this class based on the text in the textboxes
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: None
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        Version: 2.0.0.4
        */
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

        /*Name: Michael Figueroa
        Function Name: GetAddCompanyQuery
        Purpose: Produces an INSERT query to add a new EDI Company into the EDI_COMPANY table
        Parameters: None
        Return Value: None
        Local Variables: None
        Algorithm: Calls AssignValues()
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        Version: 2.0.0.4
        */
        public string GetAddCompanyQuery()
        {
            AssignValues();
            return "INSERT INTO EDI_COMPANY (COMPANY_NAME, COMPANY_ADDRESS, COMPANY_CONTACT_NAME, " +
                   "COMPANY_PHONE_NUMBER, COMPANY_EMAIL_ONE, COMPANY_EMAIL_TWO, COMPANY_EMAIL_THREE) VALUES ('" + companyName.Replace("'", "\''") + "', '" + companyAddress + "', '" +
                   companyContactName + "', '" + companyPhone + "', '" + companyEmailOne + "', '" + companyEmailTwo + "', '" + companyEmailThree + "');";
        }

        /*Name: Michael Figueroa
        Function Name: Add_Click
        Purpose: Event handler for Add Company button
        Parameters: Auto-Generated
        Return Value: None
        Local Variables: MessageBoxResult addProduct, string query
        Algorithm: if-else checks for empty Company Name - if Company Name field is null/empty, then MessageBox will prompt user and nothing
        happens; else, getAddCompanyQuery is called, then routine SQL Executes AddCompanyQuery 
        After product is added, user is asked if they want to add a product for the company; if yes, EDI_Add_Product form is opened and this form
        is closed.
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        Version: 2.0.0.4
        */
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
