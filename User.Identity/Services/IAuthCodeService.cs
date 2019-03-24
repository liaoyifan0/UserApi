﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Services
{
    public interface IAuthCodeService
    {

        /// <summary>
        /// 根据手机号验证验证码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="code">验证码</param>
        /// <returns></returns>
        bool Validate(string phone, string code);
    }
}
