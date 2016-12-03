using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCI_Library.Pages;
using HCI_Library.Core;
using static HCI_Library.Core.User;

namespace HCI_Library
{
    public class PageSwitcher
    {
        private static PageSwitcher instance = null;

        public static PageSwitcher getInstance()
        {
            if(instance == null)
            {
                instance = new PageSwitcher();
            }
            //singleton - one instance only
            return instance;
        }

        private MainWindow mainWindow = null;

        private PageSwitcher() { }

        public void SetRefContent(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void SwitchToLogin()
        {
            mainWindow.Content = Login.getInstance();
        }

        private void SwitchToMainAll()
        {
            mainWindow.Content = new MainAll();
        }

        public void LoggedIn()
        {
            SwitchToMainAll();
            //User currentUser = ConnectionManager.getInstance().CurrentUser;
            //switch(currentUser.Type)
            //{
            //    case UserType.Student:   break;
            //    case UserType.Teacher: break;
            //    case UserType.Admin: break;
            //}
          
        }
    }
}
