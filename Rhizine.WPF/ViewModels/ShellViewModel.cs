using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls;
using Rhizine.Core.Services.Interfaces;
using Rhizine.Core.ViewModels;
using Rhizine.Displays.Pages;
using Rhizine.WPF.Helpers;
using Rhizine.WPF.Properties;
using Rhizine.WPF.Services.Interfaces;
using Rhizine.WPF.ViewModels.Pages;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Rhizine.Displays.Windows;

public partial class ShellViewModel : BaseViewModel
{
    #region Fields

    private readonly ILoggingService _loggingService;
    private readonly INavigationService _navigationService;
    [ObservableProperty]
    private IFlyoutService _flyoutService;

    [ObservableProperty]
    private HamburgerMenuItem _selectedMenuItem;

    [ObservableProperty]
    private HamburgerMenuItem _selectedOptionsMenuItem;

    #endregion Fields

    #region Constructors

    public ShellViewModel(INavigationService navigationService, IFlyoutService flyoutService, ILoggingService loggingService)
    {
        _navigationService = navigationService;

        FlyoutService = flyoutService;
        _loggingService = loggingService;
        if (_flyoutService != null)
        {
            _flyoutService.OnFlyoutOpened += FlyoutOpened;
            _flyoutService.OnFlyoutClosed += FlyoutClosed;
        }
    }

    #endregion Constructors

    #region Properties

    public Func<HamburgerMenuItem, bool> IsPageRestricted { get; } =
        (menuItem) => Attribute.IsDefined(menuItem.TargetPageType, typeof(Restricted));

    // TODO: Change the icons and titles for all HamburgerMenuItems here
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellLandingPage, Glyph = "\uE8A5", TargetPageType = typeof(LandingViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellWebViewPage, Glyph = "\uE8A5", TargetPageType = typeof(WebViewViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellDataGridPage, Glyph = "\uE8A5", TargetPageType = typeof(DataGridViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellContentGridPage, Glyph = "\uE8A5", TargetPageType = typeof(ContentGridViewModel) },
        new HamburgerMenuGlyphItem() { Label = Resources.ShellListDetailsPage, Glyph = "\uE8A5", TargetPageType = typeof(ListDetailsViewModel) },
        new HamburgerMenuGlyphItem() { Label = "Markdown", Glyph = "\uE8A5", TargetPageType = typeof(MarkdownViewModel) },
    ];

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } =
    [
        new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
    ];

    #endregion Properties

    #region Methods

    private static HamburgerMenuItem FindMenuItem(IEnumerable<HamburgerMenuItem> items, string viewModelName)
    {
        return items?.FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
    }

    [RelayCommand]
    private void FlyoutClosed(string flyout)
    {
        _loggingService.LogInformation("FlyoutOpened");
    }

    [RelayCommand]
    private void FlyoutOpened(string flyout)
    {
        _loggingService.LogInformation("FlyoutOpened");
    }

    [RelayCommand]
    private void GoBack()
    {
        _ = _navigationService.GoBackAsync();
    }

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _ = _navigationService.NavigateToAsync(targetViewModel.FullName);
        }
    }

    [RelayCommand]
    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    [RelayCommand]
    private void OnMenuItemInvoked() => NavigateTo(SelectedMenuItem.TargetPageType);

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        string viewModelName = e?.Content?.GetType()?.FullName;

        if (!string.IsNullOrEmpty(viewModelName))
        {
            var menuItem = FindMenuItem(MenuItems, viewModelName) ?? FindMenuItem(OptionMenuItems, viewModelName);

            if (menuItem != null)
            {
                SelectedMenuItem = menuItem;
                SelectedOptionsMenuItem = OptionMenuItems.Contains(menuItem) ? menuItem : null;

                GoBackCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [RelayCommand]
    private void OnOptionsMenuItemInvoked() => NavigateTo(SelectedOptionsMenuItem.TargetPageType);

    [RelayCommand]
    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
    }

    #endregion Methods
}