using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Client;

/// <summary>
/// <see cref="IHospiKeeperApi"/> 的 HTTP 实现：统一处理 JSON、Bearer 令牌、
/// 404 可选结果与错误消息解析。
/// </summary>
public sealed class HospiKeeperApiClient : IHospiKeeperApi
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient _http;
    private string? _token;

    public HospiKeeperApiClient(HttpClient http) => _http = http;

    public UserDto? CurrentUser { get; private set; }

    public async Task<LoginResponse> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<LoginResponse>(
            "api/auth/login", new LoginRequest(username, password), cancellationToken);

        _token = response.Token;
        CurrentUser = response.User;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return response;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_token is not null)
            {
                await PostNoContentAsync("api/auth/logout", null, cancellationToken);
            }
        }
        finally
        {
            _token = null;
            CurrentUser = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<UserDto> GetMeAsync(CancellationToken cancellationToken = default)
    {
        CurrentUser = await GetAsync<UserDto>("api/auth/me", cancellationToken);
        return CurrentUser;
    }

    public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        => PostNoContentAsync("api/auth/change-password", request, cancellationToken);

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<UserDto>>("api/users", cancellationToken);

    public async Task<UserDto> CreateUserAsync(AccountInput input, CancellationToken cancellationToken = default)
        => await PostAsync<UserDto>("api/users", input, cancellationToken);

    public Task UpdateUserAsync(int userId, AccountInput input, CancellationToken cancellationToken = default)
        => PutNoContentAsync($"api/users/{userId}", input, cancellationToken);

    public Task SetUserStatusAsync(int userId, AccountStatus status, CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/users/{userId}/status", new AccountStatusRequest(status), cancellationToken);

    public Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
        => DeleteNoContentAsync($"api/users/{userId}", cancellationToken);

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/rooms" + Query(
            ("floor", floor?.ToString()),
            ("typeId", typeId?.ToString()),
            ("status", status?.ToString()));
        return await GetAsync<List<RoomDto>>(url, cancellationToken);
    }

    public async Task<IReadOnlyList<RoomDto>> GetFreeRoomsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<RoomDto>>("api/rooms/free", cancellationToken);

    public async Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<int>>("api/rooms/floors", cancellationToken);

    public async Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<RoomTypeDto>>("api/room-types", cancellationToken);

    public async Task<RoomDto> CreateRoomAsync(RoomInput input, CancellationToken cancellationToken = default)
        => await PostAsync<RoomDto>("api/rooms", input, cancellationToken);

    public Task UpdateRoomAsync(int roomId, RoomInput input, CancellationToken cancellationToken = default)
        => PutNoContentAsync($"api/rooms/{roomId}", input, cancellationToken);

    public Task DeleteRoomAsync(int roomId, CancellationToken cancellationToken = default)
        => DeleteNoContentAsync($"api/rooms/{roomId}", cancellationToken);

    public Task MarkCleaningCompleteAsync(int roomId, CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/rooms/{roomId}/cleaning-complete", null, cancellationToken);

    public Task RestoreRoomAsync(int roomId, CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/rooms/{roomId}/restore", null, cancellationToken);

    public Task SetMaintenanceAsync(
        int roomId,
        RoomStatus status,
        string? note,
        CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/rooms/{roomId}/maintenance", new MaintenanceRequest(status, note), cancellationToken);

    public async Task<RoomTypeDto> CreateRoomTypeAsync(RoomTypeInput input, CancellationToken cancellationToken = default)
        => await PostAsync<RoomTypeDto>("api/room-types", input, cancellationToken);

    public Task UpdateRoomTypeAsync(int roomTypeId, RoomTypeInput input, CancellationToken cancellationToken = default)
        => PutNoContentAsync($"api/room-types/{roomTypeId}", input, cancellationToken);

    public Task DeleteRoomTypeAsync(int roomTypeId, CancellationToken cancellationToken = default)
        => DeleteNoContentAsync($"api/room-types/{roomTypeId}", cancellationToken);

    public async Task<IReadOnlyList<BoardRoom>> GetBoardAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<BoardRoom>>("api/board", cancellationToken);

    public async Task<FrontDeskCounts> GetCountsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<FrontDeskCounts>("api/board/counts", cancellationToken);

    public async Task<IReadOnlyList<StayInfo>> GetLivingStaysAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/stays" + Query(("keyword", keyword));
        return await GetAsync<List<StayInfo>>(url, cancellationToken);
    }

    public Task<StayInfo?> GetStayAsync(int checkInId, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<StayInfo>($"api/stays/{checkInId}", cancellationToken);

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/reservations" + Query(("status", status?.ToString()));
        return await GetAsync<List<ReservationDto>>(url, cancellationToken);
    }

    public Task<ReservationDto?> GetReservationAsync(int reservationId, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<ReservationDto>($"api/reservations/{reservationId}", cancellationToken);

    public async Task<CheckInRecordDto> CheckInAsync(CheckInRequest request, CancellationToken cancellationToken = default)
        => await PostAsync<CheckInRecordDto>("api/checkins", request, cancellationToken);

    public async Task<BillingResult> QuoteCheckoutAsync(
        int checkInId,
        CheckoutQuoteRequest request,
        CancellationToken cancellationToken = default)
        => await PostAsync<BillingResult>($"api/checkins/{checkInId}/quote", request, cancellationToken);

    public async Task<CheckoutBillDto> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
        => await PostAsync<CheckoutBillDto>("api/checkouts", request, cancellationToken);

    public Task AddExtraChargeAsync(
        int checkInId,
        string item,
        decimal amount,
        CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/checkins/{checkInId}/extras", new ExtraChargeInput(item, amount), cancellationToken);

    public async Task<ReservationDto> CreateReservationAsync(
        ReservationRequest request,
        CancellationToken cancellationToken = default)
        => await PostAsync<ReservationDto>("api/reservations", request, cancellationToken);

    public Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default)
        => PostNoContentAsync($"api/reservations/{reservationId}/cancel", null, cancellationToken);

    public async Task<IReadOnlyList<CheckInRecordDto>> QueryCheckInsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var url = "api/records/checkins" + Query(
            ("roomNo", query.RoomNo),
            ("keyword", query.Keyword),
            ("from", query.From?.ToString("yyyy-MM-dd")),
            ("to", query.To?.ToString("yyyy-MM-dd")));
        return await GetAsync<List<CheckInRecordDto>>(url, cancellationToken);
    }

    public async Task<IReadOnlyList<CheckoutBillDto>> QueryBillsAsync(
        RecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var url = "api/records/bills" + Query(
            ("roomNo", query.RoomNo),
            ("keyword", query.Keyword),
            ("from", query.From?.ToString("yyyy-MM-dd")),
            ("to", query.To?.ToString("yyyy-MM-dd")));
        return await GetAsync<List<CheckoutBillDto>>(url, cancellationToken);
    }

    public Task<CheckInRecordDto?> GetCheckInAsync(int checkInId, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<CheckInRecordDto>($"api/records/checkins/{checkInId}", cancellationToken);

    public Task<CheckoutBillDto?> GetBillAsync(int billId, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<CheckoutBillDto>($"api/records/bills/{billId}", cancellationToken);

    public async Task<IReadOnlyList<OperationLogDto>> QueryLogsAsync(
        string? action = null,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var url = "api/logs" + Query(("action", action), ("keyword", keyword));
        return await GetAsync<List<OperationLogDto>>(url, cancellationToken);
    }

    public async Task<ReportSnapshot> GetReportSnapshotAsync(CancellationToken cancellationToken = default)
        => await GetAsync<ReportSnapshot>("api/reports/snapshot", cancellationToken);

    public async Task<RevenueRangeResult> GetRevenueRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var url = "api/reports/revenue" + Query(
            ("start", start.ToString("yyyy-MM-dd")),
            ("end", end.ToString("yyyy-MM-dd")));
        return await GetAsync<RevenueRangeResult>(url, cancellationToken);
    }

    // ------------------------------------------------------------------

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        => await SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);

    private async Task<T?> GetOrDefaultAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string url, object? body, CancellationToken cancellationToken)
        => await SendAsync<T>(CreateRequest(HttpMethod.Post, url, body), cancellationToken);

    private Task PostNoContentAsync(string url, object? body, CancellationToken cancellationToken)
        => SendNoContentAsync(CreateRequest(HttpMethod.Post, url, body), cancellationToken);

    private Task PutNoContentAsync(string url, object? body, CancellationToken cancellationToken)
        => SendNoContentAsync(CreateRequest(HttpMethod.Put, url, body), cancellationToken);

    private Task DeleteNoContentAsync(string url, CancellationToken cancellationToken)
        => SendNoContentAsync(new HttpRequestMessage(HttpMethod.Delete, url), cancellationToken);

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        return request;
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new ApiException("服务端返回内容为空。", response.StatusCode);
    }

    private async Task SendNoContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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

    private static string Query(params (string Key, string? Value)[] values)
    {
        var parts = values
            .Where(v => !string.IsNullOrWhiteSpace(v.Value))
            .Select(v => $"{Uri.EscapeDataString(v.Key)}={Uri.EscapeDataString(v.Value!)}")
            .ToList();
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
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
