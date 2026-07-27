using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Zaclip.ViewModel.Settings.Contents;

namespace Zaclip.View.Settings.Contents
{
    /// <summary>
    /// LoginDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginDialog : Window
    {
        public LoginDialog(LoginDialogViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            // PasswordBox の入力を ViewModel と同期
            // PasswordBox はセキュリティ上の理由から直接バインディングできないため
            txtPassword.PasswordChanged += (s, e) =>
            {
                if (viewModel != null)
                {
                    viewModel.Password = txtPassword.Password;
                }
            };

            viewModel.RequestClose += () => this.Close();
        }

    }

}
