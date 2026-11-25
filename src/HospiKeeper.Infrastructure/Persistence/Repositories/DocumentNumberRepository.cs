using HospiKeeper.Application.Abstractions.Repositories;
using HospiKeeper.Infrastructure.Persistence.Entities;

namespace HospiKeeper.Infrastructure.Persistence.Repositories;

/// <summary>单据序号仓储：按「前缀 + 日期」生成并持久化序列。</summary>
internal sealed class DocumentNumberRepository : IDocumentNumberRepository
{
    private readonly HotelDbContext _db;

    public DocumentNumberRepository(HotelDbContext db) => _db = db;

    public async Task<string> NextAsync(
        string prefix,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var day = date.Date;
        var sequence = await _db.DocumentSequences.FindAsync(new object[] { prefix, day }, cancellationToken);
        if (sequence is null)
        {
            sequence = new DocumentSequence { Prefix = prefix, DocDate = day, Sequence = 1 };
            _db.DocumentSequences.Add(sequence);
        }
        else
        {
            sequence.Sequence++;
        }

        return $"{prefix}{day:yyyyMMdd}{sequence.Sequence:D3}";
    }
}
