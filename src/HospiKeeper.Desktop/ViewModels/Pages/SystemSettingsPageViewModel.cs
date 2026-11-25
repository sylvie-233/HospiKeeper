using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HospiKeeper.Client;
using HospiKeeper.Contracts;
using HospiKeeper.Desktop.Messages;
using HospiKeeper.Desktop.Services;
using Sylvie233.UI.Icons;

namespace HospiKeeper.Desktop.ViewModels.Pages;

/// <summary>特色卡片编辑行。</summary>
public sealed partial class SiteFeatureEditViewModel : ObservableObject
{
    public SiteFeatureEditViewModel(SiteFeature feature, Action<SiteFeatureEditViewModel> remove)
    {
        _icon = feature.Icon;
        _title = feature.Title;
        _description = feature.Description;
        RemoveCommand = new RelayCommand(() => remove(this));
    }

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _description;

    public IRelayCommand RemoveCommand { get; }

    public SiteFeature ToContract() => new()
    {
        Icon = Icon,
        Title = Title,
        Description = Description
    };
}

/// <summary>入住须知编辑行。</summary>
public sealed partial class SiteNoticeEditViewModel : ObservableObject
{
    public SiteNoticeEditViewModel(SiteNotice notice, Action<SiteNoticeEditViewModel> remove)
    {
        _title = notice.Title;
        _line1 = notice.Line1;
        _line2 = notice.Line2;
        RemoveCommand = new RelayCommand(() => remove(this));
    }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _line1;

    [ObservableProperty]
    private string _line2;

    public IRelayCommand RemoveCommand { get; }

    public SiteNotice ToContract() => new()
    {
        Title = Title,
        Line1 = Line1,
        Line2 = Line2
    };
}

/// <summary>系统配置页面：编辑游客 Web 与桌面端静态信息，保存到 Redis。</summary>
public sealed partial class SystemSettingsPageViewModel : PageViewModelBase
{
    private readonly Action<SiteFeatureEditViewModel> _removeFeature;
    private readonly Action<SiteNoticeEditViewModel> _removeNotice;

    public SystemSettingsPageViewModel(
        IHospiKeeperApi api,
        DialogService dialog,
        ToastService toast,
        NavigationService navigation) : base(api, dialog, toast, navigation)
    {
        _removeFeature = row => Features.Remove(row);
        _removeNotice = row => Notices.Remove(row);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ReloadCommand = new AsyncRelayCommand(LoadAsync);
        AddFeatureCommand = new RelayCommand(AddFeature);
        AddNoticeCommand = new RelayCommand(AddNotice);
    }

    public override string Key => PageKeys.Settings;
    public override string Title => "系统配置";
    public override Geometry Icon => AntIcons.Settings;
    public override string Group => "基础信息（管理员）";
    public override bool AdminOnly => true;
    public override int GroupOrder => 2;
    public override int PageOrder => 100;

    public ObservableCollection<SiteFeatureEditViewModel> Features { get; } = new();
    public ObservableCollection<SiteNoticeEditViewModel> Notices { get; } = new();

    [ObservableProperty] private string _brandName = string.Empty;
    [ObservableProperty] private string _brandEnglishName = string.Empty;
    [ObservableProperty] private string _brandMark = string.Empty;
    [ObservableProperty] private string _locationTagline = string.Empty;
    [ObservableProperty] private string _heroTitlePrefix = string.Empty;
    [ObservableProperty] private string _heroAccent = string.Empty;
    [ObservableProperty] private string _heroTitleSuffix = string.Empty;
    [ObservableProperty] private string _heroSubtitle = string.Empty;
    [ObservableProperty] private string _ratingText = "4.9";
    [ObservableProperty] private string _walkMinutesText = "5";
    [ObservableProperty] private string _hotline = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _checkInTime = string.Empty;
    [ObservableProperty] private string _checkOutTime = string.Empty;
    [ObservableProperty] private string _arrivalDeadline = string.Empty;
    [ObservableProperty] private string _paymentNote = string.Empty;
    [ObservableProperty] private string _depositNote = string.Empty;
    [ObservableProperty] private string _footerDescription = string.Empty;
    [ObservableProperty] private string _footerText = string.Empty;
    [ObservableProperty] private string _desktopAppName = string.Empty;
    [ObservableProperty] private string _desktopVersion = string.Empty;
    [ObservableProperty] private string _desktopFooterText = string.Empty;
    [ObservableProperty] private string _aboutDescription = string.Empty;
    [ObservableProperty] private string _statusText = "正在从 Redis 读取配置...";
    [ObservableProperty] private string? _validationError;
    [ObservableProperty] private bool _isBusy;

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand ReloadCommand { get; }
    public IRelayCommand AddFeatureCommand { get; }
    public IRelayCommand AddNoticeCommand { get; }

