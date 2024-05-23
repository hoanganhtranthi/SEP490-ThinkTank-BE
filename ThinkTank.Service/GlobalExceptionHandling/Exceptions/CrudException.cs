
using System.Net;

namespace ThinkTank.Application.GlobalExceptionHandling.Exceptions
{
    public class CrudException : Exception
    {
        public HttpStatusCode Status { get; private set; }
        public string Error { get; set; }

        public CrudException(HttpStatusCode status, string msg, string error) : base(msg)
        {
            Status = status;
            Error = error;
        }
    }
}
