using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zaclip.Db;
using Zaclip.Models;
using Zaclip.ViewModel;
using System.Windows.Forms;
using Clipboard = System.Windows.Clipboard;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using ListViewItem = System.Windows.Controls.ListViewItem;

namespace Zaclip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;
        // クリップボード更新時にWindowsから送られてくるメッセージID
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private string _latestText = "";
        private bool _isInternalCopy;
        private NotifyIcon _notifyIcon;
        private bool _isShowDialog;

        // Win32 API: クリップボード監視登録
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);


        // Win32 API: クリップボード監視解除
        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public MainWindow()
        {
            InitializeComponent();

            var toolStripMenu = new ContextMenuStrip();
            var menuShow = new ToolStripMenuItem()
            {
                Text = "表示",
                Image = null,
            };
            menuShow.Click += (s, e) =>
            {
                this.Show();
            };
            var menuExit = new ToolStripMenuItem()
            {
                Text = "終了",
                Image = null,
            };
            menuExit.Click += (s, e) =>
            {
                this.Close();
            };
            toolStripMenu.Items.Add(menuShow);
            toolStripMenu.Items.Add(menuExit);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = toolStripMenu;

            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            _vm = new MainViewModel();
            this.DataContext = _vm;
            _vm.RequestClose += () =>
            {
                this.Hide();
            };
            _vm.RequestConfirm += (msg) =>
            {
                _isShowDialog = true;
                var result = MessageBox.Show(this, msg, "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                _isShowDialog = false;
                return result == MessageBoxResult.Yes;
            };
        }

        /// <summary>
        /// WPFのウィンドウのハンドル(HWND)が生成されたタイミングで呼ばれる
        /// → Win32 APIを使う準備ができるタイミング
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // WPFウィンドウからWin32のHWNDを取得
            var hwnd = new WindowInteropHelper(this).Handle;

            // クリップボード監視を開始（このウィンドウに通知が飛んでくるようになる）
            AddClipboardFormatListener(hwnd);

            // Windowsメッセージをフックする（WndProcを通して受け取る）
            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        /// <summary>
        /// Windowsから飛んでくるメッセージを監視して、クリップボードイベントを検知します。
        /// </summary>
        private IntPtr WndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    var text = Clipboard.GetText();
                    if (text == _latestText)
                    {
                        return IntPtr.Zero; ;
                    }

                    _latestText = text;
                    if (_isInternalCopy)
                    {
                        _isInternalCopy = false;
                        return IntPtr.Zero; ;
                    }

                    // 空チェック（画像コピーなどは空になることがある）
                    if (!string.IsNullOrEmpty(text))
                    {
                        using (var db = new AppDbContext())
                        {
                            ClipboardItem clipItem = new ClipboardItem { Text = text, CreatedAt = DateTime.Now };
                            db.ClipItems.Add(clipItem);
                            db.SaveChanges();
                            _vm.Items.Insert(0, clipItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return IntPtr.Zero;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // アプリ終了時には、クリップボード監視を削除して保存対象でないClipboardItemを削除します。
        protected override void OnClosed(EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            RemoveClipboardFormatListener(hwnd);

            using (var db = new AppDbContext())
            {
                var targets = db.ClipItems.Where(x => !x.Persisted).ToList();
                db.ClipItems.RemoveRange(targets);
                db.SaveChanges();
            }
            base.OnClosed(e);
        }

        // このアプリはフォーカスを失うとウィンドウを非表示にします。
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (_isShowDialog) return;
            this.Hide();
        }


        /// <summary>
        /// 選択されたクリップボードアイテムをコピー＆ペーストします。
        /// </summary>
        private async void SendSelectedItem()
        {
            var item = ClipList.SelectedItem as ClipboardItem;
            if (item == null) return;

            _isInternalCopy = true;
            
            Clipboard.SetText(item.Text);
            ClipList.SelectedIndex = 0;
            if(ClipList.ItemContainerGenerator.ContainerFromIndex(0) is ListViewItem firstItem)
            {
                firstItem.Focus();
            }
            this.WindowState = WindowState.Minimized;
            await Task.Delay(100);
            SendKeys.SendWait("^v");
        }


        private void ClipList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendSelectedItem();
            }
        }

        private void ClipList_MouseClick(object sender, MouseButtonEventArgs e)
        {
            SendSelectedItem();
        }
    }
    }