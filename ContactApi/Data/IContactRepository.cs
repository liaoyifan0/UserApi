using ContactApi.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContactApi.Data
{
    public interface IContactRepository
    {

        Task<bool> UpdateContactInfoAsync(BaseUserInfo userInfo, CancellationToken cancellationToken);

        Task<bool> AddContactAsync(int userId, BaseUserInfo userInfo, CancellationToken cancellationToken);
    }
}
