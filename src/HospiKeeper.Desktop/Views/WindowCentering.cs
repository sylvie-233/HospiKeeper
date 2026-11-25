using Avalonia;
using Avalonia.Controls;

namespace HospiKeeper.Desktop.Views;

internal static class WindowCentering
{
    public static void Center(Window window)
    {
        var screen = window.Screens.Primary;
        if (screen is null)
        {
            return;
        }

        var scale = screen.Scaling <= 0 ? 1 : screen.Scaling;
        var width = (int)Math.Round(window.Width * scale);
        var height = (int)Math.Round(window.Height * scale);
        var area = screen.WorkingArea;
        var x = area.X + Math.Max(0, (area.Width - width) / 2);
        var y = area.Y + Math.Max(0, (area.Height - height) / 2);
        window.Position = new PixelPoint(x, y);
    }
}
