﻿using MahApps.Metro.Controls;
using Rhizine.Displays.Pages;

namespace Rhizine.Displays.Flyouts
{
    public class FlyoutSettingsViewModel : FlyoutViewModel
    {
        public FlyoutSettingsViewModel()
        {
            this.Header = "settings";
            this.Position = Position.Left;
        }
    }
}