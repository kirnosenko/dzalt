using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DZALT.Web.Infrastructure
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var handle = HandleException(context.Exception);

            if (handle)
            {
                context.Result = ExceptionToResult(context.Exception);
            }

            context.ExceptionHandled = handle;
        }

        protected virtual bool HandleException(Exception exception)
        {
            return true;
        }

        protected virtual ContentResult ExceptionToResult(Exception exception)
        {
            var body = new StringBuilder();
            while (exception != null)
            {
                body.AppendLine($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                exception = exception.InnerException;
            }

            return new ContentResult
            {
                Content = body.ToString(),
                ContentType = "text/plain",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };
        }
    }
}
