using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace User.Identity.Infrastructure
{
    public class ResilienceClientFactory
    {
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int _retryCount;
        private int _exceptionCountBeforeBreaking;

        public ResilienceClientFactory(
            ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor,
            int retryCount,
            int exceptionCountBeforeBreaking
            )
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountBeforeBreaking = exceptionCountBeforeBreaking;
        }

        public ResilienceHttpClient GetResilienceHttpClient() =>
            new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);

        //定义异常处理策略
        private AsyncPolicy[] CreatePolicy(string origin)
        {
            return new AsyncPolicy[]
            {
                Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(
                        _retryCount,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            var msg = $"第{retryCount} 次重试 " +
                                $"of {context.PolicyKey}"+
                                $"at {context.OperationKey}"+
                                $"due to : {exception}";

                            _logger.LogWarning(msg);
                            _logger.LogDebug(msg);
                        }),
                Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(
                        _exceptionCountBeforeBreaking,
                        TimeSpan.FromMinutes(1),
                        (exception, duration) => {_logger.LogTrace("熔断器开启！");},
                        () => { _logger.LogTrace("熔断器关闭!"); }
                       )
            };
        }
    }

}
