using HospiKeeper.Application.Abstractions;
using HospiKeeper.Application.Abstractions.Caching;
using HospiKeeper.Application.Abstractions.Services;
using HospiKeeper.Application.Caching;
using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using Sylvie233.Caching;

namespace HospiKeeper.Application.Services;

/// <summary>游客在线预订用例实现：房型级库存、Web 预订与游客取消。</summary>
public sealed class GuestBookingService : IGuestBookingService
{
    private const int MaxNights = 30;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICacheService _cache;
    private readonly IHotelCacheInvalidator _cacheInvalidator;
    private readonly IOperationLogger _logger;

    public GuestBookingService(
        IUnitOfWork unitOfWork,
        IClock clock,
        ICacheService cache,
        IHotelCacheInvalidator cacheInvalidator,
        IOperationLogger logger)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _cache = cache;
        _cacheInvalidator = cacheInvalidator;
        _logger = logger;
    }

    public Task<GuestAvailabilityResult> GetAvailabilityAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        CancellationToken cancellationToken = default)
    {
        var (checkIn, checkOut) = NormalizeRange(checkInDate, checkOutDate);
        return _cache.GetOrCreateAsync(
            CacheKeys.GuestAvailability(checkIn, checkOut),
            ct => ComputeAvailabilityAsync(checkIn, checkOut, ct),
            CacheDurations.GuestAvailability,
            cancellationToken);
    }

    public Task<HotelSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            CacheKeys.GuestSummary,
            async ct =>
            {
                var rooms = await _unitOfWork.Rooms.GetAllAsync(cancellationToken: ct);
                var types = await _unitOfWork.RoomTypes.GetAllAsync(ct);
                return new HotelSummaryDto(
                    RoomCount: rooms.Count,
                    TypeCount: types.Count,
                    Rating: 4.9,
                    WalkToLakeMinutes: 5);
            },
            CacheDurations.GuestSummary,
            cancellationToken);

    public async Task<IReadOnlyList<GuestReservationDto>> GetByPhoneAsync(
        string phone,
        CancellationToken cancellationToken = default)
    {
        var normalized = InputGuard.Phone(phone);
        var reservations = await _unitOfWork.Reservations.GetByPhoneAsync(normalized, cancellationToken);
        var types = await _unitOfWork.RoomTypes.GetAllAsync(cancellationToken);

        return reservations
            .Select(r => ToDto(r, ResolveUnitPrice(r, types)))
            .ToList();
    }

    public async Task<GuestReservationDto> CreateAsync(
        GuestBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var (checkIn, checkOut) = NormalizeRange(request.CheckInDate, request.CheckOutDate);
        var guestName = InputGuard.Text(request.GuestName, "入住人姓名", 20);
        var guestPhone = InputGuard.Phone(request.GuestPhone);
        var guestIdCard = InputGuard.OptionalIdCard(request.GuestIdCard);
        var note = InputGuard.OptionalText(request.Note, "备注");

        var roomType = await _unitOfWork.RoomTypes.GetByIdAsync(request.TypeId, cancellationToken)
                       ?? throw new DomainException("请选择有效的房型。");

        var reservation = await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var remaining = await ComputeRemainingAsync(roomType.Id, checkIn, checkOut, ct);
                if (remaining <= 0)
                {
                    throw new DomainException("该房型在所选日期已订满，请调整日期或选择其他房型。");
                }

                var entity = new Reservation
                {
                    ResNo = await _unitOfWork.DocumentNumbers.NextAsync("R", _clock.Now, ct),
                    GuestName = guestName,
                    GuestPhone = guestPhone,
                    GuestIdCard = guestIdCard,
                    TypeId = roomType.Id,
                    RoomTypeName = roomType.Name,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Channel = ReservationChannel.Web,
                    UnitPrice = roomType.BasePrice,
                    RoomId = null,
                    RoomNo = null,
                    ArriveTime = checkIn.AddHours(14),
                    Status = ReservationStatus.Pending,
                    Note = note,
                    OpUserId = null,
                    OperatorName = "游客预订",
                    CreatedAt = _clock.Now
                };

                _unitOfWork.Reservations.Add(entity);
                _logger.Append(
                    OperationActions.Reservation,
                    $"{roomType.Name}（游客）",
                    $"{guestName} 在线预订 {checkIn:MM-dd} → {checkOut:MM-dd}",
                    entity.ResNo);
                return entity;
            },
            cancellationToken);

        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
        return ToDto(reservation, roomType.BasePrice);
    }

    public async Task CancelAsync(
        string resNo,
        string phone,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = InputGuard.Phone(phone);
        var reservation = await _unitOfWork.Reservations.GetByResNoAsync(resNo, cancellationToken)
                          ?? throw new DomainException("预订单不存在。");

        if (!string.Equals(reservation.GuestPhone, normalizedPhone, StringComparison.Ordinal))
        {
            throw new ForbiddenException("手机号与预订信息不一致。");
        }

        if (reservation.Channel != ReservationChannel.Web)
        {
            throw new ForbiddenException("该预订需联系前台取消。");
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new DomainException("只能取消待到店的预订。");
        }

        reservation.Status = ReservationStatus.Cancelled;
        _logger.Append(
            OperationActions.CancelReservation,
            $"{reservation.RoomTypeName}（游客）",
            $"{reservation.GuestName} 在线取消预订",
            reservation.ResNo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
    }

    private async Task<GuestAvailabilityResult> ComputeAvailabilityAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken)
    {
        var types = await _unitOfWork.RoomTypes.GetAllAsync(cancellationToken);
        var rooms = await _unitOfWork.Rooms.GetAllAsync(cancellationToken: cancellationToken);
        var pending = await _unitOfWork.Reservations.GetPendingAsync(cancellationToken);
        var nights = CountNights(checkIn, checkOut);

        var roomTypes = types
            .Select(type =>
            {
                var typeRooms = rooms.Where(r => r.TypeId == type.Id).ToList();
                var sellable = typeRooms.Count(r => r.Status is not RoomStatus.Repair and not RoomStatus.Disabled);
                var occupied = pending.Count(r =>
                    r.TypeId == type.Id
                    && r.CheckInDate.Date < checkOut
                    && r.CheckOutDate.Date > checkIn);

                return new GuestRoomTypeDto(
                    type.Id,
                    type.Name,
                    type.BasePrice,
                    type.Note,
                    typeRooms.Count == 0 ? 1 : typeRooms.Max(r => r.Beds),
                    typeRooms.Any(r => r.HasWindow),
                    typeRooms.Any(r => r.HasBath),
                    sellable,
                    Math.Max(0, sellable - occupied));
            })
            .OrderBy(t => t.BasePrice)
            .ToList();

        return new GuestAvailabilityResult(checkIn, checkOut, nights, roomTypes);
    }

    private async Task<int> ComputeRemainingAsync(
        int typeId,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken)
    {
        var rooms = await _unitOfWork.Rooms.GetByTypeAsync(typeId, cancellationToken);
        var pending = await _unitOfWork.Reservations.GetOverlappingPendingAsync(
            typeId,
            checkIn,
            checkOut,
            cancellationToken);

        var sellable = rooms.Count(r => r.Status is not RoomStatus.Repair and not RoomStatus.Disabled);
        return Math.Max(0, sellable - pending.Count);
    }

    private static GuestReservationDto ToDto(Reservation reservation, decimal unitPrice)
    {
        var nights = CountNights(reservation.CheckInDate, reservation.CheckOutDate);
        return new GuestReservationDto(
            reservation.ResNo,
            reservation.TypeId,
            reservation.RoomTypeName,
            reservation.CheckInDate,
            reservation.CheckOutDate,
            nights,
            unitPrice,
            unitPrice * nights,
            reservation.Status,
            reservation.Status == ReservationStatus.Pending ? null : reservation.RoomNo,
            reservation.GuestName,
            reservation.GuestPhone,
            reservation.Channel,
            reservation.CreatedAt);
    }

    private static decimal ResolveUnitPrice(Reservation reservation, IReadOnlyList<RoomType> types)
        => reservation.UnitPrice
           ?? types.FirstOrDefault(t => t.Id == reservation.TypeId)?.BasePrice
           ?? 0m;

    private (DateTime CheckIn, DateTime CheckOut) NormalizeRange(DateTime checkInDate, DateTime checkOutDate)
    {
        var checkIn = checkInDate.Date;
        var checkOut = checkOutDate.Date;

        if (checkIn < _clock.Now.Date)
        {
            throw new DomainException("入住日期不能早于今天。");
        }

        if (checkOut <= checkIn)
        {
            throw new DomainException("退房日期必须晚于入住日期。");
        }

        if (CountNights(checkIn, checkOut) > MaxNights)
        {
            throw new DomainException($"单次预订最长 {MaxNights} 晚。");
        }

        return (checkIn, checkOut);
    }

    private static int CountNights(DateTime checkIn, DateTime checkOut)
        => Math.Max(1, (checkOut.Date - checkIn.Date).Days);
}
