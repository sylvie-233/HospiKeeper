using System.Net;

namespace HospiKeeper.Client;

/// <summary>API 调用失败时抛出的异常，携带 HTTP 状态码与服务端消息。</summary>
public sealed class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
