using System.Net;

namespace Grand.Web.API.Exceptions
{
    public class WebHookCreateOrderException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public WebHookCreateOrderException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
