using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public sealed record UserDto(
    int Id,
    string Username,
    string RealName,
    UserRole Role,
    AccountStatus Status,
    DateTime? LastLogin,
    string DisplayName,
    bool IsAdmin);

public sealed record ChangePasswordRequest(string OldPassword, string NewPassword, string ConfirmPassword);

public sealed record AccountStatusRequest(AccountStatus Status);

public sealed record MaintenanceRequest(RoomStatus Status, string? Note);

public sealed record CheckoutQuoteRequest(DateTime ActualOutTime, decimal Damage, BillingMode Mode);

public sealed record ApiErrorResponse(string Message);

public static class UserMapping
{
    public static UserDto ToDto(this AppUser user) => new(
        user.Id,
        user.Username,
        user.RealName,
        user.Role,
        user.Status,
        user.LastLogin,
        user.DisplayName,
        user.IsAdmin);
}