    public override Task OnNavigatedToAsync(object? argument) => LoadAsync();

    private async Task LoadAsync()
    {
        await TryLoadAsync(async () =>
        {
            var settings = await Api.GetSiteSettingsAsync();
            Apply(settings);
            StatusText = settings.UpdatedAt is null
                ? "当前使用默认配置；保存后会写入 Redis。"
                : $"上次保存：{settings.UpdatedAt:yyyy-MM-dd HH:mm} · {settings.UpdatedBy}";
        });
    }

    private async Task SaveAsync()
    {
        ValidationError = null;
        if (!double.TryParse(RatingText, NumberStyles.Float, CultureInfo.InvariantCulture, out var rating)
            && !double.TryParse(RatingText, out rating))
        {
            ValidationError = "评分请输入数字，例如 4.9。";
            return;
        }

        if (!int.TryParse(WalkMinutesText, out var walkMinutes))
        {
            ValidationError = "步行分钟数请输入整数。";
            return;
        }

        IsBusy = true;
        try
        {
            var settings = BuildSettings(rating, walkMinutes);
            await RunAsync(async () =>
            {
                var saved = await Api.UpdateSiteSettingsAsync(settings);
                Apply(saved);
                StatusText = $"已保存：{saved.UpdatedAt:yyyy-MM-dd HH:mm} · {saved.UpdatedBy}";
                WeakReferenceMessenger.Default.Send(new SiteSettingsChangedMessage());
            }, "系统配置已保存，Web 刷新后生效");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private SiteSettings BuildSettings(double rating, int walkMinutes) => new()
    {
        BrandName = BrandName,
        BrandEnglishName = BrandEnglishName,
        BrandMark = BrandMark,
        LocationTagline = LocationTagline,
        HeroTitlePrefix = HeroTitlePrefix,
        HeroAccent = HeroAccent,
        HeroTitleSuffix = HeroTitleSuffix,
        HeroSubtitle = HeroSubtitle,
        Rating = rating,
        WalkToLakeMinutes = walkMinutes,
        Hotline = Hotline,
        Address = Address,
        Email = Email,
        CheckInTime = CheckInTime,
        CheckOutTime = CheckOutTime,
        ArrivalDeadline = ArrivalDeadline,
        PaymentNote = PaymentNote,
        DepositNote = DepositNote,
        FooterDescription = FooterDescription,
        FooterText = FooterText,
        DesktopAppName = DesktopAppName,
        DesktopVersion = DesktopVersion,
        DesktopFooterText = DesktopFooterText,
        AboutDescription = AboutDescription,
        Features = Features.Select(f => f.ToContract()).ToList(),
        Notices = Notices.Select(n => n.ToContract()).ToList()
    };

    private void Apply(SiteSettings settings)
    {
        BrandName = settings.BrandName;
        BrandEnglishName = settings.BrandEnglishName;
        BrandMark = settings.BrandMark;
        LocationTagline = settings.LocationTagline;
        HeroTitlePrefix = settings.HeroTitlePrefix;
        HeroAccent = settings.HeroAccent;
        HeroTitleSuffix = settings.HeroTitleSuffix;
        HeroSubtitle = settings.HeroSubtitle;
        RatingText = settings.Rating.ToString("0.0");
        WalkMinutesText = settings.WalkToLakeMinutes.ToString(CultureInfo.InvariantCulture);
        Hotline = settings.Hotline;
        Address = settings.Address;
        Email = settings.Email;
        CheckInTime = settings.CheckInTime;
        CheckOutTime = settings.CheckOutTime;
        ArrivalDeadline = settings.ArrivalDeadline;
        PaymentNote = settings.PaymentNote;
        DepositNote = settings.DepositNote;
        FooterDescription = settings.FooterDescription;
        FooterText = settings.FooterText;
        DesktopAppName = settings.DesktopAppName;
        DesktopVersion = settings.DesktopVersion;
        DesktopFooterText = settings.DesktopFooterText;
        AboutDescription = settings.AboutDescription;

        Features.Clear();
        foreach (var feature in settings.Features)
        {
            Features.Add(new SiteFeatureEditViewModel(feature, _removeFeature));
        }

        Notices.Clear();
        foreach (var notice in settings.Notices)
        {
            Notices.Add(new SiteNoticeEditViewModel(notice, _removeNotice));
        }
    }

    private void AddFeature()
        => Features.Add(new SiteFeatureEditViewModel(new SiteFeature
        {
            Icon = "✨",
            Title = "新特色",
            Description = "请填写特色说明"
        }, _removeFeature));

    private void AddNotice()
        => Notices.Add(new SiteNoticeEditViewModel(new SiteNotice
        {
            Title = "新须知",
            Line1 = "请填写须知内容",
            Line2 = string.Empty
        }, _removeNotice));
}
