using System.Net;

namespace Lumen.Web
{
    public record ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; init; }
        public T Content { get; init; }

        private string Message { get; init; }

        public ApiResponse(HttpStatusCode statusCode, T content, string message)
        {
            StatusCode = statusCode;
            Content = content;
            Message = message;
        }
    }
}
