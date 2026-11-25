namespace HospiKeeper.Contracts;

/// <summary>入住 / 账单记录的查询条件，所有字段均可为空。</summary>
/// <param name="RoomNo">房号，模糊匹配。</param>
/// <param name="Keyword">客人姓名 / 手机号 / 单号，模糊匹配。</param>
/// <param name="From">起始时间（含）。</param>
/// <param name="To">结束时间（含）。</param>
public sealed record RecordQuery(string? RoomNo, string? Keyword, DateTime? From, DateTime? To);
