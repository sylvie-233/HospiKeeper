using Avalonia;
using Avalonia.Controls;

namespace Sylvie233.UI.Controls;

public enum AntRoomCardState
{
    Free,
    Occupied,
    Reserved,
    Cleaning,
    Repair,
    Disabled
}

/// <summary>房态看板房间卡片：状态色由主题中的伪类样式统一控制。</summary>
public class AntRoomCard : Button
{
    public static readonly StyledProperty<string?> RoomNoProperty =
        AvaloniaProperty.Register<AntRoomCard, string?>(nameof(RoomNo));

    public static readonly StyledProperty<string?> TypeNameProperty =
        AvaloniaProperty.Register<AntRoomCard, string?>(nameof(TypeName));

    public static readonly StyledProperty<string?> PriceTextProperty =
        AvaloniaProperty.Register<AntRoomCard, string?>(nameof(PriceText));

    public static readonly StyledProperty<string?> GuestTextProperty =
        AvaloniaProperty.Register<AntRoomCard, string?>(nameof(GuestText));

    public static readonly StyledProperty<string?> StatusTextProperty =
        AvaloniaProperty.Register<AntRoomCard, string?>(nameof(StatusText));

    public static readonly StyledProperty<AntRoomCardState> StateProperty =
        AvaloniaProperty.Register<AntRoomCard, AntRoomCardState>(nameof(State));

    static AntRoomCard()
    {
        StateProperty.Changed.AddClassHandler<AntRoomCard>((card, _) => card.UpdateState());
    }

    public AntRoomCard() => UpdateState();

    public string? RoomNo
    {
        get => GetValue(RoomNoProperty);
        set => SetValue(RoomNoProperty, value);
    }

    public string? TypeName
    {
        get => GetValue(TypeNameProperty);
        set => SetValue(TypeNameProperty, value);
    }

    public string? PriceText
    {
        get => GetValue(PriceTextProperty);
        set => SetValue(PriceTextProperty, value);
    }

    public string? GuestText
    {
        get => GetValue(GuestTextProperty);
        set => SetValue(GuestTextProperty, value);
    }

    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public AntRoomCardState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    private void UpdateState()
    {
        PseudoClasses.Set(":free", State == AntRoomCardState.Free);
        PseudoClasses.Set(":occupied", State == AntRoomCardState.Occupied);
        PseudoClasses.Set(":reserved", State == AntRoomCardState.Reserved);
        PseudoClasses.Set(":cleaning", State == AntRoomCardState.Cleaning);
        PseudoClasses.Set(":repair", State == AntRoomCardState.Repair);
        PseudoClasses.Set(":stopped", State == AntRoomCardState.Disabled);
    }
}
