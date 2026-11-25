namespace HospiKeeper.Application.Abstractions.Repositories;

public interface IDocumentNumberRepository
{
    /// <summary>
    /// 生成「前缀 + yyyyMMdd + 3 位序号」业务单号。
    /// 序号变更由工作单元统一保存，不在此处单独提交。
    /// </summary>
    Task<string> NextAsync(string prefix, DateTime date, CancellationToken cancellationToken = default);
}
