using ContactApi.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Service
{
    public interface IUserService
    {
        Task<BaseUserInfo> GetBaseUserInfoAsync(int UserId);

    }
}
