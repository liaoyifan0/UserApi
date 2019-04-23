using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Models;

namespace ContactApi.Data
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
    {
        public Task<bool> AddRequestAsync(ContactApplyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApprovalAsync(int applierId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContactApplyRequest>> GetRequestListAsync(int UserId)
        {
            throw new NotImplementedException();
        }
    }
}
