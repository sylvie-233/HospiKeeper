using System.ComponentModel.DataAnnotations;

namespace HospiKeeper.Contracts;

/// <summary>首页特色卡片。</summary>
public sealed class SiteFeature
{
    [Required(ErrorMessage = "请填写特色图标。")]
    [StringLength(8, ErrorMessage = "特色图标不能超过 8 个字符。")]
    public string Icon { get; set; } = "✨";

    [Required(ErrorMessage = "请填写特色标题。")]
    [StringLength(20, ErrorMessage = "特色标题不能超过 20 个字符。")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "请填写特色说明。")]
    [StringLength(80, ErrorMessage = "特色说明不能超过 80 个字符。")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>入住须知卡片。</summary>
public sealed class SiteNotice
{
    [Required(ErrorMessage = "请填写须知标题。")]
    [StringLength(20, ErrorMessage = "须知标题不能超过 20 个字符。")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "请填写须知内容。")]
    [StringLength(80, ErrorMessage = "须知内容不能超过 80 个字符。")]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "须知内容不能超过 80 个字符。")]
    public string Line2 { get; set; } = string.Empty;
}

/// <summary>
/// 游客 Web 与桌面端共用的系统静态配置。
/// 存储于 Redis 的持久 key（无 TTL），未配置时使用这里的默认值。
/// </summary>
public sealed class SiteSettings
{
    [Required(ErrorMessage = "请输入品牌名称。")]
    [StringLength(40, ErrorMessage = "品牌名称不能超过 40 个字符。")]
    public string BrandName { get; set; } = "云栖民宿";

    [Required(ErrorMessage = "请输入英文品牌名。")]
    [StringLength(40, ErrorMessage = "英文品牌名不能超过 40 个字符。")]
    public string BrandEnglishName { get; set; } = "YUNQI INN";

    [Required(ErrorMessage = "请输入品牌标识字。")]
    [StringLength(2, ErrorMessage = "品牌标识字不能超过 2 个字符。")]
    public string BrandMark { get; set; } = "栖";

    [StringLength(60, ErrorMessage = "位置标语不能超过 60 个字符。")]
    public string LocationTagline { get; set; } = "西湖 · 南山路 · 独栋小院";

    [Required(ErrorMessage = "请输入首页主标题前缀。")]
    [StringLength(40, ErrorMessage = "首页主标题前缀不能超过 40 个字符。")]
    public string HeroTitlePrefix { get; set; } = "云栖民宿 · 在";

    [Required(ErrorMessage = "请输入首页高亮词。")]
    [StringLength(20, ErrorMessage = "首页高亮词不能超过 20 个字符。")]
    public string HeroAccent { get; set; } = "山与湖";

    [Required(ErrorMessage = "请输入首页主标题后缀。")]
    [StringLength(20, ErrorMessage = "首页主标题后缀不能超过 20 个字符。")]
    public string HeroTitleSuffix { get; set; } = "之间";

    [Required(ErrorMessage = "请输入首页副标题。")]
    [StringLength(120, ErrorMessage = "首页副标题不能超过 120 个字符。")]
    public string HeroSubtitle { get; set; } = "推窗见湖，出门即景。三步完成预订，到店支付，把时间留给旅途。";

    [Range(0, 5, ErrorMessage = "评分需在 0 - 5 之间。")]
    public double Rating { get; set; } = 4.9;

    [Range(1, 60, ErrorMessage = "步行时间需在 1 - 60 分钟之间。")]
    public int WalkToLakeMinutes { get; set; } = 5;

    [Required(ErrorMessage = "请输入前台热线。")]
    [StringLength(32, ErrorMessage = "前台热线不能超过 32 个字符。")]
    public string Hotline { get; set; } = "0571-8888 6666";

    [Required(ErrorMessage = "请输入地址。")]
    [StringLength(100, ErrorMessage = "地址不能超过 100 个字符。")]
    public string Address { get; set; } = "杭州市西湖区南山路 88 号";

    [Required(ErrorMessage = "请输入联系邮箱。")]
    [EmailAddress(ErrorMessage = "联系邮箱格式无效。")]
    [StringLength(80, ErrorMessage = "联系邮箱不能超过 80 个字符。")]
    public string Email { get; set; } = "hello@yunqi-inn.cn";

