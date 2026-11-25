using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Mapping;
using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Abstractions.Caching;
using HospiKeeper.Application.Caching;
using Sylvie233.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;

namespace HospiKeeper.Application.Services;

/// <summary>房间、房型与房态维护用例实现。</summary>
public sealed class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;
    private readonly IOperationLogger _logger;
    private readonly IClock _clock;
    private readonly ICacheService _cache;
    private readonly IHotelCacheInvalidator _cacheInvalidator;

    public RoomService(
        IUnitOfWork unitOfWork,
        SessionContext session,
        IOperationLogger logger,
        IClock clock,
        ICacheService cache,
        IHotelCacheInvalidator cacheInvalidator)
    {
        _unitOfWork = unitOfWork;
        _session = session;
        _logger = logger;
        _clock = clock;
        _cache = cache;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(
        int? floor = null,
        int? typeId = null,
        RoomStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Rooms(floor, typeId, status);
        return await _cache.GetOrCreateAsync(
            key,
            async ct => (await _unitOfWork.Rooms.GetAllAsync(floor, typeId, status, ct))
                .Select(r => r.ToDto())
                .ToList(),
            CacheDurations.Rooms,
            cancellationToken);
    }

    public async Task<IReadOnlyList<RoomDto>> GetFreeRoomsAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.FreeRooms,
            async ct => (await _unitOfWork.Rooms.GetFreeAsync(ct))
                .Select(r => r.ToDto())
                .ToList(),
            CacheDurations.Rooms,
            cancellationToken);

    public async Task<RoomDto?> GetRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken);
        return room?.ToDto();
    }

    public async Task<IReadOnlyList<int>> GetFloorsAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.Floors,
            async ct => (await _unitOfWork.Rooms.GetFloorsAsync(ct)).ToList(),
            CacheDurations.Rooms,
            cancellationToken);

    public async Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.RoomTypes,
            async ct => (await _unitOfWork.RoomTypes.GetAllAsync(ct))
                .Select(t => t.ToDto())
                .ToList(),
            CacheDurations.RoomTypes,
            cancellationToken);

    public async Task<RoomDto> CreateRoomAsync(RoomInput input, CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var roomNo = InputGuard.Text(input.RoomNo, "房号", 16);
        ValidateRoomInput(input);

        if (await _unitOfWork.Rooms.RoomNoExistsAsync(roomNo, null, cancellationToken))
        {
            throw new DomainException($"房号 {roomNo} 已存在。");
        }

        var type = await _unitOfWork.RoomTypes.GetByIdAsync(input.TypeId, cancellationToken)
                   ?? throw new DomainException("请选择有效的房型。");

        var room = new Room
        {
            RoomNo = roomNo,
            Floor = input.Floor,
            TypeId = type.Id,
            TypeName = type.Name,
            Beds = input.Beds,
            Price = decimal.Round(input.Price, 2),
            MinPrice = decimal.Round(input.MinPrice, 2),
            Deposit = decimal.Round(input.Deposit, 2),
            HasWindow = input.HasWindow,
            HasBath = input.HasBath,
            Status = RoomStatus.Free,
            Note = InputGuard.OptionalText(input.Note, "备注"),
            CreatedAt = _clock.Now
        };

        _unitOfWork.Rooms.Add(room);
        _logger.Append(OperationActions.Room, $"{room.RoomNo} · {room.TypeName}",
            $"新增房间 {room.RoomNo}，标价 ¥{room.Price:0.00}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
        return room.ToDto();
    }

    public async Task UpdateRoomAsync(
        int roomId,
        RoomInput input,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var roomNo = InputGuard.Text(input.RoomNo, "房号", 16);
        ValidateRoomInput(input);

        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        if (await _unitOfWork.Rooms.RoomNoExistsAsync(roomNo, roomId, cancellationToken))
        {
            throw new DomainException($"房号 {roomNo} 已存在。");
        }

        var type = await _unitOfWork.RoomTypes.GetByIdAsync(input.TypeId, cancellationToken)
                   ?? throw new DomainException("请选择有效的房型。");

        room.RoomNo = roomNo;
        room.Floor = input.Floor;
        room.TypeId = type.Id;
        room.TypeName = type.Name;
        room.Beds = input.Beds;
        room.Price = decimal.Round(input.Price, 2);
        room.MinPrice = decimal.Round(input.MinPrice, 2);
        room.Deposit = decimal.Round(input.Deposit, 2);
        room.HasWindow = input.HasWindow;
        room.HasBath = input.HasBath;
        room.Note = InputGuard.OptionalText(input.Note, "备注");

        _logger.Append(OperationActions.Room, $"{room.RoomNo} · {room.TypeName}",
            $"修改房间资料，标价 ¥{room.Price:0.00}，底价 ¥{room.MinPrice:0.00}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task DeleteRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        if (room.Status is RoomStatus.Occupied or RoomStatus.Reserved)
        {
            throw new DomainException("该房间仍有在住或预定关联，禁止删除。");
        }

        if (await _unitOfWork.Rooms.HasBusinessHistoryAsync(roomId, cancellationToken))
        {
            throw new DomainException("该房间存在历史入住 / 预定单据，禁止删除；可改为标记停用。");
        }

        _unitOfWork.Rooms.Remove(room);
        _logger.Append(OperationActions.Room, $"{room.RoomNo} · {room.TypeName}",
            $"删除房间 {room.RoomNo}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task MarkCleaningCompleteAsync(int roomId, CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        if (room.Status != RoomStatus.Cleaning)
        {
            throw new DomainException("只有打扫中的房间可以完成保洁。");
        }

        room.Status = RoomStatus.Free;
        _logger.Append(OperationActions.Cleaning, room.RoomNo, "保洁完成，房间改为空闲");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task RestoreRoomAsync(int roomId, CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        if (room.Status is not (RoomStatus.Repair or RoomStatus.Disabled or RoomStatus.Cleaning))
        {
            throw new DomainException("当前房态无需恢复。");
        }

        var oldStatus = room.Status;
        room.Status = RoomStatus.Free;
        room.Note = null;
        _logger.Append(OperationActions.Restore, room.RoomNo,
            $"{RoomStatusText(oldStatus)} → 空闲");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task SetMaintenanceAsync(
        int roomId,
        RoomStatus status,
        string? note,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        if (status is not (RoomStatus.Repair or RoomStatus.Disabled))
        {
            throw new DomainException("只能将房间标记为维修或停用。");
        }

        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        if (room.Status is RoomStatus.Occupied or RoomStatus.Reserved)
        {
            throw new DomainException("在住或预定中的房间不能标记维修/停用。");
        }

        room.Status = status;
        room.Note = InputGuard.OptionalText(note, "备注");
        var action = status == RoomStatus.Repair ? OperationActions.Maintenance : OperationActions.Disable;
        _logger.Append(action, room.RoomNo,
            $"{RoomStatusText(status)}{(room.Note is null ? string.Empty : "：" + room.Note)}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task<RoomTypeDto> CreateRoomTypeAsync(
        RoomTypeInput input,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var name = InputGuard.Text(input.Name, "房型名称", 32);
        InputGuard.Money(input.BasePrice, "基础定价");

        if (await _unitOfWork.RoomTypes.NameExistsAsync(name, null, cancellationToken))
        {
            throw new DomainException($"房型「{name}」已存在。");
        }

        var type = new RoomType
        {
            Name = name,
            BasePrice = decimal.Round(input.BasePrice, 2),
            Note = InputGuard.OptionalText(input.Note, "备注"),
            CreatedAt = _clock.Now
        };

        _unitOfWork.RoomTypes.Add(type);
        _logger.Append(OperationActions.RoomType, name,
            $"新增房型，基础定价 ¥{type.BasePrice:0.00}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
        return type.ToDto();
    }

    public async Task UpdateRoomTypeAsync(
        int roomTypeId,
        RoomTypeInput input,
        CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var name = InputGuard.Text(input.Name, "房型名称", 32);
        InputGuard.Money(input.BasePrice, "基础定价");

        var type = await _unitOfWork.RoomTypes.GetByIdAsync(roomTypeId, cancellationToken)
                   ?? throw new DomainException("房型不存在。");

        if (await _unitOfWork.RoomTypes.NameExistsAsync(name, roomTypeId, cancellationToken))
        {
            throw new DomainException($"房型「{name}」已存在。");
        }

        var oldName = type.Name;
        var oldPrice = type.BasePrice;
        type.Name = name;
        type.BasePrice = decimal.Round(input.BasePrice, 2);
        type.Note = InputGuard.OptionalText(input.Note, "备注");

        var nameChanged = !string.Equals(oldName, name, StringComparison.Ordinal);
        var priceChanged = oldPrice != type.BasePrice;
        if (nameChanged || priceChanged)
        {
            var rooms = await _unitOfWork.Rooms.GetByTypeAsync(roomTypeId, cancellationToken);
            foreach (var room in rooms)
            {
                if (nameChanged)
                {
                    room.TypeName = name;
                }

                if (priceChanged)
                {
                    room.Price = type.BasePrice;
                }
            }
        }

        if (priceChanged)
        {
            _logger.Append(OperationActions.ModifyPrice, $"房型·{name}",
                $"基础定价 ¥{oldPrice:0.00} → ¥{type.BasePrice:0.00}，已同步房间标价");
        }

        _logger.Append(OperationActions.RoomType, name,
            string.Equals(oldName, name, StringComparison.Ordinal)
                ? "修改房型资料"
                : $"房型改名：{oldName} → {name}，已同步房间数据");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    public async Task DeleteRoomTypeAsync(int roomTypeId, CancellationToken cancellationToken = default)
    {
        _session.RequireAdmin();
        var type = await _unitOfWork.RoomTypes.GetByIdAsync(roomTypeId, cancellationToken)
                   ?? throw new DomainException("房型不存在。");

        if (await _unitOfWork.RoomTypes.CountRoomsAsync(roomTypeId, cancellationToken) > 0)
        {
            throw new DomainException($"房型「{type.Name}」下仍有房间，不能删除。");
        }

        _unitOfWork.RoomTypes.Remove(type);
        _logger.Append(OperationActions.RoomType, type.Name, "删除房型");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateRoomsAsync(cancellationToken);
    }

    private static void ValidateRoomInput(RoomInput input)
    {
        InputGuard.Ensure(input.Floor is >= 1 and <= 99, "楼层需在 1 - 99 之间。");
        InputGuard.Ensure(input.Beds is >= 1 and <= 10, "床位数需在 1 - 10 之间。");
        InputGuard.Money(input.Price, "标价", allowZero: false);
        InputGuard.Money(input.MinPrice, "底价");
        InputGuard.Money(input.Deposit, "押金");
        InputGuard.Ensure(input.Price >= input.MinPrice, "标价不能低于底价。");
    }

    private static string RoomStatusText(RoomStatus status) => status switch
    {
        RoomStatus.Free => "空闲",
        RoomStatus.Occupied => "已入住",
        RoomStatus.Reserved => "预定",
        RoomStatus.Cleaning => "打扫中",
        RoomStatus.Repair => "维修",
        RoomStatus.Disabled => "停用",
        _ => status.ToString()
    };
}