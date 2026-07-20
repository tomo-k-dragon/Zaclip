using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Zaclip.Command;
using Zaclip.Services.AuthService;

namespace Zaclip.ViewModel.Settings.Contents
{
    internal class LoginDialogViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        public event Action? RequestClose;
        public event PropertyChangedEventHandler? PropertyChanged;

        public LoginDialogViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand<object?>(execute: LoginAsync);
        }

        private async void LoginAsync(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            try
            {
                IsLoading = true;
                var result = await _authService.LoginAsync(Email, Password);
                // ここでログイン成功後の処理を行う
                // 例: トークン保存、ウィンドウクローズ要求など
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                // エラーハンドリング
                // 実装は別途
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
