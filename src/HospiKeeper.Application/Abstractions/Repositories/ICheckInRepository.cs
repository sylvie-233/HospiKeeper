using HospiKeeper.Contracts;
using HospiKeeper.Domain.Entities;

namespace HospiKeeper.Application.Abstractions.Repositories;

/// <summary>入住记录查询与变更。</summary>
public interface ICheckInRepository
{
    /// <summary>查询全部在住记录。</summary>
    Task<IReadOnlyList<CheckInRecord>> GetLivingAsync(CancellationToken cancellationToken = default);

    /// <summary>按 ID 查询入住记录，可选择是否加载同住人与消费明细。</summary>
    Task<CheckInRecord?> GetByIdAsync(
        int checkInId,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    /// <summary>按条件查询记录；非管理员只能查询本人经办的数据。</summary>
    Task<IReadOnlyList<CheckInRecord>> QueryAsync(
        RecordQuery query,
        int? operatorUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>判断房间是否存在入住历史。</summary>
    Task<bool> HasAnyForRoomAsync(int roomId, CancellationToken cancellationToken = default);

    /// <summary>统计在住记录数量。</summary>
    Task<int> CountLivingAsync(CancellationToken cancellationToken = default);

    /// <summary>统计指定日期办理入住的记录数。</summary>
    Task<int> CountByCheckInDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>加入工作单元，等待统一保存。</summary>
    void Add(CheckInRecord record);
}
