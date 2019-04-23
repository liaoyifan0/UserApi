using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Data;
using ContactApi.Models;
using ContactApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ContactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : BaseController
    {

        private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
        private readonly IUserService _userService;
        private readonly IOptions<MongoSettings> _settings;

        public ContactController(
            IContactApplyRequestRepository contactApplyRequestRepository,
            IUserService userService,
            IOptions<MongoSettings> settings
        )
        {
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
            _settings = settings;
        }


        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests()
        {
            var result = await _contactApplyRequestRepository.GetRequestListAsync(userIdentity.UserId);
            return Ok(result);
        }

        [HttpPost]
        [Route("apply-requests")]
        public async Task<IActionResult> AddApplyRequest(int userId)
        {
            var userBaseInfo = await _userService.GetBaseUserInfoAsync(userId);
            if (userBaseInfo == null)
                throw new Exception("用户参数错误。");

            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest
            {
                ApplierId = userIdentity.UserId,
                UserId = userId,
                Name = userBaseInfo.Name,
                Company = userBaseInfo.Company,
                Title = userBaseInfo.Title,
                Avatar = userBaseInfo.Avatar,
                CreationTime = DateTime.Now
            });

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut]
        [Route("apply-requests")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId)
        {
            var result = await _contactApplyRequestRepository.ApprovalAsync(applierId);
            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }


    }
}
