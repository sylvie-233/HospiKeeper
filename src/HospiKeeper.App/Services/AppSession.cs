using CommunityToolkit.Mvvm.ComponentModel;
using HospiKeeper.Contracts;

namespace HospiKeeper.App.Services;

/// <summary>管理 App 当前登录的老板账号。</summary>
public sealed class AppSession : ObservableObject
{
    private UserDto? _user;

    public UserDto? User => _user;

    public bool IsAuthenticated => _user is not null;

    public bool IsAdmin => _user?.IsAdmin == true;

    public string DisplayName => _user?.DisplayName ?? "未登录";

    public string Initial => string.IsNullOrWhiteSpace(_user?.RealName)
        ? "栖"
        : _user.RealName[..1];

    public void SignIn(UserDto user)
    {
        _user = user;
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Initial));
    }

    public void SignOut()
    {
        _user = null;
        OnPropertyChanged(nameof(User));
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Initial));
    }
}
