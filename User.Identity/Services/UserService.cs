using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
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
            var addressList = address.First().AddressList;
            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{host}:{port}";
        }

        public async Task<int> CheckOrCreate(string phone)
        {
            _logger.LogTrace("Enter check or create");
            var form = new Dictionary<string, string> { {"phone", phone} };

            try
            {
                //_httpClient实例是我们在StartUp中注册的ResilienceHttpClient,使用Polly进行容错处理
                var response = await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create", form);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var userId = await response.Content.ReadAsStringAsync();
                    int.TryParse(userId, out int intUserId);

                    return intUserId;
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Check or create重试后失败");
                throw e;
            }            

            return 0;
        }
    }
}
