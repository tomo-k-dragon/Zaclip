using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;
using Zaclip.Db;
using Zaclip.Service;
using Zaclip.Service.Interface;
using Microsoft.Extensions.Configuration;
using Zaclip.Settings;

namespace Zaclip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

            // DI コンテナを構築
            var services = new ServiceCollection();
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // データベース初期化
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
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
