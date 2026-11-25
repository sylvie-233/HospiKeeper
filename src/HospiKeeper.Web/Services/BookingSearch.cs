namespace HospiKeeper.Web.Services;

/// <summary>游客查询条件。</summary>
public sealed record SearchCriteria(DateTime CheckIn, DateTime CheckOut, int? TypeId);
