using Avalonia.Media;

namespace HospiKeeper.UI.Icons;

/// <summary>内置几何图标，供导航、按钮与空状态复用。</summary>
public static class AntIcons
{
    public static Geometry Dashboard { get; } = Geometry.Parse(
        "M3,3 H10 V10 H3 Z M14,3 H21 V10 H14 Z M3,14 H10 V21 H3 Z M14,14 H21 V21 H14 Z");

    public static Geometry Plus { get; } = Geometry.Parse(
        "M11,4 H13 V11 H20 V13 H13 V20 H11 V13 H4 V11 H11 Z");

    public static Geometry Calendar { get; } = Geometry.Parse(
        "M4,5 H20 V21 H4 Z M6,2 H8 V7 H6 Z M16,2 H18 V7 H16 Z M4,10 H20 V12 H4 Z");

    public static Geometry Bed { get; } = Geometry.Parse(
        "M3,6 H5 V12 H19 V8 H21 V19 H19 V16 H5 V19 H3 Z M6,7 H10 V10 H6 Z M11,7 H15 V10 H11 Z");


    public static Geometry Chart { get; } = Geometry.Parse(
        "M4,20 V10 H7 V20 Z M10,20 V4 H13 V20 Z M16,20 V13 H19 V20 Z");

    public static Geometry Records { get; } = Geometry.Parse(
        "M6,3 H18 V21 H6 Z M3,3 H5 V21 H3 Z M9,7 H16 V9 H9 Z M9,11 H16 V13 H9 Z M9,15 H16 V17 H9 Z");

    public static Geometry Door { get; } = Geometry.Parse(
        "M5,3 H19 V21 H5 Z M15,11 H17 V14 H15 Z");

    public static Geometry Tag { get; } = Geometry.Parse(
        "M3,12 L12,3 H21 V12 L12,21 Z M16,7 H18 V10 H16 Z");

    public static Geometry Users { get; } = Geometry.Parse(
        "M9,4 A4,4 0 1 1 9,12 A4,4 0 1 1 9,4 Z M2,21 C2,16 5,14 9,14 C13,14 16,16 16,21 Z " +
        "M17,5 A3.5,3.5 0 1 1 17,12 A3.5,3.5 0 1 1 17,5 Z " +
        "M16,14 C19,14 22,16 22,20 H17 C17,17.5 16.5,15.5 15.5,14.5 Z");

    public static Geometry Settings { get; } = Geometry.Parse(
        "M10,2 H14 V5 L16.5,6 L19,4 L21,7 L18.5,9 V12 L21,14 L19,17 L16.5,15 L14,16 V19 H10 V16 " +
        "L7.5,15 L5,17 L3,14 L5.5,12 V9 L3,7 L5,4 L7.5,6 L10,5 Z M12,8 A4,4 0 1 0 12,16 A4,4 0 1 0 12,8 Z");



    public static Geometry Refresh { get; } = Geometry.Parse(
        "M12,4 A8,8 0 1 1 5,7 L3,4 V11 H10 L7.5,9 A5,5 0 1 0 12,7 Z");

    public static Geometry Close { get; } = Geometry.Parse(
        "M5,7 L7,5 L12,10 L17,5 L19,7 L14,12 L19,17 L17,19 L12,14 L7,19 L5,17 L10,12 Z");

    public static Geometry Minimize { get; } = Geometry.Parse("M4,11 H20 V13 H4 Z");

    public static Geometry Maximize { get; } = Geometry.Parse(
        "M4,4 H20 V20 H4 Z M6,6 V18 H18 V6 Z");


    public static Geometry Check { get; } = Geometry.Parse(
        "M4,12 L9,17 L20,6 L18,4 L9,13 L6,10 Z");

    public static Geometry Warning { get; } = Geometry.Parse(
        "M12,2 L23,21 H1 Z M11,8 H13 V15 H11 Z M11,17 H13 V19 H11 Z");

    public static Geometry Info { get; } = Geometry.Parse(
        "M12,2 A10,10 0 1 1 12,22 A10,10 0 1 1 12,2 Z M11,10 H13 V18 H11 Z M11,6 H13 V8 H11 Z");

    public static Geometry Money { get; } = Geometry.Parse(
        "M3,6 H21 V18 H3 Z M5,9 V15 H19 V9 Z M10,10 H14 V14 H10 Z");

    public static Geometry Key { get; } = Geometry.Parse(
        "M14,3 A6,6 0 1 1 14,15 L12,15 L9,18 L7,18 L7,20 L3,20 V16 L10,9 A6,6 0 0 1 14,3 Z " +
        "M14,6 H16 V9 H14 Z");



    public static Geometry Clock { get; } = Geometry.Parse(
        "M12,2 A10,10 0 1 1 12,22 A10,10 0 1 1 12,2 Z M11,6 H13 V12 H17 V14 H11 Z");


    public static Geometry Home { get; } = Geometry.Parse(
        "M12,3 L22,11 H19 V21 H14 V15 H10 V21 H5 V11 H2 Z");

    public static Geometry Sparkle { get; } = Geometry.Parse(
        "M12,2 L14,8 L20,10 L14,12 L12,18 L10,12 L4,10 L10,8 Z");



    public static Geometry User { get; } = Geometry.Parse(
        "M12,3 A4.5,4.5 0 1 1 12,12 A4.5,4.5 0 1 1 12,3 Z M4,21 C4,16 7.5,14 12,14 C16.5,14 20,16 20,21 Z");

}
