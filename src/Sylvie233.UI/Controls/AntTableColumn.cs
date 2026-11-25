using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace Sylvie233.UI.Controls;

/// <summary>AntTable 列定义：文本绑定列或自定义模板列。</summary>
public sealed class AntTableColumn
{
    public string Header { get; set; } = string.Empty;

    public GridLength Width { get; set; } = new(1, GridUnitType.Star);

    /// <summary>文本列的数据绑定路径（相对行数据）。</summary>
    public string? Binding { get; set; }

    /// <summary>模板列；设置后优先于 Binding。</summary>
    public IDataTemplate? CellTemplate { get; set; }

    /// <summary>附加到单元格文本的样式类（如 text-money、text-danger）。</summary>
    public string? CellClasses { get; set; }

    public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
}
