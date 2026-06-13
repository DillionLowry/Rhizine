using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rhizine.Core.Helpers;
using Rhizine.Core.Services.Interfaces;
using Rhizine.Displays.Pages;
using Rhizine.WPF.Models;
using Rhizine.WPF.Services.Interfaces;
using Rhizine.WPF.ViewModels.Pages;
using Rhizine.WPF.Views.Interfaces;
using Rhizine.WPF.Views.Pages;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Rhizine.WPF.Services;

/// <summary>
/// The ApplicationHostService class is responsible for the lifecycle management of the application,
/// including initialization, start-up processes, and graceful shutdown. It integrates various services
/// such as navigation, theme selection, data persistence, flyout management, toast notifications, and
/// identity services. This class implements the IHostedService interface, allowing it to be managed
/// by the host during application start-up and shutdown.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ApplicationHostService"/> class with required services.
/// </remarks>
/// <param name="serviceProvider">Service provider for dependency injection.</param>
/// <param name="activationHandlers">Collection of activation handlers for handling different types of app activations.</param>
/// <param name="navigationService">Service for managing navigation within the application.</param>
/// <param name="themeSelectorService">Service for selecting and applying application themes.</param>
/// <param name="persistAndRestoreService">Service for persisting and restoring application data.</param>
/// <param name="flyoutService">Service for managing flyout UI components.</param>
/// <param name="toastNotificationsService">Service for showing toast notifications.</param>
/// <param name="config">Configuration settings for the application.</param>
public class ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService<Frame, NavigationEventArgs> navigationService,
    IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IFlyoutService flyoutService,
    IToastNotificationsService toastNotificationsService, IPageService PageService, IOptions<AppConfig> config) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IFlyoutService _flyoutService = flyoutService;
    private readonly IPersistAndRestoreService _persistAndRestoreService = persistAndRestoreService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IEnumerable<IActivationHandler> _activationHandlers = activationHandlers;
    private readonly IIdentityService _identityService; // TODO
    private readonly IToastNotificationsService _toastNotificationsService = toastNotificationsService;
    private readonly IPageService _pageService = PageService;
    private readonly AppConfig _appConfig = config.Value;
    private IShellWindow _shellWindow;
    private bool _isInitialized;

    /// <summary>
    /// Starts the application's services asynchronously. This includes initializing the identity service,
    /// handling activation logic, and performing startup routines.
    /// </summary>
    /// <param name="cancellationToken">Token for handling task cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            Initialize();
            _isInitialized = true;
        }

        if (_identityService is not null)
        {
            _identityService.InitializeWithAadAndPersonalMsAccounts(_appConfig.IdentityClientId, "http://localhost");
            await _identityService.AcquireTokenSilentAsync();
        }

        await HandleActivationAsync(cancellationToken);
        Startup();
    }

    private void Initialize()
    {
        _persistAndRestoreService.RestoreData();
        _themeSelectorService.Initialize();
        _pageService.Register<LandingViewModel, LandingPage>();
        _pageService.Register<WebViewViewModel, WebViewPage>();
        _pageService.Register<DataGridViewModel, DataGridPage>();
        _pageService.Register<ContentGridViewModel, ContentGridPage>();
        _pageService.Register<ContentGridDetailViewModel, ContentGridDetailPage>();
        _pageService.Register<ListDetailsViewModel, ListDetailsPage>();
        _pageService.Register<SettingsViewModel, SettingsPage>();
        _pageService.Register<MarkdownViewModel, MarkdownViewerPage>();
    }

    private void Startup()
    {
        //_toastNotificationsService.ShowToastNotificationSample();
    }

    /// <summary>
    /// Stops the application's services asynchronously. This includes persisting application data.
    /// </summary>
    /// <param name="cancellationToken">Token for handling task cancellation.</param>
    /// <returns>A task representing the completion of the operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _persistAndRestoreService.PersistData();
        return Task.CompletedTask;
    }

    private async Task HandleActivationAsync(CancellationToken cancellationToken)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h?.CanHandle(null) == true);
        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(null);
        }

        if (!System.Windows.Application.Current.Windows.OfType<IShellWindow>().Any())
        {
            _shellWindow = _serviceProvider.GetRequiredService<IShellWindow>();
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _flyoutService.Initialize();
            _shellWindow.ShowWindow();
            await _navigationService.NavigateToAsync(typeof(LandingViewModel).FullName);
        }
    }

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType"/>. -or- null if there is no service object of type <paramref name="serviceType"/>.</returns>
    public object GetService(Type serviceType) => _serviceProvider?.GetService(serviceType);

    /// <summary>
    /// Get service of type <typeparamref name="TService"/> from the System.IServiceProvider.
    /// </summary>
    /// <typeparam name="TService">The type of service object to get.</typeparam>
    /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
    public TService? GetService<TService>() =>
        _serviceProvider is null ? default : _serviceProvider.GetService<TService>();
}