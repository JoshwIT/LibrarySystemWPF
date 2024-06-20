﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
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

namespace Smart_Library_Management_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SLMSDataContext _SLMS = null;
        bool loginFlag = false;

        public MainWindow()
        {
            InitializeComponent();
            _SLMS = new SLMSDataContext(Properties.Settings.Default.LibWonderConnectionString);
        }

        private void cbShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            tbPasswordCheck.Text = pbPassword.Password;
            tbPasswordCheck.Visibility = Visibility.Visible;
            pbPassword.Visibility = Visibility.Collapsed;
        }

        private void cbShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            pbPassword.Password = tbPasswordCheck.Text;
            pbPassword.Visibility=Visibility.Visible;
            tbPasswordCheck.Visibility=Visibility.Collapsed;
        }

        private void btSubmit_Click(object sender, RoutedEventArgs e)
        {
            loginFlag = false;

            var loginQuery = from l in _SLMS.Accounts
                             where

                                l.Username == tbUsername.Text
                             select l;

            if (tbUsername.Text.Length > 0 && pbPassword.Password.Length > 0)
            {
                if (loginQuery.Count() == 1)
                {
                    foreach (var login in loginQuery)
                    {
                        if (login.Password == pbPassword.Password || login.Password == tbPasswordCheck.Text)
                        {
                            loginFlag = true;
                        }
                    }
                }

                if (loginFlag)
                {
                    foreach(var login in loginQuery)
                    {
                        if (login.Acc_Type == "Admin")
                        {
                            MessageBox.Show("Welcome Admin!");
                            Admin_Homepage AH = new Admin_Homepage();
                            AH.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Welcome User!");
                            User_Homepage UP = new User_Homepage();
                            UP.Show();
                            this.Close();
                        }
                    }
                }

                else
                {
                    MessageBox.Show("Invalid Credentials!");
                    tbUsername.Text = null;
                    pbPassword.Password = null;
                    tbPasswordCheck.Text = null;
                }

            }
            else
            {
                MessageBox.Show("Please input a credential!");
                tbUsername.Text = null;
                pbPassword.Password = null;
                tbPasswordCheck.Text = null;
            }
        }

        private void btSignup_Click(object sender, RoutedEventArgs e)
        {
            Signup_Page SP = new Signup_Page();
            SP.Show();
            this.Close();
        }
    }
}
