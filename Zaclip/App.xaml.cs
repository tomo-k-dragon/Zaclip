using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Zaclip.Db;
using Zaclip.Handlers;
using Zaclip.Services.AuthService;
using Zaclip.Services.Credential;
using Zaclip.Services.ServerClipboardService;
using Zaclip.Settings;
using Zaclip.States;
using Zaclip.ViewModel;
using Application = System.Windows.Application;

namespace Zaclip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

            // DI コンテナを構築
            var services = new ServiceCollection();
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            ConfigureServices(services);
            services.AddSingleton<TokenStore>();
            services.AddSingleton<SessionContext>();
            services.AddTransient<AuthenticationHandler>();

            services.AddTransient<ServerClipboardService>();
            services.AddTransient<CredentialService>();
            services.AddTransient<AuthService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
            ServiceProvider = services.BuildServiceProvider();

            // データベース初期化
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            var authService = ServiceProvider.GetRequiredService<AuthService>();
            await authService.AutoLoginAsync();

            var window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // HttpClient を登録
            services.AddHttpClient();

            // Service を登録
            services.AddScoped<IAuthService, AuthService>();

            // ViewModel を登録（必要に応じて追加）
            services.AddTransient<ViewModel.Settings.Contents.LoginDialogViewModel>();
        }
    }

}
