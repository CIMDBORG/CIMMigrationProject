using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Helper Class that contains a few methods that are used to generate myTalent spreadsheet
namespace WpfApp2
{
    class MyTalentHelper
    {
        /*Name: Michael Figueroa
        Function Name: GetUserUpdateCount
        Purpose: This returns the number of userUpdates a user has made on a particular issue
        Parameters: string userUpdate
        Return Value: int userUpdateArray.Length
        Local Variables: char[] delimiter, string[] userUpdateArray
        Algorithm: delimiter is instantiated, then userUpdate is split by delimiter, with the values added to a string array. The length
        of the array is returned. - This method may be flawed, have to look into it. 
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static int GetUserUpdateCount(string userUpdate)
        {
            char[] delimiter = new char[] {';'};
            string[] userUpdateArray = userUpdate.Split(delimiter);
            return userUpdateArray.Length;
        }

        /*Name: Michael Figueroa
        Function Name: GetFirstManagerUpdate
        Purpose:  Returns the first date on which a manager request a user update on an item
        Parameters: string managerUpdate
        Return Value: first value of managerUpdateArray
        Local Variables: char[] delimiter, string[] managerUpdateArray
        Algorithm: delimiter is instantiated, then managerUpdate is split by delimiter, with the values added to a string array. 
        Then, the first value in managerUpdateArray is returned. -This method seems to be fine, may need further testing
        Version: 2.0.0.4
        Date modified: Prior to 1/1/20
        Assistance Received: N/A
        */
        public static string GetFirstManagerUpdate(string managerUpdate)
        {
            char[] delimiter = new char[] {';'};
            string[] managerUpdateArray = managerUpdate.Split(delimiter);
            return managerUpdateArray[0];
        }
    }
}