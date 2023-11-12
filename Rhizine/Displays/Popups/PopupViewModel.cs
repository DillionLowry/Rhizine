﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhizine.Displays.Popups;

public abstract partial class PopupViewModel : ObservableRecipient
{
    // Event to signal when the popup is closing.
    public event EventHandler Closing;

    // Command to close the popup.
    [RelayCommand]
    public void Close()
    {
        OnClosing();
        ClosePopup();
    }

    // Method to close the popup. Can be overridden in derived classes for custom close logic.
    protected virtual void ClosePopup()
    {
        WeakReferenceMessenger.Default.Send(new ClosePopupMessage(this));
    }

    // Method called when the popup is closing. Can be overridden in derived classes.
    protected virtual void OnClosing()
    {
        Closing?.Invoke(this, EventArgs.Empty);
    }

    // Method to show the popup. Must be implemented by derived classes.
    [RelayCommand]
    public abstract void Show();

    // Method to hide the popup. Can be overridden in derived classes.
    [RelayCommand]
    public virtual void Hide()
    {

    }
}
// Message class used to signal that a popup should close.
public class ClosePopupMessage
{
    public PopupViewModel Popup { get; }

    public ClosePopupMessage(PopupViewModel popup)
    {
        Popup = popup;
    }
}