    [Required(ErrorMessage = "请输入入住时间说明。")]
    [StringLength(40, ErrorMessage = "入住时间说明不能超过 40 个字符。")]
    public string CheckInTime { get; set; } = "14:00 后";

    [Required(ErrorMessage = "请输入退房时间说明。")]
    [StringLength(40, ErrorMessage = "退房时间说明不能超过 40 个字符。")]
    public string CheckOutTime { get; set; } = "12:00 前";

    [Required(ErrorMessage = "请输入到店截止说明。")]
    [StringLength(40, ErrorMessage = "到店截止说明不能超过 40 个字符。")]
    public string ArrivalDeadline { get; set; } = "18:00 前";

    [Required(ErrorMessage = "请输入支付说明。")]
    [StringLength(120, ErrorMessage = "支付说明不能超过 120 个字符。")]
    public string PaymentNote { get; set; } = "到店支付，支持微信 / 支付宝 / 现金";

    [Required(ErrorMessage = "请输入押金说明。")]
    [StringLength(120, ErrorMessage = "押金说明不能超过 120 个字符。")]
    public string DepositNote { get; set; } = "退房验收后退还，具体以到店为准";

    [Required(ErrorMessage = "请输入页脚品牌介绍。")]
    [StringLength(300, ErrorMessage = "页脚品牌介绍不能超过 300 个字符。")]
    public string FooterDescription { get; set; } =
        "位于西湖南山路的独栋小院，推窗见湖，出门即景。我们相信好的住宿不只是睡一晚，更是一段旅途里安放身心的地方。";

    [Required(ErrorMessage = "请输入页脚文案。")]
    [StringLength(120, ErrorMessage = "页脚文案不能超过 120 个字符。")]
    public string FooterText { get; set; } = "云栖民宿 · 在线预订 · 到店支付 · 客服 9:00 - 21:00";

    [Required(ErrorMessage = "请输入桌面端系统名称。")]
    [StringLength(40, ErrorMessage = "桌面端系统名称不能超过 40 个字符。")]
    public string DesktopAppName { get; set; } = "云栖民宿管理系统";

    [Required(ErrorMessage = "请输入桌面端版本号。")]
    [StringLength(20, ErrorMessage = "版本号不能超过 20 个字符。")]
    public string DesktopVersion { get; set; } = "v1.0";

    [Required(ErrorMessage = "请输入桌面端状态栏文案。")]
    [StringLength(80, ErrorMessage = "桌面端状态栏文案不能超过 80 个字符。")]
    public string DesktopFooterText { get; set; } = "HospiKeeper · v1.0";

    [Required(ErrorMessage = "请输入关于本系统描述。")]
    [StringLength(300, ErrorMessage = "关于描述不能超过 300 个字符。")]
    public string AboutDescription { get; set; } =
        "面向中小型民宿 / 小旅馆的单机经营管理软件，聚焦房态、入住、预定、结算与报表。";

    public List<SiteFeature> Features { get; set; } = DefaultFeatures();

    public List<SiteNotice> Notices { get; set; } = DefaultNotices();

    /// <summary>最后保存时间，由服务端写入。</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>最后保存人，由服务端写入。</summary>
    public string? UpdatedBy { get; set; }

    private static List<SiteFeature> DefaultFeatures() =>
    [
        new() { Icon = "🛏️", Title = "舒适房型", Description = "单人间、双人间与大床房，满足独行、结伴与情侣出行。" },
        new() { Icon = "⏰", Title = "灵活到店", Description = "免预订金，入住日 18:00 前到店即可；房号到店分配。" },
        new() { Icon = "💳", Title = "到店支付", Description = "房费与押金到店支付，线上不收取任何费用。" },
        new() { Icon = "🧹", Title = "每日打扫", Description = "一客一换，公共区域每日清洁，住得安心。" }
    ];

    private static List<SiteNotice> DefaultNotices() =>
    [
        new() { Title = "入住与退房", Line1 = "入住 14:00 后凭预订号或手机号办理", Line2 = "退房 次日 12:00 前，可寄存行李" },
        new() { Title = "预订与取消", Line1 = "免登录、免预订金，提交即锁定房型库存", Line2 = "待到店状态可在「我的预订」自助取消" },
        new() { Title = "押金与支付", Line1 = "房费到店支付，支持微信 / 支付宝 / 现金", Line2 = "押金退房验收后退还，具体以到店为准" }
    ];
}
