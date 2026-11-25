using HospiKeeper.Application.Common;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;
using HospiKeeper.Domain.Enums;
using HospiKeeper.Domain.Services;
using HospiKeeper.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospiKeeper.Infrastructure.Persistence;

/// <summary>
/// 首次启动时写入与原型一致的演示数据：14 间房、六种房态、4 位在住、
/// 预定、近 30 天经营历史、操作日志与账单。
/// </summary>
public static class DemoDataSeeder
{
    private static readonly string[] GuestNames =
    {
        "张伟", "李娜", "刘洋", "陈静", "周杰", "王芳", "孙敏", "赵丽", "何伟",
        "郑爽", "冯磊", "朱琳", "许强", "林静", "黄勇", "徐婷", "马超", "胡蝶"
    };

    private static readonly (string Item, decimal Amount)[] ExtraItems =
    {
        ("矿泉水", 4m), ("泡面", 6m), ("饮料", 8m), ("洗衣", 10m), ("零食", 12m)
    };

    private static readonly RoomSeed[] RoomSeeds =
    {
        new("101", 1, "单人间", 1, 128m, 100m, 100m, true, true, RoomStatus.Free),
        new("102", 1, "双人间", 2, 168m, 130m, 200m, true, true, RoomStatus.Occupied),
        new("103", 1, "大床房", 2, 198m, 160m, 200m, true, true, RoomStatus.Occupied),
        new("104", 1, "单人间", 1, 128m, 100m, 100m, true, true, RoomStatus.Cleaning),
        new("105", 1, "双人间", 2, 168m, 130m, 200m, false, true, RoomStatus.Reserved),
        new("106", 1, "双人间", 2, 168m, 130m, 200m, false, true, RoomStatus.Repair),
        new("201", 2, "大床房", 2, 198m, 160m, 300m, true, true, RoomStatus.Occupied),
        new("202", 2, "大床房", 2, 198m, 160m, 200m, true, true, RoomStatus.Occupied),
        new("203", 2, "双人间", 2, 168m, 130m, 200m, true, true, RoomStatus.Free),
        new("204", 2, "单人间", 1, 128m, 100m, 100m, true, false, RoomStatus.Free),
        new("205", 2, "双人间", 2, 168m, 130m, 200m, true, true, RoomStatus.Free),
        new("206", 2, "大床房", 2, 198m, 160m, 300m, true, true, RoomStatus.Reserved),
        new("207", 2, "大床房", 2, 198m, 160m, 300m, true, true, RoomStatus.Cleaning),
        new("208", 2, "单人间", 1, 128m, 100m, 100m, false, false, RoomStatus.Disabled)
    };

