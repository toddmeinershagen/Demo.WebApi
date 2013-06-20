using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Common.Logging;

namespace Demo.WebApi.Filters
{
    public class ShieldingExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public ShieldingExceptionFilterAttribute()
        {
            GenerateNewGuid = Guid.NewGuid;
        }

        /// <summary>
        /// This filter should do the following:
        /// 1.  Log the details of the unhandled exception.
        /// 2.  Set the status code to 500.
        /// 3.  Set the message to a generic message with the unique case id to contact the administrator.
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var id = GenerateNewGuid();
            LogException(actionExecutedContext.Exception, id);

            var response = GetGenericErrorResponse(actionExecutedContext, id);
            actionExecutedContext.Response = response;
        }

        public Func<Guid> GenerateNewGuid { get; set; }

        private static HttpResponseMessage GetGenericErrorResponse(HttpActionExecutedContext actionExecutedContext, Guid id)
        {
            var genericMessage = string.Format("An error occurred on the server (Error Id:  {0}).  If you continue to experience this problem, contact your administrator.", id);
            var error = new HttpError(genericMessage);
            var response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            return response;
        }

        private void LogException(Exception exception, Guid id)
        {
            exception.Data["Id"] = id;
            _logger.Error(exception.Message, exception);
        }
    }
}