using Microsoft.Extensions.Options;
using Rhizine.Core.Models;
using Rhizine.Core.Services.Interfaces;
using System.Text.Json;

namespace Rhizine.Core.Services;

/// <summary>
/// Provides services for managing local application settings, including reading from and saving to a local JSON settings file.
/// This service integrates file handling, logging, and caching mechanisms to manage settings efficiently.
/// </summary>
/// <remarks>
/// This class provides functionalities to read from and save settings to a local JSON file.
/// It uses <see cref="IFileService"/> for file operations and <see cref="ILoggingService"/> for logging errors.
/// Settings are stored in a a cache via <see cref="ICachingService"/>.
/// </remarks>
public class LocalSettingsService : ILocalSettingsService
{
    private const string DefaultApplicationDataFolder = "App/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly ICachingService _cachingService;

    private readonly string _applicationDataFolder;
    private readonly string _localSettingsFile;

    private bool _isInitialized;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSettingsService"/> class with specified services and configuration options.
    /// </summary>
    /// <param name="fileService">Service for file operations.</param>
    /// <param name="loggingService">Service for logging.</param>
    /// <param name="cachingService">Service for caching settings.</param>
    /// <param name="options">Configuration options that specify settings file paths.</param>
    public LocalSettingsService(IFileService fileService, ILoggingService loggingService, ICachingService cachingService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _applicationDataFolder = Path.Combine(localApplicationData, options.Value.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        _localSettingsFile = options.Value.LocalSettingsFile ?? DefaultLocalSettingsFile;
    }

    /// <summary>
    /// Ensures the service is initialized by loading settings from the file and caching them.
    /// </summary>
    /// <remarks>
    /// This method checks if the service is already initialized to avoid redundant operations.
    /// It reads the settings file, deserializes it, and populates the settings dictionary.
    /// </remarks>
    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            var fileContents = await _fileService.ReadAsync<string>(_applicationDataFolder, _localSettingsFile);
            if (!string.IsNullOrEmpty(fileContents))
            {
                var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(fileContents, JsonOptions);
                foreach (var setting in settings)
                {
                    await _cachingService.SetAsync(setting.Key, setting.Value);
                }
            }
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Reads the specified setting from the cache. Initializes the service if not already done.
    /// </summary>
    /// <typeparam name="T">The type of the setting to read.</typeparam>
    /// <param name="key">The key identifying the setting.</param>
    /// <returns>The value of the setting if found; otherwise, null.</returns>
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync();
        return await _cachingService.GetAsync<T>(key);
    }

    /// <summary>
    /// Saves the specified setting by updating the local settings file and caching the value. Initializes the service if not already done.
    /// </summary>
    /// <typeparam name="T">The type of the setting to save.</typeparam>
    /// <param name="key">The key identifying the setting.</param>
    /// <param name="value">The value of the setting to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        await InitializeAsync();

        // Update the setting in the JSON file
        var settings = new Dictionary<string, object> { { key, value } };
        var serializedSettings = JsonSerializer.Serialize(settings, JsonOptions);
        await _fileService.SaveAsync(_applicationDataFolder, _localSettingsFile, serializedSettings);

        // Update the cache
        await _cachingService.SetAsync(key, value);
    }
}