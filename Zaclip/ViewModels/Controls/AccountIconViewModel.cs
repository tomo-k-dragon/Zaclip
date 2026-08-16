using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Zaclip.Command;
using Zaclip.Services.AuthService;
using Zaclip.States;

namespace Zaclip.ViewModels.Controls
{
    public class AccountIconViewModel : INotifyPropertyChanged
    {
        private readonly SessionContext _session;
        private readonly TokenStore _tokenStore;
        private readonly IAuthService _authService;

        public AccountIconViewModel(SessionContext session, IAuthService authService, TokenStore tokenStore)
        {
            _session = session;
            _authService = authService;
            _tokenStore = tokenStore;

            LoginCommand = new RelayCommand(OnLogin);
            LogoutCommand = new RelayCommand(OnLogout);
            OpenAccountSettingCommand = new RelayCommand(OnOpenAccountSetting);
            _session.SessionChanged += () => Refresh();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// ログイン状態に応じたアイコン
        /// </summary>
        public PackIconKind Icon =>
            _session.IsLoggedIn
                ? PackIconKind.AccountCheck
                : PackIconKind.AccountOff;

        /// <summary>
        /// ツールチップ
        /// </summary>
        public string ToolTip =>
            _session.IsLoggedIn
                ? _session.UserEmail!
                : "ログインしていません。";

        public Visibility LoggedInVisibility =>
            _session.IsLoggedIn
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility LoggedOutVisibility =>
            _session.IsLoggedIn
                ? Visibility.Collapsed
                : Visibility.Visible;

        public RelayCommand LoginCommand { get; }

        public RelayCommand LogoutCommand { get; }

        public RelayCommand OpenAccountSettingCommand { get; }

        public event Action? LoginRequested;

        public event Action? AccountSettingRequested;

        private void OnLogin()
        {
            LoginRequested?.Invoke();
        }

        private async void OnLogout()
        {
            if(!_session.IsLoggedIn || _tokenStore.RefreshToken == null)
                return;

            await _authService.LogoutAsync(_tokenStore.RefreshToken);
        }

        private void OnOpenAccountSetting()
        {
            AccountSettingRequested?.Invoke();
        }

        /// <summary>
        /// セッション情報変更後に呼び出す
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(ToolTip));
            OnPropertyChanged(nameof(LoggedInVisibility));
            OnPropertyChanged(nameof(LoggedOutVisibility));
        }
    }
}