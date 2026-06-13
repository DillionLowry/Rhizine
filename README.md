Dependency Injection
Structured Logging
MVVM design pattern
Community toolkit ObservableProperty, RelayCommand
Sample Services/pages from Template Studio


## Rhizine.Core
implementation details for interfaces defined in Usings.cs of each project

## Rhizine.WPF
Shell design pattern
Frame based navigation

## Rhizine.WinUI
Shell design pattern
Frame based navigation

## Rhizine.MAUI
shell design pattern
shell navigation

Changes made to build exe on windows:
- .csproj added: ```<WindowsPackageType>None</WindowsPackageType>```

- LaunchSettings.json: ```"commandName": "MsixPackage"``` Changed to ```"commandName": "Project"```
