using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactApi.Models;
using MongoDB.Driver;

namespace ContactApi.Data
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactApplyRequestRepository(ContactContext context)
        {
            _contactContext = context;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.ApplierId == request.ApplierId && r.UserId == request.UserId);
            if(await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update
                    .Set(i => i.CreationTime, DateTime.Now);

                var updateResult = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update);
                return CheckUpdateResultHelper.CheckUpdateOneSuccessfully(updateResult);
            }

            await _contactContext.ContactApplyRequests.InsertOneAsync(request, null, cancellationToken);
            return true;
        }

        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.ApplierId == applierId && r.UserId == userId);

            var update = Builders<ContactApplyRequest>.Update.Set(r => r.HandledTime, DateTime.Now).Set(r => r.Approvaled, 1);

            var updateResult = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);

            return CheckUpdateResultHelper.CheckUpdateOneSuccessfully(updateResult);
        }

        public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken)
        {
            return (await _contactContext.ContactApplyRequests.FindAsync(i => i.UserId == userId, null, cancellationToken)).ToList(); 
        }
    }
}
