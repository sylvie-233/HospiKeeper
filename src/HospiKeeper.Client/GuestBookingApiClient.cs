using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HospiKeeper.Contracts;

namespace HospiKeeper.Client;

/// <summary>游客预订公开接口的 HTTP 实现。</summary>
public sealed class GuestBookingApiClient : IGuestBookingApi
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient _http;

    public GuestBookingApiClient(HttpClient http) => _http = http;

    public async Task<HotelSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        => await GetAsync<HotelSummaryDto>("api/public/summary", cancellationToken);

    public async Task<GuestAvailabilityResult> GetAvailabilityAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/public/room-types?checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}";
        return await GetAsync<GuestAvailabilityResult>(url, cancellationToken);
    }

    public async Task<IReadOnlyList<GuestReservationDto>> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/public/reservations?phone={Uri.EscapeDataString(phone)}";
        return await GetAsync<List<GuestReservationDto>>(url, cancellationToken);
    }

    public async Task<GuestReservationDto> CreateAsync(
        GuestBookingRequest request,
        CancellationToken cancellationToken = default)
        => await PostAsync<GuestReservationDto>("api/public/reservations", request, cancellationToken);

    public Task CancelAsync(
        string resNo,
        string phone,
        CancellationToken cancellationToken = default)
        => PostNoContentAsync(
            $"api/public/reservations/{Uri.EscapeDataString(resNo)}/cancel",
            new CancelGuestBookingRequest(phone),
            cancellationToken);

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new ApiException("服务端返回内容为空。", response.StatusCode);
    }

    private async Task<T> PostAsync<T>(string url, object body, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsync(
            url,
            new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json"),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new ApiException("服务端返回内容为空。", response.StatusCode);
    }

    private async Task PostNoContentAsync(
        string url,
        object body,
        CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsync(
            url,
            new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json"),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        string message;
        try
        {
            message = JsonSerializer.Deserialize<ApiErrorResponse>(raw, JsonOptions)?.Message ?? raw;
        }
        catch (JsonException)
        {
            message = raw;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"请求失败（{(int)response.StatusCode}）。";
        }

        throw new ApiException(message, response.StatusCode);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
