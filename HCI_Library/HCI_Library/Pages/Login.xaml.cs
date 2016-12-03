using HCI_Library.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HCI_Library.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private static Login instance = null;

        public static Login getInstance()
        {
            if(instance == null)
            {
                instance = new Login();
            }
            return instance;
        }


        private const string LOGIN_TEXT = "Login";
        private const string CANCEL_TEXT = "Cancel";

        public Login()
        {
            InitializeComponent();
        }

 

        private void ForgotPassword_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            HideError();
            if(btnLogin.Content.Equals(LOGIN_TEXT))
            {
                SetLoggingIn();
                CheckLogin();
            }
            else
            {
                SetNormal();
            }
            
        }


        private async void CheckLogin()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.ToString().Trim();
            if(username.Length == 0)
            {
                SetNormal();
                ShowError("You must enter a username!");
                return;
            }
            if (password.Length == 0)
            {
                SetNormal();
                ShowError("You must enter a password!");
                return;
            }

            //Simulate logging in
            string result = await Task.Run(() =>
            {
                return ConnectionManager.getInstance().StartLogin(username, password);
            });

            //If cancelled
            if(btnLogin.Content.Equals(LOGIN_TEXT))
            {
                await Task.Run(() => ConnectionManager.getInstance().LoginCancelled());
                //MessageBox.Show("Connection cancelled");
                return;
            }

            if (result.Equals("VALID"))
            {
                SetNormal();
                //MessageBox.Show("Logged in successfully");
                PageSwitcher.getInstance().LoggedIn();
            }
            else if(result.Equals("INVALID"))
            {
                SetNormal();
                ShowError("Wrong username or password");
            }
            else 
            {
                SetNormal();
                ShowError(result);
            }

        }



        private void ShowError(string error)
        {
            lblError.Content = error;
            panelError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            panelError.Visibility = Visibility.Hidden;
        }

        private void SetLoggingIn()
        {
            progressLogin.Visibility = Visibility.Visible;
            lblForgotPassword.Visibility = Visibility.Hidden;
            btnLogin.Content = CANCEL_TEXT;

        }

        private void SetNormal()
        {
            progressLogin.Visibility = Visibility.Hidden;
            lblForgotPassword.Visibility = Visibility.Visible;
            btnLogin.Content = LOGIN_TEXT;

        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                btnLogin_Click(null, null);
        }
    }
}
