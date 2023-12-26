using System.Net;

namespace Lumen.Web
{
    public record ApiResponse(HttpStatusCode StatusCode, string Content)
    {
        public ApiResponse() : this(HttpStatusCode.OK, string.Empty)
        {

        }
    }
}
