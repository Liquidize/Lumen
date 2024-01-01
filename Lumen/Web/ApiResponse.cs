using System.Net;

namespace Lumen.Web
{
    public record ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; init; }
        public T Content { get; init; }

        public ApiResponse(HttpStatusCode statusCode, T content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
