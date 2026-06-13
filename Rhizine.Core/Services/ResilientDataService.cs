using Rhizine.Core.Services.Interfaces;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace Rhizine.Core.Services;

/// <summary>
/// Initializes a new instance of the ResilientDataService class.
/// </summary>
/// <param name="httpClient">The HttpClient instance for making HTTP requests.</param>
/// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
public partial class ResilientDataService : IDataService
{
    private readonly IHttpClientFactory _clientFactory;

    // Ideally, this key should come from a secure source such as Azure Key Vault or Windows Certificate Store
    private readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("your-32-length-secure-key-here!");

    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Asynchronously sends a GET request to the specified URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in the response.</typeparam>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Get, url, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a POST request with the provided item to the specified URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent and the response expected.</typeparam>
    /// <param name="url">The URL to send the POST request to.</param>
    /// <param name="item">The item to include in the POST request.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> PostAsync<T>(string url, T item, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync(HttpMethod.Post, url, item, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a PUT request with the provided item to the specified URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent and the response expected.</typeparam>
    /// <param name="url">The URL to send the PUT request to.</param>
    /// <param name="item">The item to include in the PUT request.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> PutAsync<T>(string url, T item, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync(HttpMethod.Put, url, item, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a DELETE request to the specified URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the data expected in the response.</typeparam>
    /// <param name="url">The URL to send the DELETE request to.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> DeleteAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Delete, url, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a request with the specified HTTP method to the given URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the response expected.</typeparam>
    /// <param name="method">The HTTP method to use for the request.</param>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> SendRequestAsync<T>(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await ProcessResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a request with the specified HTTP method and item to the given URL and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type of the item being sent and the response expected.</typeparam>
    /// <param name="method">The HTTP method to use for the request.</param>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="item">The item to include in the request.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> SendRequestAsync<T>(HttpMethod method, string url, T item, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(item)
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await ProcessResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// Processes the HTTP response, deserializing and returning the content if successful, or throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The type of the response expected.</typeparam>
    /// <param name="response">The HttpResponseMessage to process.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The deserialized response object of type T.</returns>
    public async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(_options, cancellationToken);
        }
        else
        {
            throw new HttpRequestException($"Error: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Encrypts a string using AES encryption with a predefined key and initialization vector.
    /// </summary>
    /// <param name="text">The plaintext string to encrypt.</param>
    /// <returns>The encrypted string, encoded in base64.</returns>
    public string EncryptString(string text)
    {
        using Aes aesAlg = Aes.Create();
        using var encryptor = aesAlg.CreateEncryptor(_encryptionKey, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }

        var iv = aesAlg.IV;
        var decryptedContent = msEncrypt.ToArray();
        var result = new byte[iv.Length + decryptedContent.Length];

        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts a string that was encrypted using AES encryption with the predefined key and initialization vector.
    /// </summary>
    /// <param name="cipherText">The base64-encoded encrypted string to decrypt.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public string DecryptString(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        var iv = new byte[aesAlg.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        using var decryptor = aesAlg.CreateDecryptor(_encryptionKey, iv);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}