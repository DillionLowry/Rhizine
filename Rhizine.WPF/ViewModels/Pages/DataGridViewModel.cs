﻿using Rhizine.Core.Models;
using Rhizine.Core.Services.Interfaces;
using Rhizine.Core.ViewModels;
using System.Collections.ObjectModel;

namespace Rhizine.WPF.ViewModels.Pages;

public class DataGridViewModel : BaseViewModel
{
    private readonly ISampleDataService _sampleDataService;
    private readonly ILoggingService _loggingService;

    public ObservableCollection<SampleOrder> Source { get; } = [];

    public DataGridViewModel(ISampleDataService sampleDataService, ILoggingService loggingService)
    {
        _sampleDataService = sampleDataService;
        _loggingService = loggingService;
    }

    public override async void OnNavigatedTo(object parameter)
    {
        try
        {
            Source.Clear();

            // Replace this with your actual data
            var data = await _sampleDataService.GetGridDataAsync();

            foreach (var item in data)
            {
                Source.Add(item);
            }
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync(ex, "Error while navigating to Data Grid");
        }
    }


}