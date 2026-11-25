using System.Net;

namespace HospiKeeper.Client;

public sealed class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
