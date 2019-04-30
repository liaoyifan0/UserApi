﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeGrantType : IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserService _userService;

        public SmsAuthCodeGrantType(IAuthCodeService authCodeService, IUserService userService)
        {
            _authCodeService = authCodeService;
            _userService = userService;
        }

        public string GrantType => "sms_auth_code";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            string phone = context.Request.Raw["phone"];
            string code = context.Request.Raw["auth_code"];

            var errorValidationResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);

            if(String.IsNullOrWhiteSpace(phone) || String.IsNullOrWhiteSpace(code))
            {
                context.Result = errorValidationResult;
                return;
            }

            if(!_authCodeService.Validate(phone, code))
            {
                context.Result = errorValidationResult;
                return;
            }

            var userInfo = await _userService.CheckOrCreate(phone);
            if(userInfo == null)
            {
                context.Result = errorValidationResult;
                return;
            }
            var claims = new Claim[]
            {
                new Claim("name", userInfo.Name??string.Empty),
                new Claim("Company", userInfo.Company??string.Empty),
                new Claim("Title", userInfo.Title??string.Empty),
                new Claim("Avatar", userInfo.Avatar??string.Empty)
            };

            context.Result = new GrantValidationResult(userInfo.Id.ToString(), GrantType, claims);            
        }
    }
}
