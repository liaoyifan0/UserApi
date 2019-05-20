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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(
            Func<string, IEnumerable<IAsyncPolicy>> policyCreator, 
            ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _httpclient = new HttpClient();
            _policyCreator = policyCreator;
            _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> GetStringAsync(string url, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var origin = GetOriginFromUri(url);

            return HttpInvoker(origin, async context =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                var response = await _httpclient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            });
        }

        public Task<HttpResponseMessage> PutAsync<T>(string url, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            Func<HttpContent> func = () => { return GetHttpContent(item); };
            return DoPostPutAsync(HttpMethod.Put, url, func, authorizationToken, requestId, authorizationMethod);
        }


        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            Func<HttpContent> func = () => { return GetHttpContent(item); };
            return await DoPostPutAsync(HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string ,string> keyValues, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            Func<HttpContent> func = () => { return GetHttpContent(keyValues); };
            return await DoPostPutAsync(HttpMethod.Post, url, func, authorizationToken, requestId, authorizationMethod);
        }

        private HttpContent GetHttpContent<T>(T item)
        {
            return new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");
        }

        private HttpContent GetHttpContent(Dictionary<string, string> keyValues)
        {
            return new FormUrlEncodedContent(keyValues);
        }


        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod method, string uri, Func<HttpContent> func, string authorizationToken, string requestId, string authorizationMethod)
        {
            if(method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be eithrt post or put");
            }
            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async context =>
            {
                var requestMessage = new HttpRequestMessage(method, uri);
                SetAuthorizationHeader(requestMessage);

                requestMessage.Content = func();
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }
                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }
                var response = await _httpclient.SendAsync(requestMessage);

                if(response.StatusCode == HttpStatusCode.InternalServerError)
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
