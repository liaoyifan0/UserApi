using ContactApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Controllers
{
    public class BaseController : ControllerBase
    {
        protected UserIdentity userIdentity => new UserIdentity { UserId = 1, Name = "liaoyifan" };
    }
}
