using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactApi.Models;

namespace ContactApi.Data
{
    public interface IContactApplyRequestRepository
    {

        Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken);

        Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken);
            
        Task<List<ContactApplyRequest>> GetRequestListAsync(int UserId, CancellationToken cancellationToken);
    }
}
