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
using HospiKeeper.Domain.Services;

namespace HospiKeeper.Application.Services;

/// <summary>前台核心流程实现：看板、在住、入住、退房与预定。</summary>
public sealed class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionContext _session;
    private readonly IOperationLogger _logger;
    private readonly IClock _clock;
    private readonly ICacheService _cache;
    private readonly IHotelCacheInvalidator _cacheInvalidator;

    public BookingService(
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

    public async Task<IReadOnlyList<BoardRoom>> GetBoardAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.Board,
            LoadBoardAsync,
            CacheDurations.Board,
            cancellationToken);

    private async Task<List<BoardRoom>> LoadBoardAsync(CancellationToken cancellationToken)
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync(cancellationToken: cancellationToken);
        var living = await _unitOfWork.CheckIns.GetLivingAsync(cancellationToken);
        var reserved = await _unitOfWork.Reservations.GetPendingAsync(cancellationToken);

        var livingByRoom = living
            .GroupBy(c => c.RoomId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CheckInTime).First());
        var reservedByRoom = reserved
            .GroupBy(r => r.RoomId)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.ArriveTime).First());

        return rooms
            .Select(r => new BoardRoom(
                r.ToDto(),
                livingByRoom.GetValueOrDefault(r.Id)?.ToDto(),
                reservedByRoom.GetValueOrDefault(r.Id)?.ToDto()))
            .ToList();
    }

    public async Task<IReadOnlyList<StayInfo>> GetLivingStaysAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var stays = await _cache.GetOrCreateAsync(
            CacheKeys.StaysAll,
            LoadLivingStaysAsync,
            CacheDurations.Stays,
            cancellationToken);

        var kw = keyword?.Trim();
        if (string.IsNullOrEmpty(kw))
        {
            return stays;
        }

        return stays
            .Where(s => s.CheckIn.RoomNo.Contains(kw, StringComparison.OrdinalIgnoreCase)
                        || s.CheckIn.GuestName.Contains(kw, StringComparison.OrdinalIgnoreCase)
                        || s.CheckIn.GuestPhone.Contains(kw, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<List<StayInfo>> LoadLivingStaysAsync(CancellationToken cancellationToken)
    {
        var stays = await _unitOfWork.CheckIns.GetLivingAsync(cancellationToken);
        return stays
            .OrderByDescending(c => c.CheckInTime)
            .Select(ToStayInfo)
            .ToList();
    }

    public async Task<StayInfo?> GetStayAsync(int checkInId, CancellationToken cancellationToken = default)
    {
        var record = await _unitOfWork.CheckIns.GetByIdAsync(checkInId, includeDetails: true, cancellationToken);
        return record is null || record.Status != CheckInStatus.Living ? null : ToStayInfo(record);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsAsync(
        ReservationStatus? status = null,
        CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.Reservations(status),
            async ct => (await _unitOfWork.Reservations.GetAllAsync(status, ct))
                .Select(r => r.ToDto())
                .ToList(),
            CacheDurations.Reservations,
            cancellationToken);

    public async Task<ReservationDto?> GetReservationAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId, cancellationToken);
        return reservation?.ToDto();
    }

    public async Task<FrontDeskCounts> GetCountsAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            CacheKeys.Counts,
            LoadCountsAsync,
            CacheDurations.Counts,
            cancellationToken);

    private async Task<FrontDeskCounts> LoadCountsAsync(CancellationToken cancellationToken)
    {
        var living = await _unitOfWork.CheckIns.CountLivingAsync(cancellationToken);
        var pending = await _unitOfWork.Reservations.CountPendingAsync(cancellationToken);
        var cleaning = await _unitOfWork.Rooms.CountByStatusAsync(RoomStatus.Cleaning, cancellationToken);
        return new FrontDeskCounts(living, pending, cleaning);
    }

    public async Task<CheckInRecordDto> CheckInAsync(
        CheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var guestName = InputGuard.Text(request.GuestName, "客人姓名", 32);
        var guestPhone = InputGuard.Phone(request.GuestPhone);
        var guestIdCard = InputGuard.IdCard(request.GuestIdCard);
        var note = InputGuard.OptionalText(request.Note, "备注");
        var price = InputGuard.Money(request.Price, "成交单价", allowZero: false);
        var deposit = InputGuard.Money(request.Deposit, "押金");
        InputGuard.Ensure(request.PlanOutTime > request.CheckInTime, "预计退房时间必须晚于入住时间。");
        var companions = ValidateCompanions(request.Companions);

        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new DomainException("房间不存在，请刷新后重试。");

        Reservation? reservation = null;
        if (request.ReservationId is int reservationId)
        {
            reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId, cancellationToken)
                          ?? throw new DomainException("预定单不存在。");
            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new DomainException("该预定已处理，不能重复转入入住。");
            }

            if (reservation.RoomId != room.Id)
            {
                throw new DomainException("预定房间与入住房间不一致。");
            }

            if (room.Status != RoomStatus.Reserved)
            {
                throw new DomainException("预定房间当前不是「预定」状态，请刷新房态。");
            }
        }
        else if (room.Status != RoomStatus.Free)
        {
            throw new DomainException($"房间 {room.RoomNo} 当前不是空闲状态，不能办理入住。");
        }

        if (price < room.MinPrice)
        {
            throw new DomainException($"成交单价不能低于底价 ¥{room.MinPrice:0.00}。");
        }

        var record = new CheckInRecord
        {
            CheckInNo = await _unitOfWork.DocumentNumbers.NextAsync("I", request.CheckInTime, cancellationToken),
            RoomId = room.Id,
            RoomNo = room.RoomNo,
            RoomTypeName = room.TypeName,
            GuestName = guestName,
            GuestPhone = guestPhone,
            GuestIdCard = guestIdCard,
            CheckInTime = request.CheckInTime,
            PlanOutTime = request.PlanOutTime,
            Price = price,
            OriginPrice = price != room.Price ? room.Price : null,
            Deposit = deposit,
            Status = CheckInStatus.Living,
            Note = note,
            OpUserId = _session.UserId,
            OperatorName = _session.DisplayName,
            CreatedAt = _clock.Now
        };

        record.Companions.AddRange(companions.Select(c => new Companion
        {
            Name = c.Name,
            Phone = c.Phone,
            IdCard = c.IdCard
        }));

        _unitOfWork.CheckIns.Add(record);
        room.Status = RoomStatus.Occupied;

        if (reservation is not null)
        {
            reservation.Status = ReservationStatus.Converted;
            reservation.CheckIn = record;
            reservation.CheckInNo = record.CheckInNo;
        }

        var priceNote = record.OriginPrice is decimal origin
            ? $"议价 ¥{price:0.00}（标价 ¥{origin:0.00}）"
            : $"房价 ¥{price:0.00}";
        var detail = $"{guestName} 入住，{priceNote}，押金 ¥{deposit:0.00}";
        if (reservation is not null)
        {
            detail += $"，由预定 {reservation.ResNo} 转入";
        }

        _logger.Append(OperationActions.CheckIn, $"{room.RoomNo} · {room.TypeName}",
            detail, record.CheckInNo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
        return record.ToDto();
    }

    public BillingResult QuoteCheckout(
        CheckInRecordDto record,
        DateTime actualOutTime,
        decimal damage,
        BillingMode mode)
    {
        InputGuard.Money(damage, "损坏扣费");
        return BillingCalculator.Calculate(
            record.CheckInTime,
            actualOutTime,
            record.Price,
            record.Deposit,
            record.ExtraTotal,
            damage,
            mode);
    }

    public async Task<CheckoutBillDto> CheckoutAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        InputGuard.Money(request.Damage, "损坏扣费");

        var record = await _unitOfWork.CheckIns.GetByIdAsync(
                         request.CheckInId, includeDetails: true, cancellationToken)
                     ?? throw new DomainException("入住单不存在。");
        if (record.Status != CheckInStatus.Living)
        {
            throw new DomainException("该入住单已退房，不能重复结算。");
        }

        var room = await _unitOfWork.Rooms.GetByIdAsync(record.RoomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");

        var billing = BillingCalculator.Calculate(
            record.CheckInTime,
            request.ActualOutTime,
            record.Price,
            record.Deposit,
            record.ExtraTotal,
            request.Damage,
            request.Mode);

        var bill = new CheckoutBill
        {
            BillNo = await _unitOfWork.DocumentNumbers.NextAsync("S", request.ActualOutTime, cancellationToken),
            CheckInId = record.Id,
            CheckInNo = record.CheckInNo,
            RoomId = record.RoomId,
            RoomNo = record.RoomNo,
            RoomTypeName = record.RoomTypeName,
            GuestName = record.GuestName,
            CheckInTime = record.CheckInTime,
            CheckoutTime = request.ActualOutTime,
            Mode = request.Mode,
            Nights = billing.Units,
            RoomFee = billing.RoomFee,
            ExtraTotal = billing.ExtraTotal,
            Damage = billing.Damage,
            OtherFee = 0m,
            DepositReceived = billing.Deposit,
            DepositUsed = Math.Min(billing.Deposit, billing.Total),
            Total = billing.Total,
            Diff = billing.Difference,
            Note = InputGuard.OptionalText(request.Note, "备注"),
            OpUserId = _session.UserId,
            OperatorName = _session.DisplayName,
            CreatedAt = _clock.Now
        };

        bill.Extras.AddRange(record.ExtraCharges.Select(e => new BillLine
        {
            Item = e.Item,
            Amount = e.Amount
        }));

        record.Status = CheckInStatus.CheckedOut;
        record.ActualOutTime = request.ActualOutTime;
        record.ExtendHours = request.Mode == BillingMode.Hourly ? billing.Units : 0;
        room.Status = RoomStatus.Cleaning;

        _unitOfWork.Bills.Add(bill);

        var damageNote = billing.Damage > 0m ? $"（含损坏扣费 ¥{billing.Damage:0.00}）" : string.Empty;
        _logger.Append(OperationActions.CheckOut, $"{record.RoomNo} · {record.GuestName}",
            $"结算 ¥{billing.Total:0.00}{damageNote}，房间转待打扫", bill.BillNo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
        return bill.ToDto();
    }

    public async Task AddExtraChargeAsync(
        ExtraChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var item = InputGuard.Text(request.Item, "消费项目", 64);
        var amount = InputGuard.Money(request.Amount, "消费金额", allowZero: false);

        var record = await _unitOfWork.CheckIns.GetByIdAsync(
                         request.CheckInId, includeDetails: true, cancellationToken)
                     ?? throw new DomainException("入住单不存在。");
        if (record.Status != CheckInStatus.Living)
        {
            throw new DomainException("该入住单已退房，不能追加消费。");
        }

        record.ExtraCharges.Add(new ExtraCharge
        {
            CheckInId = record.Id,
            Item = item,
            Amount = amount,
            ChargeTime = _clock.Now,
            OpUserId = _session.UserId,
            OperatorName = _session.DisplayName
        });

        _logger.Append(OperationActions.ExtraCharge, record.RoomNo,
            $"{record.GuestName} 追加 {item} ¥{amount:0.00}", record.CheckInNo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
    }

    public async Task<ReservationDto> CreateReservationAsync(
        ReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var guestName = InputGuard.Text(request.GuestName, "客人姓名", 32);
        var guestPhone = InputGuard.Phone(request.GuestPhone);
        var note = InputGuard.OptionalText(request.Note, "备注");

        var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId, cancellationToken)
                   ?? throw new DomainException("房间不存在。");
        if (room.Status != RoomStatus.Free)
        {
            throw new DomainException($"房间 {room.RoomNo} 当前不可预定。");
        }

        var reservation = new Reservation
        {
            ResNo = await _unitOfWork.DocumentNumbers.NextAsync("R", _clock.Now, cancellationToken),
            GuestName = guestName,
            GuestPhone = guestPhone,
            RoomId = room.Id,
            RoomNo = room.RoomNo,
            RoomTypeName = room.TypeName,
            ArriveTime = request.ArriveTime,
            Status = ReservationStatus.Pending,
            Note = note,
            OpUserId = _session.UserId,
            OperatorName = _session.DisplayName,
            CreatedAt = _clock.Now
        };

        _unitOfWork.Reservations.Add(reservation);
        room.Status = RoomStatus.Reserved;

        _logger.Append(OperationActions.Reservation, $"{room.RoomNo} · {room.TypeName}",
            $"{guestName} 预定 {reservation.ArriveTime:MM-dd HH:mm} 到店", reservation.ResNo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
        return reservation.ToDto();
    }

    public async Task CancelReservationAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        _session.RequireUser();
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId, cancellationToken)
                          ?? throw new DomainException("预定单不存在。");
        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new DomainException("只能取消待到店的预定。");
        }

        reservation.Status = ReservationStatus.Cancelled;

        var room = await _unitOfWork.Rooms.GetByIdAsync(reservation.RoomId, cancellationToken);
        if (room is not null
            && room.Status == RoomStatus.Reserved
            && !await _unitOfWork.Reservations.HasPendingForRoomAsync(room.Id, reservation.Id, cancellationToken))
        {
            room.Status = RoomStatus.Free;
        }

        _logger.Append(OperationActions.CancelReservation, reservation.RoomNo,
            $"预定 {reservation.ResNo}（{reservation.GuestName}）已取消", reservation.ResNo);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidator.InvalidateFrontDeskAsync(cancellationToken);
    }

    private static StayInfo ToStayInfo(CheckInRecord record)
        => new(
            record.ToDto(),
            record.ExtraTotal,
            BillingCalculator.CountNights(record.CheckInTime, record.PlanOutTime) * record.Price);

    private static List<CompanionInput> ValidateCompanions(IReadOnlyList<CompanionInput>? inputs)
    {
        var result = new List<CompanionInput>();
        if (inputs is null)
        {
            return result;
        }

        InputGuard.Ensure(inputs.Count <= 10, "同住人最多登记 10 位。");
        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input.Name)
                && string.IsNullOrWhiteSpace(input.Phone)
                && string.IsNullOrWhiteSpace(input.IdCard))
            {
                continue;
            }

            result.Add(new CompanionInput(
                InputGuard.Text(input.Name, "同住人姓名", 32),
                InputGuard.OptionalPhone(input.Phone),
                InputGuard.OptionalIdCard(input.IdCard)));
        }

        return result;
    }
}