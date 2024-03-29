﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Animations;
using Rhizine.Core.Models;
using Rhizine.Core.Models.Interfaces;
using Rhizine.Core.Services.Interfaces;
using Rhizine.Core.ViewModels;
using Rhizine.WinUI.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Rhizine.WinUI.ViewModels;

public partial class ContentGridViewModel(INavigationService navigationService, ISampleDataService sampleDataService) : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService = navigationService;
    private readonly ISampleDataService _sampleDataService = sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = [];

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetContentGridDataAsync();
        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnItemClick(SampleOrder? clickedItem)
    {
        try
        {
            if (clickedItem != null)
            {
                _navigationService.NavigationSource.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.NavigateToAsync(typeof(ContentGridDetailViewModel).FullName!, clickedItem.OrderID);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}