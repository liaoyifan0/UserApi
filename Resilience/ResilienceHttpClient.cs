using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpclient;
        private readonly Func<string, IEnumerable<IAsyncPolicy>> _policyCreator;
        private readonly ConcurrentDictionary<string, AsyncPolicyWrap> _policyWrappers;
        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly HttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(
            Func<string, IEnumerable<Policy>> policyCreator, 
            ILogger<ResilienceHttpClient> logger,
            HttpContextAccessor httpContextAccessor
        )
        {
            _httpclient = new HttpClient();
            _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            return await DoPostAsync(HttpMethod.Post, url, item, authorizationToken, requestId, authorizationMethod);
        }

        private Task<HttpResponseMessage> DoPostAsync<T>(HttpMethod method, string uri, T item, string authorizationToken, string requestId, string authorizationMethod)
        {
            if(method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be eithrt post or put");
            }
            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async context =>
            {
                var requestMessage = new HttpRequestMessage(method, origin);
                SetAuthorizationHeader(requestMessage);

                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }
                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }
                var response = await _httpclient.SendAsync(requestMessage);

                if(response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Context,Task<T>> func)
        {
            var normalizedOrigin = NormalizeOrigin(origin);
            if(!_policyWrappers.TryGetValue(normalizedOrigin, out AsyncPolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }
            return await policyWrap.ExecuteAsync(func, new Context(normalizedOrigin));
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim().ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage httpRequestMessage)
        {
            var authorization = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                httpRequestMessage.Headers.Add("Authorization", new List<string> { authorization });
            }
        }
        
    }
}
