using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Dtos;

namespace ContactApi.Service
{
    public class UserService : IUserService
    {
        public Task<BaseUserInfo> GetBaseUserInfoAsync(int UserId)
        {
            throw new NotImplementedException();
        }
    }
}
