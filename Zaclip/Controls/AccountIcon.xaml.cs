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
using UserControl = System.Windows.Controls.UserControl;
using Ctrl = System.Windows.Controls;

namespace Zaclip.Controls
{
    /// <summary>
    /// AccountIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class AccountIcon : UserControl
    {
        public AccountIcon()
        {
            InitializeComponent();
        }

        /// <summary>アイコンボタンの読み込み時イベント。</summary>
        private void Icon_Button_Loaded(object sender, RoutedEventArgs e)
        {
            // クリックでコンテキストメニューを制御するためにコンテキストメニューを明示的にボタンに関連付ける。
            var button = (Ctrl.Button)sender;
            button.ContextMenu.PlacementTarget = button;
        }

        private void Icon_Click(object sender, RoutedEventArgs e) =>
            ((Ctrl.Button)sender).ContextMenu.IsOpen = true;
    }
}