    public static async Task SeedAsync(
        HotelDbContext db,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var numbers = new SeedNumbers();
        var admin = AddUser(db, "admin", "老陈", UserRole.Admin, AccountStatus.Active, now.AddDays(-400));
        var front01 = AddUser(db, "front01", "小王", UserRole.Front, AccountStatus.Active, now.AddDays(-200));
        var front02 = AddUser(db, "front02", "小李", UserRole.Front, AccountStatus.Disabled, now.AddDays(-150));
        await db.SaveChangesAsync(cancellationToken);

        var single = AddRoomType(db, "单人间", 128m, "小面积，适合独行客人");
        var twin = AddRoomType(db, "双人间", 168m, "标准双床");
        var king = AddRoomType(db, "大床房", 198m, "1.8m 大床，情侣/夫妻偏好");
        await db.SaveChangesAsync(cancellationToken);

        var types = new Dictionary<string, RoomType>
        {
            ["单人间"] = single,
            ["双人间"] = twin,
            ["大床房"] = king
        };

        var rooms = new Dictionary<string, Room>();
        foreach (var seed in RoomSeeds)
        {
            var room = AddRoom(db, seed, types[seed.TypeName]);
            rooms[seed.RoomNo] = room;
        }

        await db.SaveChangesAsync(cancellationToken);

        var today = now.Date;
        var res105 = AddReservation(db, numbers, rooms["105"], "王芳", "13900005678",
            today.AddHours(18), front01, now.AddHours(-3));
        var res206 = AddReservation(db, numbers, rooms["206"], "孙敏", "13700009012",
            today.AddDays(1).AddHours(14), front01, now.AddHours(-1));

        var stay102 = AddCheckIn(db, numbers, rooms["102"], "张伟", "13800001234", "330101199001120012",
            today.AddDays(-1).AddHours(14), today.AddDays(2).AddHours(12), null,
            price: 168m, deposit: 200m, op: front01, status: CheckInStatus.Living);
        AddExtra(stay102, "矿泉水", 4m, stay102.CheckInTime.AddHours(1), front01);

        var stay103 = AddCheckIn(db, numbers, rooms["103"], "李娜", "15900002233", "330101199203280078",
            today.AddDays(-2).AddHours(16), today.AddHours(12), null,
            price: 198m, deposit: 200m, op: front01, status: CheckInStatus.Living);

        var stay201Start = now.TimeOfDay >= TimeSpan.FromHours(14)
            ? today.AddHours(13).AddMinutes(30)
            : now.AddMinutes(-45);
        var stay201 = AddCheckIn(db, numbers, rooms["201"], "刘洋", "13600004455", "330101198805040034",
            stay201Start, today.AddDays(3).AddHours(12), null,
            price: 198m, deposit: 300m, op: front01, status: CheckInStatus.Living);
        stay201.Companions.Add(new Companion
        {
            Name = "张敏",
            Phone = "13800001111",
            IdCard = "330101199510230024"
        });

        var stay202 = AddCheckIn(db, numbers, rooms["202"], "陈静", "13500006677", "330101199511090056",
            today.AddDays(-3).AddHours(15), today.AddHours(12), null,
            price: 198m, deposit: 200m, op: front02, status: CheckInStatus.Living);
        AddExtra(stay202, "泡面", 6m, stay202.CheckInTime.AddHours(2), front02);

        var stay204 = AddCheckIn(db, numbers, rooms["204"], "周杰", "13612348765", "330101199008170090",
            today.AddDays(-1).AddHours(14), today.AddHours(12), today.AddHours(12).AddMinutes(10),
            price: 128m, deposit: 100m, op: front01, status: CheckInStatus.CheckedOut);

        var stay104 = AddCheckIn(db, numbers, rooms["104"], "何伟", "13755556666", "330101198706120031",
            today.AddDays(-1).AddHours(18), today.AddHours(12), today.AddHours(9).AddMinutes(30),
            price: 128m, deposit: 100m, op: front01, status: CheckInStatus.CheckedOut);

        var stay207 = AddCheckIn(db, numbers, rooms["207"], "赵丽", "13988887777", "330101199309300047",
            today.AddDays(-2).AddHours(19), today.AddHours(12), today.AddHours(10).AddMinutes(20),
            price: 198m, deposit: 300m, op: admin, status: CheckInStatus.CheckedOut);

        await db.SaveChangesAsync(cancellationToken);

        AddBill(db, numbers, stay204, stay204.ActualOutTime!.Value, front01);
        AddBill(db, numbers, stay104, stay104.ActualOutTime!.Value, front01);
        AddBill(db, numbers, stay207, stay207.ActualOutTime!.Value, admin);

        AddReservation(db, numbers, rooms["201"], "刘洋", "13600004455",
            today.AddDays(-1).AddHours(13), front01, today.AddDays(-1).AddHours(10),
            ReservationStatus.Converted, stay201);
        AddReservation(db, numbers, rooms["203"], "郑爽", "13012345678",
            today.AddDays(-4).AddHours(15), front01, today.AddDays(-5).AddHours(16),
            ReservationStatus.Cancelled);

        var history = BuildHistory(db, numbers, now, rooms, front01, front02, admin);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var record in history)
        {
            var op = record.OpUserId == admin.Id ? admin
                : record.OpUserId == front02.Id ? front02
                : front01;
            AddBill(db, numbers, record, record.ActualOutTime!.Value, op);
        }

        await db.SaveChangesAsync(cancellationToken);

        AddLog(db, front01, OperationActions.CheckIn, "201 · 大床房",
            "刘洋 入住，房价 ¥198.00，押金 ¥300.00", stay201.CheckInTime, stay201.CheckInNo);
        AddLog(db, front01, OperationActions.CheckOut, "204 · 周杰",
            "结算 ¥128.00，房间转待打扫", today.AddHours(12).AddMinutes(10));
        AddLog(db, admin, OperationActions.ModifyPrice, "房型·大床房",
            "基础定价 ¥188.00 → ¥198.00", today.AddDays(-1).AddHours(9));
        AddLog(db, front01, OperationActions.Reservation, "206 · 大床房",
            "孙敏 预定 次日 14:00 到店", now.AddHours(-1), res206.ResNo);
        AddLog(db, front01, OperationActions.Cleaning, "203",
            "保洁完成，房间改为空闲", today.AddHours(8).AddMinutes(40));
        AddLog(db, front02, OperationActions.ExtraCharge, "202",
            "陈静 追加 泡面 ¥6.00", today.AddHours(9).AddMinutes(5), stay202.CheckInNo);
        AddLog(db, admin, OperationActions.Login, "admin", "登录成功", now.AddHours(-2));
        AddLog(db, front01, OperationActions.Reservation, "105 · 双人间",
            "王芳 预定今日 18:00 到店", now.AddHours(-3), res105.ResNo);

