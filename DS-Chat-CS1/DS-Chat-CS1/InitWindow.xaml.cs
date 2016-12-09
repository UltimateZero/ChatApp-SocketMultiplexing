﻿using DS_Chat_CS1.Pages;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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

namespace DS_Chat_CS1
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : MetroWindow
    {
        public InitWindow()
        {
            InitializeComponent();
            Title = "Login";
            transitioning.Content = new Login();

        }


        public void SwitchToLobby()
        {
            Title = "Lobby";
            transitioning.Content = new Lobby();
        }

    }
}