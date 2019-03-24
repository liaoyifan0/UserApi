using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpclient;
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWraps;
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly HttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(
            Func<string, IEnumerable<Policy>> policyCreator, 
            ILogger<ResilienceHttpClient> logger,
            HttpContextAccessor httpContextAccessor
        )
        {
            _httpclient = new HttpClient();
            _policyWraps = new ConcurrentDictionary<string, PolicyWrap>();
            _policyCreator = policyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            throw new NotImplementedException();
        }

        
    }
}
