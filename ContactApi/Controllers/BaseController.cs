﻿using ContactApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected UserIdentity UserIdentity
        {
            get
            {
                var identity = new UserIdentity();
                identity.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "sub").Value);
                identity.Name = User.Claims.FirstOrDefault(c => c.Type == "name").Value;
                identity.Company = User.Claims.FirstOrDefault(c => c.Type == "company").Value;
                identity.Title = User.Claims.FirstOrDefault(c => c.Type == "title").Value;
                identity.Avatar = User.Claims.FirstOrDefault(c => c.Type == "avatar").Value;

                return identity;
            }
        }
    }
}
