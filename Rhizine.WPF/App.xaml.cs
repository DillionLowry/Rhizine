using CommunityToolkit.WinUI.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rhizine.Core.Helpers;
using Rhizine.Core.Services;
using Rhizine.Core.Services.Interfaces;
using Rhizine.Displays.Pages;
using Rhizine.Displays.Windows;
using Rhizine.WPF.Helpers;
using Rhizine.WPF.Models;
using Rhizine.WPF.Services;
using Rhizine.WPF.Services.Interfaces;
using Rhizine.WPF.ViewModels;
using Rhizine.WPF.ViewModels.Pages;
using Rhizine.WPF.Views.Interfaces;
using Rhizine.WPF.Views.Pages;
using Rhizine.WPF.Views.Windows;
using Serilog;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Rhizine;

public partial class App : Application
{
    private IHost _host;

    public T GetService<T>() where T : class => _host.Services.GetService(typeof(T)) as T;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // NOTE: Entry Assembly logic removed from app configure when building as single exe
        // TODO: make single exe .ConfigureAppConfiguration optional
        /* Looks for the .dll which will fail when embedded in .exe
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(appLocation);
            })
        */
        /*
        // https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += (toastArgs) =>
        {
            _ = Current.Dispatcher.Invoke(async () =>
            {
                var config = GetService<IConfiguration>();
                config[ToastNotificationActivationHandler.ActivationArguments] = toastArgs.Argument;
                await _host.StartAsync();
            }).ConfigureAwait(true);
        };

        // TODO: Register arguments to use on App initialization
        var activationArgs = new Dictionary<string, string>
        {
            { ToastNotificationActivationHandler.ActivationArguments, string.Empty }
        };
        */
        // Configure Serilog

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build())
            
            .CreateLogger();

        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Use Serilog for logging
            .ConfigureServices(ConfigureServices)
            .Build();
        /*
        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            // ToastNotificationActivator code will run after this completes and will show a window if necessary.
            return;
        }
        */
        await _host.StartAsync();
    }

    // When adding a service, add it below and in Services\ApplicationHostService.cs
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // App Host
        // see C:\Users\Main\source\repos\Rhizine\Rhizine.WPF\Services\ApplicationHostService.cs
        services.AddHostedService<ApplicationHostService>();
        //services.AddLogging(configure => configure.AddConsole()); // default console

        // Activation Handlers
        services.AddSingleton<IActivationHandler, ToastNotificationActivationHandler>();

        // Services
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<ISampleDataService, SampleDataService>();    
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IFlyoutService, FlyoutService>();
        services.AddSingleton<IPopupService, PopupService>();
        services.AddSingleton<IWebViewService, WebViewService>();

        services.AddSingleton<IToastNotificationsService, ToastNotificationsService>();

        // Views and ViewModels
        services.AddTransient<IShellWindow, ShellWindow>();
        services.AddTransient<ShellViewModel>();

        services.AddTransient<LandingViewModel>();
        services.AddTransient<LandingPage>();

        services.AddTransient<WebViewViewModel>();
        services.AddTransient<WebViewPage>();

        services.AddTransient<DataGridViewModel>();
        services.AddTransient<DataGridPage>();

        services.AddTransient<ContentGridViewModel>();
        services.AddTransient<ContentGridPage>();

        services.AddTransient<ContentGridDetailViewModel>();
        services.AddTransient<ContentGridDetailPage>();

        services.AddTransient<ListDetailsViewModel>();
        services.AddTransient<ListDetailsPage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddTransient<MarkdownViewModel>();
        services.AddTransient<MarkdownViewerPage>();

        services.AddTransient<IShellDialogWindow, ShellDialogWindow>();
        services.AddTransient<ShellDialogViewModel>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }

    protected async void OnExit(object sender, ExitEventArgs e)
    {
        await Log.CloseAndFlushAsync();

        await _host.StopAsync();
        _host.Dispose();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        GetService<ILoggingService>().HandleGlobalException(e.Exception);
        e.Handled = true;
    }
}