

using System.Net;
using System.Text.Json;
using ThinkTank.Service.Exceptions;

namespace MuTote.API.Utility
{
    public class GlobalErrorHandlingMiddleware
    {
       private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private static Task HandleExceptionAsync(HttpContext context, CrudException exception)
        {
            HttpStatusCode status;
            var stackTrace = String.Empty;
            string message;
            message = exception.Message;
            status = exception.Status;
            stackTrace = exception.StackTrace;
            
            var exceptionResult = JsonSerializer.Serialize(new
            {
                error = message,
                stackTrace
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(exceptionResult);
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CrudException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
    }
}
