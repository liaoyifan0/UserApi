using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserApi2.Data;
using Microsoft.AspNetCore.JsonPatch;
using UserApi2.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UserApi2.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly UserContext _context;
        private readonly ILogger<UserController> _logger;


        public UserController(UserContext userContext, ILogger<UserController> logger)
        {
            _context = userContext;
            _logger = logger;
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Properties)
                .FirstOrDefaultAsync(U => U.Id == userIdentity.UserId);

            if(user == null)
            {
                throw new UserOperationException($"错误的上下文用户ID {userIdentity.UserId}");
            }

            return new JsonResult(user);
        }

        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch(JsonPatchDocument<User> patchUser)
        {
            User user = await _context.Users.Include(u => u.Properties).FirstOrDefaultAsync(u => u.Id == userIdentity.UserId);
            patchUser.ApplyTo(user);

            await _context.SaveChangesAsync();

            return new JsonResult(user);
        }

        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate([FromForm]string phone)
        {
            // todo: 验证phone格式

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Phone == phone);
            if(user == null)
            {
                user = new User() { Phone = phone };
                await _context.Users.AddAsync(user);

                await _context.SaveChangesAsync();
                user = await _context.Users.SingleOrDefaultAsync(u => u.Phone == phone);
            }

            return Ok(new {
                user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }

        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _context.UserTags.Where(u => u.UserId == userIdentity.UserId).ToListAsync());
        }

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdentity.UserId));
        }

        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody]List<string> tags)
        {
            var originTags =  await _context.UserTags.Where(u => u.UserId == userIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(i => i.Tag));

            await _context.UserTags.AddRangeAsync(newTags.Select(i => new UserTag
            {
                UserId = userIdentity.UserId,
                CreatedTime = DateTime.Now,
                Tag = i
            }));

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
