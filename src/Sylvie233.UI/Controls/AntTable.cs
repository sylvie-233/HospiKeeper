using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Sylvie233.UI.Controls;

/// <summary>
/// 轻量数据表格：固定表头 + 可滚动行，支持文本列、模板列、列宽与对齐，
/// 用于替代 Avalonia 12 中已废弃的 DataGrid，同时保持 Ant Design 视觉。
/// </summary>
public class AntTable : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<AntTable, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<AntTable, string?>(nameof(EmptyText), "暂无数据");

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<AntTable, int>(nameof(PageSize), 10);

    public static readonly StyledProperty<bool> ShowPaginationProperty =
        AvaloniaProperty.Register<AntTable, bool>(nameof(ShowPagination), true);

    private Grid? _headerGrid;
    private ItemsControl? _rows;
    private TextBlock? _empty;
    private Border? _pagination;
    private INotifyCollectionChanged? _collection;
    private int _currentPage = 1;
    private bool _rebuildScheduled;

    static AntTable()
    {
        ItemsSourceProperty.Changed.AddClassHandler<AntTable>((table, _) =>
        {
            table._currentPage = 1;
            table.AttachAndRebuild();
        });

        PageSizeProperty.Changed.AddClassHandler<AntTable>((table, _) => table.Rebuild());
        ShowPaginationProperty.Changed.AddClassHandler<AntTable>((table, _) => table.Rebuild());
    }

    public AntTable()
    {
        Columns.CollectionChanged += (_, _) => Rebuild();
        _rowsTemplate = new FuncDataTemplate<object>((item, _) => BuildRow(item), supportsRecycling: false);
    }

    private readonly IDataTemplate _rowsTemplate;

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>每页条数；设置为 0 可关闭分页。</summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>是否显示分页；数据不足一页时仍会自动隐藏。</summary>
    public bool ShowPagination
    {
        get => GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public AvaloniaList<AntTableColumn> Columns { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerGrid = e.NameScope.Find<Grid>("PART_HeaderGrid");
        _rows = e.NameScope.Find<ItemsControl>("PART_Rows");
        _empty = e.NameScope.Find<TextBlock>("PART_Empty");
        _pagination = e.NameScope.Find<Border>("PART_Pagination");

        if (_rows is not null)
        {
            _rows.ItemTemplate = _rowsTemplate;
        }

        AttachAndRebuild();
    }

    private void AttachAndRebuild()
    {
        if (_collection is not null)
        {
            _collection.CollectionChanged -= OnItemsChanged;
        }

        _collection = ItemsSource as INotifyCollectionChanged;
        if (_collection is not null)
        {
            _collection.CollectionChanged += OnItemsChanged;
        }

        Rebuild();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 过滤 / 刷新时通常先 Clear 再批量 Add，合并为一次重建。
        _currentPage = 1;
        if (_rebuildScheduled)
        {
            return;
        }

        _rebuildScheduled = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _rebuildScheduled = false;
                Rebuild();
            },
            DispatcherPriority.Background);
    }

    private void Rebuild()
    {
        if (_headerGrid is null || _rows is null || _empty is null || _pagination is null)
        {
            return;
        }

        BuildHeader();

        var allItems = ItemsSource?.Cast<object>().ToList() ?? [];
        var total = allItems.Count;
        var enabled = ShowPagination && PageSize > 0;
        var pageCount = enabled ? Math.Max(1, (total + PageSize - 1) / PageSize) : 1;

        if (_currentPage > pageCount)
        {
            _currentPage = pageCount;
        }

        if (_currentPage < 1)
        {
            _currentPage = 1;
        }

        var pageItems = enabled
            ? allItems.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList()
            : allItems;

        _rows.ItemsSource = pageItems;
        _empty.IsVisible = total == 0;
        _pagination.IsVisible = enabled && total > PageSize;
        BuildPagination(total, pageCount);
    }

    private void BuildPagination(int total, int pageCount)
    {
        if (_pagination is null)
        {
            return;
        }

        _pagination.Child = null;
        if (!_pagination.IsVisible)
        {
            return;
        }

        var layout = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
        };

        var totalText = new TextBlock
        {
            Text = $"共 {total} 条",
            Classes = { "ant-table-total" },
            VerticalAlignment = VerticalAlignment.Center
        };
        layout.Children.Add(totalText);

        var pages = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(pages, 2);
        layout.Children.Add(pages);

        var previous = CreatePageButton("‹", "ant-pagination-nav", _currentPage > 1,
            () => SetPage(_currentPage - 1));
        ToolTip.SetTip(previous, "上一页");
        pages.Children.Add(previous);

        foreach (var page in BuildPageNumbers(pageCount))
        {
            if (page is null)
            {
                pages.Children.Add(new TextBlock
                {
                    Text = "…",
                    Classes = { "ant-pagination-ellipsis" },
                    VerticalAlignment = VerticalAlignment.Center
                });
                continue;
            }

            var button = CreatePageButton(page.Value.ToString(), "ant-pagination-item", true,
                () => SetPage(page.Value));
            if (page.Value == _currentPage)
            {
                button.Classes.Add("active");
            }

            pages.Children.Add(button);
        }

        var next = CreatePageButton("›", "ant-pagination-nav", _currentPage < pageCount,
            () => SetPage(_currentPage + 1));
        ToolTip.SetTip(next, "下一页");
        pages.Children.Add(next);

        _pagination.Child = layout;
    }

    private IEnumerable<int?> BuildPageNumbers(int pageCount)
    {
        if (pageCount <= 7)
        {
            for (var page = 1; page <= pageCount; page++)
            {
                yield return page;
            }

            yield break;
        }

        yield return 1;

        var start = Math.Max(2, _currentPage - 2);
        var end = Math.Min(pageCount - 1, _currentPage + 2);

        if (start > 2)
        {
            yield return null;
        }

        for (var page = start; page <= end; page++)
        {
            yield return page;
        }

        if (end < pageCount - 1)
        {
            yield return null;
        }

        yield return pageCount;
    }

    private Button CreatePageButton(string text, string className, bool enabled, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            IsEnabled = enabled,
            Focusable = false
        };
        button.Classes.Add(className);
        button.Click += (_, _) => onClick();
        return button;
    }

    private void SetPage(int page)
    {
        if (page < 1 || page == _currentPage)
        {
            return;
        }

        _currentPage = page;
        Rebuild();
    }

    private void BuildHeader()
    {
        if (_headerGrid is null)
        {
            return;
        }

        _headerGrid.Children.Clear();
        _headerGrid.ColumnDefinitions.Clear();

        foreach (var column in Columns)
        {
            _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(column.Width));
        }

        for (var i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            var text = new TextBlock
            {
                Text = column.Header,
                Classes = { "ant-table-header-text" },
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = column.Alignment
            };
            var cell = new Border
            {
                Padding = new Thickness(12, 0),
                Child = text
            };
            Grid.SetColumn(cell, i);
            _headerGrid.Children.Add(cell);
        }
    }

    private Control BuildRow(object item)
    {
        var grid = new Grid { Height = 44 };
        for (var i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            grid.ColumnDefinitions.Add(new ColumnDefinition(column.Width));

            Control content;
            if (column.CellTemplate is not null)
            {
                content = new ContentControl
                {
                    Content = item,
                    ContentTemplate = column.CellTemplate,
                    HorizontalContentAlignment = column.Alignment,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = column.Alignment,
                    FontSize = 13,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                textBlock.Classes.Add("ant-table-cell");
                if (!string.IsNullOrWhiteSpace(column.CellClasses))
                {
                    foreach (var cls in column.CellClasses.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        textBlock.Classes.Add(cls);
                    }
                }

                if (!string.IsNullOrWhiteSpace(column.Binding))
                {
                    textBlock.Bind(TextBlock.TextProperty, new Binding(column.Binding));
                    // 列宽不足时通过 Tooltip 查看完整内容。
                    textBlock.Bind(ToolTip.TipProperty, new Binding(column.Binding));
                }

                content = textBlock;
            }

            var cell = new Border
            {
                Padding = new Thickness(12, 0),
                Child = content
            };
            Grid.SetColumn(cell, i);
            grid.Children.Add(cell);
        }

        return new Border
        {
            Classes = { "ant-table-row" },
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = new SolidColorBrush(Color.Parse("#F0F0F0")),
            Child = grid
        };
    }
}
