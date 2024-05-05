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

        using HttpClient client = httpClientFactory.CreateClient("FM");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        StringContent content = new("{}", new MediaTypeHeaderValue("application/json"));

        HttpResponseMessage response = await client.PostAsync(_fileMakerOptions.SessionsUrl, content);

        string responseContent = await response.Content.ReadAsStringAsync();
        FilemakerSessionDto? sessionInfo =
            JsonSerializer.Deserialize<FilemakerSessionDto>(responseContent, jsonSerializerOptions);

        return sessionInfo?.Response?.Token;
    }

    public async Task<string?> SubmitAsync(IEnumerable<LookupDto> lookupList) {
        string? token = await LoginAsync()
            ?? throw new Exception("Failed to log in");

        using HttpClient client = httpClientFactory.CreateClient("FM");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        string listJson = JsonSerializer.Serialize(lookupList);

        FileMakerRecordDto record = new(listJson, "process-post");

        string recordJson = JsonSerializer.Serialize(record);

        StringContent content = new(recordJson, new MediaTypeHeaderValue("application/json"));

        HttpResponseMessage response = await client.PostAsync(_fileMakerOptions.SubmitUrl, content);

        string responseContent = await response.Content.ReadAsStringAsync();
        FileMakerResponseDto? result = JsonSerializer.Deserialize<FileMakerResponseDto>(responseContent, jsonSerializerOptions);
        return result?.Response?.ScriptResult;
    }
}
