﻿namespace Rhizine.Displays.Interfaces;

public interface INavigationAware
{
    Task OnNavigatedTo(object parameter);

    Task OnNavigatedFrom();

    //Task OnNavigatedToAsync(object parameter);
    //Task OnNavigatedFromAsync();
}