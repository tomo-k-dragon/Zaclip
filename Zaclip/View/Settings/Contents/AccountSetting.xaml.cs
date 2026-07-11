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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zaclip.View.Settings.Contents
{
    /// <summary>
    /// AccountSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class AccountSetting : System.Windows.Controls.UserControl
    {
        public AccountSetting()
        {
            InitializeComponent();
        }

    public void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginDialog = new LoginDialog();
            loginDialog.Owner = Window.GetWindow(this);
            loginDialog.ShowDialog();
        }
    }
}
