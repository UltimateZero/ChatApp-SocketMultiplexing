using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HCI_Library.Core
{
    class ConnectionManager
    {
        private static ConnectionManager instance = null;

        public static ConnectionManager getInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionManager();
            }

            return instance;
        }

        public User CurrentUser {
            get;
            private set;
        }


        public string StartLogin(string username, string password)
        {
            //Thread.Sleep(1000 * 4);
            if (username.Equals("student") && password.Equals("student"))
            {
                CurrentUser = new User(username, password, User.UserType.Student);
                return "VALID";
            }
            else if (username.Equals("teacher") && password.Equals("teacher"))
            {
                CurrentUser = new User(username, password, User.UserType.Teacher);
                return "VALID";
            }
            else if (username.Equals("admin") && password.Equals("admin"))
            {
                CurrentUser = new User(username, password, User.UserType.Admin);
                return "VALID";
            }
            else
            {
                CurrentUser = null;
                return "INVALID";
            }
               
        }

        public void LoginCancelled()
        {
            Thread.Sleep(1000 * 4);
        }
    }
}
