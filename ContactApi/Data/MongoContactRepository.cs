using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactApi.Dtos;
using ContactApi.Models;
using MongoDB.Driver;

namespace ContactApi.Data
{
    public class MongoContactRepository : IContactRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext context)
        {
            _contactContext = context;
        }

        public async Task<bool> AddContactAsync(int userId, BaseUserInfo userInfo, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactBook>.Filter.Eq(i => i.UserId, userId);
            if(await _contactContext.ContactBooks.CountDocumentsAsync(filter) == 0)
            {
                var contactBook = new ContactBook
                {
                    UserId = userId
                };
                await _contactContext.ContactBooks.InsertOneAsync(contactBook);
            }

            var contact = new Contact
            {
                Avatar = userInfo.Avatar,
                Company = userInfo.Company,
                Name = userInfo.Name,
                Title = userInfo.Title,
                UserId = userInfo.UserId
            };
            var update = Builders<ContactBook>.Update.AddToSet(i => i.Contacts, contact);

            var updateResult = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
            return CheckUpdateResultHelper.CheckUpdateOneSuccessfully(updateResult);
        }

        public async Task<List<Contact>> GetContactAsync(int userId, CancellationToken cancellationToken)
        {
            var contactBook = await (await _contactContext.ContactBooks.FindAsync(i => i.UserId == userId, null, cancellationToken)).FirstOrDefaultAsync();
            if (contactBook == null)
            {
                // Log TBD

                return null;
            }

            return contactBook.Contacts.ToList();
        }

        public async Task<bool> TagContactAsync(int contactId, int userId, List<string> tags, CancellationToken cancellationToken)
        {
            var filter = Builders<ContactBook>.Filter.And(
                    Builders<ContactBook>.Filter.Eq(c => c.UserId, userId),
                    Builders<ContactBook>.Filter.Eq("Contacts.UserId", contactId)
                );

            var update = Builders<ContactBook>.Update
                    .Set("Contacts.$.Tags", tags);
            var updateResult = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);

            return CheckUpdateResultHelper.CheckUpdateOneSuccessfully(updateResult);
        }

        public async Task<bool> UpdateContactInfoAsync(BaseUserInfo userInfo, CancellationToken cancellationToken)
        {
            var contactBooks = await _contactContext.ContactBooks.FindAsync(i => i.UserId == userInfo.UserId, null, cancellationToken); //只能过滤出当前查询用户的ContactBook
            var contactBook = await contactBooks.FirstOrDefaultAsync();

            var contactIds = contactBook.Contacts.Select(i => i.UserId);

            var filter = Builders<ContactBook>.Filter.And(
                        Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),  
                        Builders<ContactBook>.Filter.ElemMatch(c => c.Contacts, contact => contact.UserId == userInfo.UserId)
                        //CantactBook对应多个Contact，in filter可以过滤出所有与当前查询用户为好友的人的ContactBook
                    );

            var update = Builders<ContactBook>.Update
                    .Set("Contacts.$.Name", userInfo.Name)
                    .Set("Contacts.$.Avatar", userInfo.Avatar)
                    .Set("Contacts.$.Company", userInfo.Company)
                    .Set("Contacts.$.Title", userInfo.Title);

            var updateResult = _contactContext.ContactBooks.UpdateMany(filter, update);

            return CheckUpdateResultHelper.CheckUpdateSuccessfully(updateResult);
        }
    }
}
