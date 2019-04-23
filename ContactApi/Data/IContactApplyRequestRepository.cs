using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactApi.Models;

namespace ContactApi.Data
{
    public interface IContactApplyRequestRepository
    {

        Task<bool> AddRequestAsync(ContactApplyRequest request);

        Task<bool> ApprovalAsync(int applierId);
            
        Task<List<ContactApplyRequest>> GetRequestListAsync(int UserId);
    }
}
