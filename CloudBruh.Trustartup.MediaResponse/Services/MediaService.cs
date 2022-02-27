using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudBruh.Trustartup.MediaResponse.Models;

namespace CloudBruh.Trustartup.MediaResponse.Services;

public class MediaService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = {new JsonStringEnumConverter()}
    };
    
    private readonly ILogger<MediaService> _logger;
    private readonly HttpClient _httpClient;

    public MediaService(ILogger<MediaService> logger, HttpClient httpClient, IConfiguration config)
    {
        _logger = logger;
        _httpClient = httpClient;
        
        _httpClient.BaseAddress = new Uri(config.GetValue<string>("Settings:MediaSystemUrl"));
    }
    
    public async Task<MediaRawDto?> GetMediaAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MediaRawDto>($"api/Media", SerializerOptions);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not retrieve media, {Exception}", e.Message);
            return null;
        }
    }
    
    public async Task<MediaRawDto?> GetMediumAsync(long id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MediaRawDto>($"api/Media/{id}", SerializerOptions);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not retrieve medium with id {Id}, {Exception}", id, e.Message);
            return null;
        }
    }
    
    public async Task<MediaRawDto?> GetMediumByFileNameAsync(string filename)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MediaRawDto>($"api/Media/file/{filename}", SerializerOptions);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not retrieve medium with filename {Filename}, {Exception}", filename, e.Message);
            return null;
        }
    }

    public async Task<MediaRawDto?> PostMedium(MediaRawDto dto)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Media", dto);
            return JsonSerializer.Deserialize<MediaRawDto>(await response.Content.ReadAsStreamAsync(), SerializerOptions);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not post the media, {Exception}", e.Message);
            return null;
        }
    }

    public async Task<Stream?> DownloadMedium(long id)
    {
        try
        {
            return await _httpClient.GetStreamAsync($"api/Media/{id}/download");
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not download medium with id {Id}, {Exception}", id, e.Message);
            return null;
        }
    }
    
    public async Task<MediaRawDto?> UploadMedium(long id, IFormFile file)
    {
        try
        {
            using var multipart = new MultipartFormDataContent();
            await using Stream stream = file.OpenReadStream();
            
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
            multipart.Add(fileContent, "file");
            
            HttpResponseMessage response = await _httpClient.PostAsync($"api/Media/{id}/upload", multipart);
            return JsonSerializer.Deserialize<MediaRawDto>(await response.Content.ReadAsStreamAsync(), SerializerOptions);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Could not download medium with id {Id}, {Exception}", id, e.Message);
            return null;
        }
    }
}