using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;


namespace UserApi2.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(IHostingEnvironment env, ILogger<GlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            var response = new JsonErrorResponse();
            if(context.Exception.GetType() == typeof(UserOperationException))
            {
                response.Message = context.Exception.Message;

                context.Result = new BadRequestObjectResult(response);
            }
            else
            {
                response.Message = "发生了未知的内部错误";

                if(_env.IsDevelopment())
                {
                    response.DevelopmentMessage = context.Exception.StackTrace;
                }

                context.Result = new InternalServerErrorObjectResult(response);
            }

            _logger.LogError(context.Exception, context.Exception.Message);

            context.ExceptionHandled = true;
        }

        public class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object value) : base(value)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

    }
}
