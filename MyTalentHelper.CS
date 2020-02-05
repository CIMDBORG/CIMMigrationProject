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
        public static int GetUserUpdateCount(string userUpdate)
        {
            char[] delimiter = new char[] {';'};
            string[] userUpdateArray = userUpdate.Split(delimiter);
            return userUpdateArray.Length;
        }

        public static string GetFirstManagerUpdate(string managerUpdate)
        {
            char[] delimiter = new char[] {';'};
            string[] managerUpdateArray = managerUpdate.Split(delimiter);
            return managerUpdateArray[0];
        }
    }
}