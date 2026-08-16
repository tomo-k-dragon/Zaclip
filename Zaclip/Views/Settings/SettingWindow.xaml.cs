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
using Zaclip.States;
using Zaclip.View.Settings.Contents;

namespace Zaclip.View
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(SettingPage settingPage)
        {
            InitializeComponent();

            MenuListBox.SelectedIndex = (int)settingPage;
        }

        private void MenuListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            switch (MenuListBox.SelectedIndex)
            {
                case 1:
                    ContentArea.Content = new AccountSetting();
                    break;
                default:
                    ContentArea.Content = null;
                    break;
            }
        }
    }
}
