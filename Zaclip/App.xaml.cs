using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.Data;
using System.Windows;
using Zaclip.Db;
using Zaclip.Handlers;
using Zaclip.Services.AuthService;
using Zaclip.Services.ClipboardItemsService;
using Zaclip.Services.Credential;
using Zaclip.Services.ServerClipboardService;
using Zaclip.Settings;
using Zaclip.States;
using Zaclip.View;
using Zaclip.View.Settings.Contents;
using Zaclip.ViewModel;
using Zaclip.ViewModel.Settings.Contents;
using Zaclip.ViewModels.Controls;
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
            
            var services = new ServiceCollection();
            ConfigureServices(services, BuildConfiguration());
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

        private IConfiguration BuildConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

        // <summary>アプリケーションの起動設定を行う</summary>
        private void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.Configure<ApiSettings>(config.GetSection("ApiSettings"));
            RegisterServices(services);
            RegisterHttpClients(services);
            RegisterViews(services);
        }

        /// <summary>アプリケーションで使用する状態保持クラスとサービスを登録する</summary>
        private void RegisterServices(IServiceCollection services)
        {
            // 状態保持のシングルトンインスタンスを登録
            services.AddSingleton<TokenStore>();
            services.AddSingleton<SessionContext>();
            // サービスの登録
            services.AddTransient<AuthenticationHandler>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ICredentialService, CredentialService>();
            services.AddTransient<IServerClipboardService, ServerClipboardService>();
        }

        /// <summary>サービスへHttpClientの設定を行う</summary>
        private void RegisterHttpClients(IServiceCollection services)
        {
            services.AddHttpClient<AuthService>(
                (sp, client) => client.BaseAddress = GetBaseUri(sp));
            services.AddHttpClient<ServerClipboardService>(
                (sp, client) => client.BaseAddress = GetBaseUri(sp))
                .AddHttpMessageHandler<AuthenticationHandler>();

            Uri GetBaseUri(IServiceProvider sp) =>
                new Uri(sp.GetRequiredService<IOptions<ApiSettings>>().Value.BaseUrl);
        }

        // <summary>アプリケーションで使用するビューとViewModelを登録する</summary>
        private void RegisterViews(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AccountIconViewModel>();
            services.AddTransient<SettingWindow>();
            services.AddTransient<LoginDialog>();
            services.AddTransient<LoginDialogViewModel>();
        }
    }

}
