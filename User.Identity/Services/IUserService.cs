using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public interface IUserService
    {

        /// <summary>
        /// 根据手机号检查用户是否注册，如果没有则注册
        /// </summary>
        /// <param name="phone"></param>
        /// <returns>用户id</returns>
        Task<UserInfo> CheckOrCreate(string phone);
    }
}
