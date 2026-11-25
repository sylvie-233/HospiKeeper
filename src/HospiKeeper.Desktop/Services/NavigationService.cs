namespace HospiKeeper.Desktop.Services;

public static class PageKeys
{
    public const string Dashboard = "dashboard";
    public const string CheckIn = "checkin";
    public const string Reservations = "reservations";
    public const string Stay = "stay";
    public const string Records = "records";
    public const string Report = "report";
    public const string Logs = "logs";
    public const string Rooms = "rooms";
    public const string RoomTypes = "roomtypes";
    public const string Accounts = "accounts";
}

/// <summary>页面间导航（采用事件而非直接引用 MainViewModel，避免循环依赖）。</summary>
public sealed class NavigationService
{
    public event Action<string, object?>? Requested;

    public void Navigate(string pageKey, object? argument = null)
        => Requested?.Invoke(pageKey, argument);
}
