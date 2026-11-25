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

    private Grid? _headerGrid;
    private ItemsControl? _rows;
    private TextBlock? _empty;
    private INotifyCollectionChanged? _collection;

    static AntTable()
    {
        ItemsSourceProperty.Changed.AddClassHandler<AntTable>((table, _) => table.AttachAndRebuild());
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

    public AvaloniaList<AntTableColumn> Columns { get; } = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerGrid = e.NameScope.Find<Grid>("PART_HeaderGrid");
        _rows = e.NameScope.Find<ItemsControl>("PART_Rows");
        _empty = e.NameScope.Find<TextBlock>("PART_Empty");

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

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_headerGrid is null || _rows is null || _empty is null)
        {
            return;
        }

        BuildHeader();

        _rows.ItemsSource = ItemsSource;
        var hasItems = ItemsSource is not null && ItemsSource.Cast<object>().Any();
        _empty.IsVisible = !hasItems;
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
