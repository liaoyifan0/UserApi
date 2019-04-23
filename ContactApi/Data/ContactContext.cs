using ContactApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Data
{
    public class ContactContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<ContactBook> _collection;
        private readonly MongoSettings _appSettings;

        public ContactContext(
            IOptions<MongoSettings> setting
            )
        {
            _appSettings = setting.Value;
            var client = new MongoClient(_appSettings.ConnectionString);

            if (client != null)
            {
                _database = client.GetDatabase(_appSettings.Database);
            }
        }

        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                CheckAndCreateCollection("ContactBooks");
                return _database.GetCollection<ContactBook>("ContactBooks");
            }
        }

        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                CheckAndCreateCollection("ContactApplyRequests");
                return _database.GetCollection<ContactApplyRequest>("ContactApplyRequests");
            }
        }

        private void CheckAndCreateCollection(string collectionName)
        {
            var collections = _database.ListCollectionNames().ToList();
            if (!collections.Contains(collectionName))
            {
                _database.CreateCollection(collectionName);
            }
 
        }
    }
}
