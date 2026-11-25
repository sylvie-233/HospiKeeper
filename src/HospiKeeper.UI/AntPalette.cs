using Avalonia.Media;

namespace HospiKeeper.UI;

/// <summary>与 Ant Design 语义色一致的画刷集合，方便 ViewModel 直接提供装填色。</summary>
public static class AntPalette
{
    public static readonly IBrush Primary = Brush("#1677FF");
    public static readonly IBrush PrimaryHover = Brush("#4096FF");
    public static readonly IBrush Success = Brush("#52C41A");
    public static readonly IBrush Warning = Brush("#FAAD14");
    public static readonly IBrush Error = Brush("#FF4D4F");
    public static readonly IBrush Purple = Brush("#722ED1");
    public static readonly IBrush Cyan = Brush("#13C2C2");
    public static readonly IBrush Orange = Brush("#FA8C16");
    public static readonly IBrush Red = Brush("#CF1322");
    public static readonly IBrush Text = Brush("#1F1F1F");
    public static readonly IBrush TextSecondary = Brush("#595959");
    public static readonly IBrush TextTertiary = Brush("#8C8C8C");

    public static readonly IBrush PrimaryBg = Brush("#E6F4FF");
    public static readonly IBrush SuccessBg = Brush("#F6FFED");
    public static readonly IBrush WarningBg = Brush("#FFF7E6");
    public static readonly IBrush ErrorBg = Brush("#FFF1F0");
    public static readonly IBrush PurpleBg = Brush("#F9F0FF");
    public static readonly IBrush CyanBg = Brush("#E6FFFB");
    public static readonly IBrush OrangeBg = Brush("#FFF7E6");

    public static readonly IBrush RoomFree = Brush("#52C41A");
    public static readonly IBrush RoomOccupied = Brush("#1677FF");
    public static readonly IBrush RoomReserved = Brush("#722ED1");
    public static readonly IBrush RoomCleaning = Brush("#13C2C2");
    public static readonly IBrush RoomRepair = Brush("#FAAD14");
    public static readonly IBrush RoomDisabled = Brush("#BFBFBF");

    public static SolidColorBrush Brush(string hex) => new(Color.Parse(hex));
}
