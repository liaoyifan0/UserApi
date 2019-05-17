using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Dtos;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        }

            public Task<BaseUserInfo> GetBaseUserInfoAsync(int UserId)
        {
            throw new NotImplementedException();
        }
    }
}
