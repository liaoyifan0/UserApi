﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactApi.Data;
using ContactApi.Models;
using ContactApi.Service;
using ContactApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ContactApi.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactController : BaseController
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
        private readonly IUserService _userService;
        private readonly IOptions<MongoSettings> _settings;

        public ContactController(
            IContactRepository contactRepository,
            IContactApplyRequestRepository contactApplyRequestRepository,
            IUserService userService,
            IOptions<MongoSettings> settings
        )
        {
            _contactRepository = contactRepository;
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
            _settings = settings;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(UserIdentity.UserId, cancellationToken));
        }

        [HttpPut]
        [Route("tag")]
        public async Task<IActionResult> TagContact([FromBody]TagContactInputModel input, CancellationToken cancellationToken)
        {
            var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, input.ContactId, input.Tags, cancellationToken);
            if (result)
            {
                return Ok();
            }

            // Log TBD
            return BadRequest();
        }

        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            var userBaseInfo = await _userService.GetBaseUserInfoAsync(userId);
            if (userBaseInfo == null)
                throw new Exception("用户参数错误。");

            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest
            {
                ApplierId = UserIdentity.UserId,
                UserId = userId,
                Name = userBaseInfo.Name,
                Company = userBaseInfo.Company,
                Title = userBaseInfo.Title,
                Avatar = userBaseInfo.Avatar,
                CreationTime = DateTime.Now
            }, cancellationToken);

            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut]
        [Route("apply-requests/{applierId}")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);
            if (!result)
            {
                return BadRequest();
            }

            //Mongo暂时不支持事务
            var applier = await _userService.GetBaseUserInfoAsync(applierId);
            var user = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);
            await _contactRepository.AddContactAsync(UserIdentity.UserId, applier, cancellationToken);
            await _contactRepository.AddContactAsync(applierId, user, cancellationToken);

            return Ok();
        }


    }
}
