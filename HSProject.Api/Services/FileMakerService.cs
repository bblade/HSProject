using HSProject.Api.Models;
using HSProject.Api.Options;
using Microsoft.Extensions.Options;

using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace HSProject.Api.Services;
public class FileMakerService(IOptions<FileMakerOptions> fileMakerOptions, IHttpClientFactory httpClientFactory) {

    private static readonly JsonSerializerOptions jsonSerializerOptions
        = new() { PropertyNameCaseInsensitive = true };

    private readonly FileMakerOptions _fileMakerOptions = fileMakerOptions.Value;

    private async Task<string?> LoginAsync() {
        string basicAuth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_fileMakerOptions.Username}:{_fileMakerOptions.Password}"));

        using HttpClient client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        StringContent content = new("{}", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(_fileMakerOptions.SessionsUrl, content);

        string responseContent = await response.Content.ReadAsStringAsync();
        FilemakerSessionInfo? sessionInfo =
            JsonSerializer.Deserialize<FilemakerSessionInfo>(responseContent, jsonSerializerOptions);

        return sessionInfo?.Response?.Token;
    }

    public async Task<string> SubmitAsync() {
        string? token = await LoginAsync()
            ?? throw new Exception("Failed to log in");

        return token;
    }
}