        foreach (var sequence in numbers.Export())
        {
            db.DocumentSequences.Add(sequence);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<CheckInRecord> BuildHistory(
        HotelDbContext db,
        SeedNumbers numbers,
        DateTime now,
        IReadOnlyDictionary<string, Room> rooms,
        AppUser front01,
        AppUser front02,
        AppUser admin)
    {
        var today = now.Date;
        var rng = new Random(20261125);
        var operators = new[] { front01, front02, admin };
        var roomPool = rooms.Values.Where(r => r.RoomNo != "208").ToList();
        var occupiedToday = new HashSet<string> { "102", "103", "201", "202" };
        var records = new List<CheckInRecord>();

        for (var i = 29; i >= 1; i--)
        {
            var day = today.AddDays(-i);
            var weekend = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var count = 3 + (weekend ? 2 : 0) + rng.Next(0, 3);

            for (var n = 0; n < count; n++)
            {
                var room = roomPool[rng.Next(roomPool.Count)];
                if (i == 1 && occupiedToday.Contains(room.RoomNo))
                {
                    continue;
                }

                var guest = GuestNames[rng.Next(GuestNames.Length)];
                var phone = $"13{rng.Next(100000000, 999999999)}";
                var idCard = BuildIdCard(rng);
                var checkInTime = day.AddHours(13 + rng.Next(0, 7)).AddMinutes(rng.Next(0, 60));
                var checkOutTime = day.AddDays(1).AddHours(9 + rng.Next(0, 3)).AddMinutes(rng.Next(0, 60));
                var op = operators[rng.Next(operators.Length)];

                var record = AddCheckIn(db, numbers, room, guest, phone, idCard,
                    checkInTime, day.AddDays(2).AddHours(12), checkOutTime,
                    room.Price, room.Deposit, op, CheckInStatus.CheckedOut);

                if (rng.Next(100) < 60)
                {
                    var extra = ExtraItems[rng.Next(ExtraItems.Length)];
                    AddExtra(record, extra.Item, extra.Amount,
                        checkInTime.AddHours(1 + rng.Next(0, 5)), op);
                }

                records.Add(record);
            }
        }

        return records;
    }

    private static AppUser AddUser(
        HotelDbContext db,
        string username,
        string realName,
        UserRole role,
        AccountStatus status,
        DateTime createdAt)
    {
        var user = new AppUser
        {
            Username = username,
            RealName = realName,
            Role = role,
            Status = status,
            PasswordHash = PasswordHasher.Hash("123456"),
            LastLogin = status == AccountStatus.Active ? createdAt.AddDays(120) : createdAt.AddDays(30),
            CreatedAt = createdAt
        };
        db.Users.Add(user);
        return user;
    }

    private static RoomType AddRoomType(HotelDbContext db, string name, decimal price, string note)
    {
        var type = new RoomType
        {
            Name = name,
            BasePrice = price,
            Note = note,
            CreatedAt = DateTime.Now
        };
        db.RoomTypes.Add(type);
        return type;
    }

    private static Room AddRoom(HotelDbContext db, RoomSeed seed, RoomType type)
    {
        var room = new Room
        {
            RoomNo = seed.RoomNo,
            Floor = seed.Floor,
            TypeId = type.Id,
            TypeName = type.Name,
            Beds = seed.Beds,
            Price = seed.Price,
            MinPrice = seed.MinPrice,
            Deposit = seed.Deposit,
            HasWindow = seed.HasWindow,
            HasBath = seed.HasBath,
            Status = seed.Status,
            CreatedAt = DateTime.Now
        };
        db.Rooms.Add(room);
        return room;
    }

    private static CheckInRecord AddCheckIn(
        HotelDbContext db,
        SeedNumbers numbers,
        Room room,
        string guest,
        string phone,
        string idCard,
        DateTime checkInTime,
        DateTime planOutTime,
        DateTime? actualOutTime,
        decimal price,
        decimal deposit,
        AppUser op,
        CheckInStatus status)
    {
        var record = new CheckInRecord
        {
            CheckInNo = numbers.Next("I", checkInTime),
            RoomId = room.Id,
            RoomNo = room.RoomNo,
            RoomTypeName = room.TypeName,
            GuestName = guest,
            GuestPhone = phone,
            GuestIdCard = idCard,
            CheckInTime = checkInTime,
            PlanOutTime = planOutTime,
            ActualOutTime = actualOutTime,
            Price = price,
            Deposit = deposit,
            Status = status,
            OpUserId = op.Id,
            OperatorName = op.DisplayName,
            CreatedAt = checkInTime
        };
        db.CheckIns.Add(record);
        return record;
    }

    private static CheckoutBill AddBill(
        HotelDbContext db,
        SeedNumbers numbers,
        CheckInRecord record,
        DateTime checkoutTime,
        AppUser op)
    {
        var billing = BillingCalculator.Calculate(
            record.CheckInTime,
            checkoutTime,
            record.Price,
            record.Deposit,
            record.ExtraTotal,
            0m,
            BillingMode.Daily);

        var bill = new CheckoutBill
        {
            BillNo = numbers.Next("S", checkoutTime),
            CheckInId = record.Id,
            CheckInNo = record.CheckInNo,
            RoomId = record.RoomId,
            RoomNo = record.RoomNo,
            RoomTypeName = record.RoomTypeName,
            GuestName = record.GuestName,
            CheckInTime = record.CheckInTime,
            CheckoutTime = checkoutTime,
            Mode = BillingMode.Daily,
            Nights = billing.Units,
            RoomFee = billing.RoomFee,
            ExtraTotal = billing.ExtraTotal,
            Damage = 0m,
            DepositReceived = billing.Deposit,
            DepositUsed = Math.Min(billing.Deposit, billing.Total),
            Total = billing.Total,
            Diff = billing.Difference,
            OpUserId = op.Id,
            OperatorName = op.DisplayName,
            CreatedAt = checkoutTime
        };

        foreach (var extra in record.ExtraCharges)
        {
            bill.Extras.Add(new BillLine { Item = extra.Item, Amount = extra.Amount });
        }

        db.Bills.Add(bill);
        return bill;
    }

    private static Reservation AddReservation(
        HotelDbContext db,
        SeedNumbers numbers,
        Room room,
        string guest,
        string phone,
        DateTime arriveTime,
        AppUser op,
        DateTime createdAt,
        ReservationStatus status = ReservationStatus.Pending,
        CheckInRecord? converted = null)
    {
        var reservation = new Reservation
        {
            ResNo = numbers.Next("R", createdAt),
            GuestName = guest,
            GuestPhone = phone,
            RoomId = room.Id,
            RoomNo = room.RoomNo,
            RoomTypeName = room.TypeName,
            ArriveTime = arriveTime,
            Status = status,
            CheckInId = converted?.Id,
            CheckInNo = converted?.CheckInNo,
            OpUserId = op.Id,
            OperatorName = op.DisplayName,
            CreatedAt = createdAt
        };
        db.Reservations.Add(reservation);
        return reservation;
    }

    private static void AddExtra(
        CheckInRecord record,
        string item,
        decimal amount,
        DateTime time,
        AppUser op)
    {
        record.ExtraCharges.Add(new ExtraCharge
        {
            Item = item,
            Amount = amount,
            ChargeTime = time,
            OpUserId = op.Id,
            OperatorName = op.DisplayName
        });
    }

    private static void AddLog(
        HotelDbContext db,
        AppUser user,
        string action,
        string? target,
        string? detail,
        DateTime at,
        string? bizNo = null)
    {
        db.OperationLogs.Add(new OperationLog
        {
            UserId = user.Id,
            UserName = user.DisplayName,
            Action = action,
            Target = target,
            Detail = detail,
            BizNo = bizNo,
            CreatedAt = at
        });
    }

    private static string BuildIdCard(Random rng)
    {
        var birth = new DateTime(1970 + rng.Next(0, 30), 1 + rng.Next(0, 12), 1 + rng.Next(0, 27));
        return $"330101{birth:yyyyMMdd}{rng.Next(1000, 9999):D4}";
    }

    private sealed record RoomSeed(
        string RoomNo,
        int Floor,
        string TypeName,
        int Beds,
        decimal Price,
        decimal MinPrice,
        decimal Deposit,
        bool HasWindow,
        bool HasBath,
        RoomStatus Status);

    private sealed class SeedNumbers
    {
        private readonly Dictionary<(string Prefix, DateTime Date), int> _values = new();

        public string Next(string prefix, DateTime date)
        {
            var key = (prefix, date.Date);
            _values.TryGetValue(key, out var current);
            current++;
            _values[key] = current;
            return $"{prefix}{date:yyyyMMdd}{current:D3}";
        }

        public IEnumerable<DocumentSequence> Export()
            => _values.Select(kv => new DocumentSequence
            {
                Prefix = kv.Key.Prefix,
                DocDate = kv.Key.Date,
                Sequence = kv.Value
            });
    }
}
