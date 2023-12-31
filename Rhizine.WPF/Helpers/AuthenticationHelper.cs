﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Rhizine.Core.Helpers;
using Rhizine.WPF.Properties;
using System.Windows;

namespace Rhizine.WPF.Helpers;

internal static class AuthenticationHelper
{
    internal static async Task ShowLoginErrorAsync(LoginResultType loginResult)
    {
        var metroWindow = Application.Current.MainWindow as MetroWindow;
        switch (loginResult)
        {
            case LoginResultType.NoNetworkAvailable:
                await metroWindow.ShowMessageAsync(Resources.DialogNoNetworkAvailableContent, Resources.DialogAuthenticationTitle);
                break;

            case LoginResultType.UnknownError:
                await metroWindow.ShowMessageAsync(Resources.DialogAuthenticationTitle, Resources.DialogStatusUnknownErrorContent);
                break;
        }
    }
}