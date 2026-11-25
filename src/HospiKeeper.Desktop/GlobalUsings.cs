// 桌面端只消费 API 契约。这里用别名保留既有代码中的领域类型名称，
// 实际类型是 Contracts 中的 DTO，避免 UI 直接依赖 EF 实体。
global using Room = HospiKeeper.Contracts.RoomDto;
global using RoomType = HospiKeeper.Contracts.RoomTypeDto;
global using Reservation = HospiKeeper.Contracts.ReservationDto;
global using CheckInRecord = HospiKeeper.Contracts.CheckInRecordDto;
global using CheckoutBill = HospiKeeper.Contracts.CheckoutBillDto;
global using OperationLog = HospiKeeper.Contracts.OperationLogDto;
