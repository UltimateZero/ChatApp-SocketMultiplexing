using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCI_Library.Core
{
    class User
    {
        public enum UserType {
            Student,
            Teacher,
            Admin
        }

        public string Username { get; private set; }
        private string Password;
        public UserType Type { get; private set; }

        public User(string username, string password, UserType type)
        {
            Username = username;
            Password = password;
            Type = type;
            
        }

    }
}
