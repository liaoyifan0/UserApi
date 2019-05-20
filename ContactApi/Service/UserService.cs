using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ContactApi.Dtos;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;

namespace ContactApi.Service
{
    public class UserService : IUserService
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger<UserService> _logger;
        private string _userServiceUrl;

        public UserService(IHttpClient httpClient,
            IOptions<ServiceDiscoveryOptions> serviceDiscoveryOptions,
            ILogger<UserService> logger,
            IDnsQuery dnsQuery)
        {
            _httpClient = httpClient;
            _logger = logger;

            var address = dnsQuery.ResolveService("service.consul", serviceDiscoveryOptions.Value.ServiceName);
            var addressList = address.FirstOrDefault().AddressList;
            var hostName = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{hostName}:{port}";
        }

        public async Task<UserIdentity> GetBaseUserInfoAsync(int UserId)
        {
            _logger.LogTrace("Enter check or create");

            try
            {
                //_httpClient实例是我们在StartUp中注册的ResilienceHttpClient,使用Polly进行容错处理
                var response = await _httpClient.GetStringAsync(_userServiceUrl + "/api/users/baseinfo/" + UserId);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    var userInfo = JsonConvert.DeserializeObject<UserIdentity>(response);

                    _logger.LogTrace($"Current user id is: {userInfo.UserId}");
                    return userInfo;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Check or create重试后失败");
                throw e;
            }

            return null;
            throw new NotImplementedException();
        }
    }
}
