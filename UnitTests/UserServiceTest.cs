using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using UserApi2;
using Xunit;

namespace UnitTests
{
    public class UserServiceTest
    {
        public HttpClient _httpClient { get; }


        public UserServiceTest()
        {

            //如果不指定ContentRoot就获取不到配置文件
            //如果不指定Url，那么app.Properties["server.Features"] as FeatureCollection.Address;中就没有Url
            var server = new TestServer(WebHost.CreateDefaultBuilder()
                .UseContentRoot(@"D:\project\docker\UserApi\UserApi2")
                .UseUrls("http://localhost:5001")
                .UseStartup<Startup>());

            _httpClient = server.CreateClient();
        }
        [Fact]
        public async void UserService_ShouldBeOk()
        {
            var response = await _httpClient.GetAsync("http://localhost:5001/api/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

       
    }
}
