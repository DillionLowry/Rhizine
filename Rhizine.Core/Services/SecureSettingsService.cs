using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Rhizine.Core.Models;
using Rhizine.Core.Services.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rhizine.Core.Services;

/// <summary>
/// Service for managing local settings in a JSON file.
/// </summary>
/// <remarks>
/// This class provides functionalities to read from and save settings to a local JSON file.
/// It uses <see cref="IFileService"/> for file operations and <see cref="ILoggingService"/> for logging errors.
/// Settings are stored in a concurrent dictionary and are lazily loaded upon first use.
/// </remarks>
public class SecureSettingsService : ILocalSettingsService
{
    private const string DefaultApplicationDataFolder = "App/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";
    private readonly string EncryptionKey = "your-very-secure-key-here";

    private readonly IFileService _fileService;
    private readonly ILoggingService _loggingService;
    private readonly IMemoryCache _memoryCache;
    private readonly LocalSettingsOptions _options;

    private readonly string _applicationDataFolder;
    private readonly string _localSettingsFile;
    private bool _isInitialized;

    public SecureSettingsService(IFileService fileService, ILoggingService loggingService, IMemoryCache memoryCache, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        _localSettingsFile = _options.LocalSettingsFile ?? DefaultLocalSettingsFile;
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            await LoadSettingsAsync();
        }
    }

    private async Task LoadSettingsAsync()
    {
        var encryptedSettings = await _fileService.ReadAsync<string>(_applicationDataFolder, _localSettingsFile);
        if (!string.IsNullOrEmpty(encryptedSettings))
        {
            try
            {
                var jsonSettings = Decrypt(encryptedSettings);
                var settings = JsonSerializer.Deserialize<ConcurrentDictionary<string, object>>(jsonSettings);
                _memoryCache.Set("Settings", settings);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex,"Failed to decrypt or deserialize settings.");
            }
        }
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await EnsureInitializedAsync();

        if (_memoryCache.TryGetValue("Settings", out ConcurrentDictionary<string, object> settings) && settings.TryGetValue(key, out var value))
        {
            return value is JsonElement element ? element.Deserialize<T>() : (T)value;
        }

        return default;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        await EnsureInitializedAsync();

        if (_memoryCache.TryGetValue("Settings", out ConcurrentDictionary<string, object> settings))
        {
            settings[key] = value;
            var jsonSettings = JsonSerializer.Serialize(settings);
            var encryptedSettings = Encrypt(jsonSettings);
            await _fileService.SaveAsync(_applicationDataFolder, _localSettingsFile, encryptedSettings);
        }
    }

    private string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plaintext);
        }

        var iv = aes.IV;
        var encrypted = ms.ToArray();
        var result = new byte[iv.Length + encrypted.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    private string Decrypt(string encryptedText)
    {
        var combined = Convert.FromBase64String(encryptedText);
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);

        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherText = new byte[combined.Length - iv.Length];

        Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(combined, iv.Length, cipherText, 0, cipherText.Length);

        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}